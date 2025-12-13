using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Exports;
using CusomMapOSM_Application.Interfaces.Features.Usage;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using CusomMapOSM_Application.Interfaces.Services.LayerData;
using CusomMapOSM_Application.Interfaces.Services.MapFeatures;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Exports;
using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Domain.Entities.Exports.Enums;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Exports;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using CusomMapOSM_Infrastructure.Services;
using DomainMembership = CusomMapOSM_Domain.Entities.Memberships.Membership;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Optional;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Features.Exports;

// Helper class for SVG path data
internal class SvgPathInfo
{
    public string Type { get; set; } = string.Empty; // "path", "circle", "polygon", "polyline"
    public string Data { get; set; } = string.Empty; // Path data, circle coords, or polygon points
    public string Fill { get; set; } = "#3388ff";
    public string Stroke { get; set; } = "#3388ff";
    public string StrokeWidth { get; set; } = "2";
    public string FillOpacity { get; set; } = "0.2";
    public string StrokeOpacity { get; set; } = "1";
    public string? Transform { get; set; }
}

public class ExportService : IExportService
{
    private readonly IExportRepository _exportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapRepository _mapRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IFirebaseStorageService _firebaseStorageService;
    private readonly IExportQuotaService _exportQuotaService;
    private readonly IUsageService _usageService;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMapFeatureStore _mapFeatureStore;
    private readonly ILayerDataStore _layerDataStore;
    private readonly ILogger<ExportService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ExportService(
        IExportRepository exportRepository,
        ICurrentUserService currentUserService,
        IMapRepository mapRepository,
        IMembershipRepository membershipRepository,
        IWorkspaceRepository workspaceRepository,
        IFirebaseStorageService firebaseStorageService,
        IExportQuotaService exportQuotaService,
        IUsageService usageService,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        IMapFeatureStore mapFeatureStore,
        ILayerDataStore layerDataStore,
        ILogger<ExportService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _exportRepository = exportRepository;
        _currentUserService = currentUserService;
        _mapRepository = mapRepository;
        _membershipRepository = membershipRepository;
        _workspaceRepository = workspaceRepository;
        _firebaseStorageService = firebaseStorageService;
        _exportQuotaService = exportQuotaService;
        _usageService = usageService;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _mapFeatureStore = mapFeatureStore;
        _layerDataStore = layerDataStore;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Option<ExportResponse, Error>> CreateExportAsync(CreateExportRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<ExportResponse, Error>(
                    Error.Unauthorized("Export.Unauthorized", "User must be authenticated"));
            }

            // Validate map exists
            var map = await _mapRepository.GetMapById(request.MapId);
            if (map == null || !map.IsActive)
            {
                return Option.None<ExportResponse, Error>(
                    Error.NotFound("Export.MapNotFound", "Map not found or is not active"));
            }

            // Get user's active membership
            DomainMembership? membership = null;
            Guid? orgId = null;
            
            if (request.MembershipId.HasValue)
            {
                membership = await _membershipRepository.GetByIdAsync(request.MembershipId.Value, CancellationToken.None);
                if (membership != null)
                {
                    orgId = membership.OrgId;
                }
            }
            else
            {
                // Try to get membership from map's workspace organization
                if (map.WorkspaceId.HasValue)
                {
                    // Get workspace to find organization
                    var workspace = await _workspaceRepository.GetByIdAsync(map.WorkspaceId.Value);
                    if (workspace != null && workspace.OrgId.HasValue)
                    {
                        orgId = workspace.OrgId.Value;
                        membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(
                            currentUserId.Value,
                            workspace.OrgId.Value,
                            CancellationToken.None);
                    }
                }
            }

            // Validate membership exists and is active
            if (membership == null)
            {
                return Option.None<ExportResponse, Error>(
                    Error.NotFound("Export.MembershipNotFound", 
                        "Active membership not found. Please provide a membershipId or ensure you have an active membership for the map's organization."));
            }

            if (membership.Status != MembershipStatusEnum.Active)
            {
                return Option.None<ExportResponse, Error>(
                    Error.ValidationError("Export.MembershipInactive", 
                        $"Membership is not active. Current status: {membership.Status}. Please activate your membership to create exports."));
            }

            // Ensure membership includes Plan data
            if (membership.Plan == null)
            {
                // Reload membership with includes if Plan is missing
                membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(
                    currentUserId.Value,
                    membership.OrgId,
                    CancellationToken.None);
                
                if (membership?.Plan == null)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.Failure("Export.MembershipPlanNotFound", 
                            "Membership plan information not found. Please contact support."));
                }
            }

            // Check count-based export quota before creating export
            if (orgId.HasValue)
            {
                var quotaCheck = await _usageService.CheckUserQuotaAsync(
                    currentUserId.Value, 
                    orgId.Value, 
                    "exports", 
                    1, 
                    CancellationToken.None);
                
                if (!quotaCheck.HasValue)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.Failure("Export.QuotaCheckFailed", 
                            "Failed to check export quota. Please try again."));
                }

                var quotaResult = quotaCheck.Match(
                    some: quota => quota,
                    none: error => throw new InvalidOperationException($"Quota check failed: {error.Description}")
                );

                if (!quotaResult.IsAllowed)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.ValidationError("Export.QuotaExceeded", 
                            $"Export quota exceeded. {quotaResult.Message} Please upgrade your membership or wait for quota reset."));
                }
            }

            // Determine file extension and folder
            var (fileExtension, folder) = GetFileInfo(request.Format);
            var fileName = $"map_{request.MapId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";

            // Create export record with Pending status
            // Use empty string for FilePath initially - will be set to full Firebase URL after upload
            var export = new Export
            {
                UserId = currentUserId.Value,
                MembershipId = membership.MembershipId, // Now guaranteed to be non-null
                MapId = request.MapId,
                FilePath = string.Empty, // Will be updated with full Firebase URL after upload
                FileSize = 0, // Will be updated after file generation
                ExportTypeId = Guid.NewGuid(), // TODO: Map to actual export type table if exists
                ExportType = request.Format,
                Status = ExportStatusEnum.Pending,
                QuotaType = "included",
                CreatedAt = DateTime.UtcNow
            };

            var createdExport = await _exportRepository.CreateAsync(export);

            // Consume export quota immediately when export is created (not waiting for approval)
            // This ensures the quota is reserved even while waiting for admin approval
            if (orgId.HasValue)
            {
                var consumeQuotaResult = await _usageService.ConsumeUserQuotaAsync(
                    currentUserId.Value,
                    orgId.Value,
                    "exports",
                    1,
                    CancellationToken.None);
                
                if (!consumeQuotaResult.HasValue)
                {
                    _logger.LogWarning("Failed to consume export quota for export {ExportId}, user {UserId}, org {OrgId}. Export created but quota not tracked.", 
                        createdExport.ExportId, currentUserId.Value, orgId.Value);
                    // Note: We don't fail the export creation here since the export record is already created
                    // This is a tracking issue, but the export should still be processable
                }
                else
                {
                    var consumed = consumeQuotaResult.Match(
                        some: result => result,
                        none: error => false
                    );
                    
                    if (!consumed)
                    {
                        _logger.LogWarning("Quota consumption failed for export {ExportId}, user {UserId}, org {OrgId}. Export created but quota not consumed.", 
                            createdExport.ExportId, currentUserId.Value, orgId.Value);
                    }
                    else
                    {
                        _logger.LogInformation("Consumed export quota: incremented ExportsThisCycle for user {UserId}, org {OrgId}, export {ExportId} (status: {Status})", 
                            currentUserId.Value, orgId.Value, createdExport.ExportId, createdExport.Status);
                    }
                }
            }

            // Store export configuration to pass to background processing
            var exportConfig = new
            {
                ViewState = request.ViewState,
                MapImageData = request.MapImageData,
                SvgPathData = request.SvgPathData,
                VisibleLayerIds = request.VisibleLayerIds,
                VisibleFeatureIds = request.VisibleFeatureIds,
                Options = request.Options
            };

            // For GeoJSON, process synchronously so we can return the approved status and file URL immediately
            // For PNG/PDF, process asynchronously in background since they require admin approval
            if (request.Format == ExportTypeEnum.GeoJSON)
            {
                try
                {
                    // Process GeoJSON synchronously
                    await ProcessExportAsyncInternal(createdExport.ExportId, request.Format, map, exportConfig);
                    
                    // Refresh the export to get the updated status and file URL
                    var updatedExport = await _exportRepository.GetByIdWithIncludesAsync(createdExport.ExportId);
                    if (updatedExport != null)
                    {
                        return Option.Some<ExportResponse, Error>(MapToDto(updatedExport));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing GeoJSON export {ExportId} synchronously", createdExport.ExportId);
                    await UpdateExportStatusAsync(createdExport.ExportId, ExportStatusEnum.Failed, 
                        $"Export processing failed: {ex.Message}");
                }
            }
            else
            {
                // Process PNG/PDF asynchronously in a background scope to avoid DbContext disposal issues
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (_serviceScopeFactory == null)
                        {
                            _logger.LogError("ServiceScopeFactory is null. Cannot process export {ExportId} in background.", createdExport.ExportId);
                            await UpdateExportStatusAsync(createdExport.ExportId, ExportStatusEnum.Failed, 
                                "ServiceScopeFactory is not available for background processing");
                            return;
                        }

                        using var scope = _serviceScopeFactory.CreateScope();
                        var scopedExportService = scope.ServiceProvider.GetRequiredService<IExportService>();
                        var scopedMapRepository = scope.ServiceProvider.GetRequiredService<IMapRepository>();
                        
                        // Get fresh map instance from new scope
                        var scopedMap = await scopedMapRepository.GetMapById(map.MapId);
                        if (scopedMap == null)
                        {
                            _logger.LogError("Map {MapId} not found in background processing scope", map.MapId);
                            await UpdateExportStatusAsync(createdExport.ExportId, ExportStatusEnum.Failed, 
                                "Map not found in background processing");
                            return;
                        }
                        
                        // If view state is provided, update the map's view state temporarily for export
                        if (!string.IsNullOrEmpty(exportConfig.ViewState))
                        {
                            scopedMap.ViewState = exportConfig.ViewState;
                            _logger.LogInformation("Using provided view state for export {ExportId}: {ViewState}", 
                                createdExport.ExportId, exportConfig.ViewState);
                        }
                        
                        // Process using scoped services with export configuration
                        await ((ExportService)scopedExportService).ProcessExportAsyncInternal(
                            createdExport.ExportId, request.Format, scopedMap, exportConfig);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in background export processing for export {ExportId}", createdExport.ExportId);
                        try
                        {
                            await UpdateExportStatusAsync(createdExport.ExportId, ExportStatusEnum.Failed, 
                                $"Background processing failed: {ex.Message}");
                        }
                        catch
                        {
                            // If we can't update status, at least log it
                            _logger.LogError("Failed to update export status after background processing error");
                        }
                    }
                });
            }

            // Return the export (for GeoJSON it will have Approved status and fileUrl, for PNG/PDF it will be Pending)
            var finalExport = await _exportRepository.GetByIdWithIncludesAsync(createdExport.ExportId);
            return Option.Some<ExportResponse, Error>(MapToDto(finalExport ?? createdExport));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating export for map {MapId}", request.MapId);
            return Option.None<ExportResponse, Error>(
                Error.Failure("Export.CreateFailed", $"Failed to create export: {ex.Message}"));
        }
    }

    public async Task<Option<ExportResponse, Error>> GetExportByIdAsync(int exportId)
    {
        try
        {
            var export = await _exportRepository.GetByIdWithIncludesAsync(exportId);
            if (export == null)
            {
                return Option.None<ExportResponse, Error>(
                    Error.NotFound("Export.NotFound", "Export not found"));
            }

            return Option.Some<ExportResponse, Error>(MapToDto(export));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting export {ExportId}", exportId);
            return Option.None<ExportResponse, Error>(
                Error.Failure("Export.GetFailed", $"Failed to get export: {ex.Message}"));
        }
    }

    public async Task<Option<ExportListResponse, Error>> GetMyExportsAsync()
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<ExportListResponse, Error>(
                    Error.Unauthorized("Export.Unauthorized", "User must be authenticated"));
            }

            var exports = await _exportRepository.GetByUserIdAsync(currentUserId.Value);
            var exportDtos = exports.Select(e => MapToDto(e)).ToList();

            return Option.Some<ExportListResponse, Error>(new ExportListResponse
            {
                Exports = exportDtos,
                Total = exportDtos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exports for user");
            return Option.None<ExportListResponse, Error>(
                Error.Failure("Export.GetFailed", $"Failed to get exports: {ex.Message}"));
        }
    }

    public async Task<Option<ExportListResponse, Error>> GetExportsByMapIdAsync(Guid mapId)
    {
        try
        {
            var exports = await _exportRepository.GetByMapIdAsync(mapId);
            var exportDtos = exports.Select(e => MapToDto(e)).ToList();

            return Option.Some<ExportListResponse, Error>(new ExportListResponse
            {
                Exports = exportDtos,
                Total = exportDtos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exports for map {MapId}", mapId);
            return Option.None<ExportListResponse, Error>(
                Error.Failure("Export.GetFailed", $"Failed to get exports: {ex.Message}"));
        }
    }

    public async Task<Option<ExportListResponse, Error>> GetExportsByOrganizationIdAsync(Guid organizationId)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<ExportListResponse, Error>(
                    Error.Unauthorized("Export.Unauthorized", "User must be authenticated"));
            }

            // Verify user is a member of the organization
            var membership = await _membershipRepository.GetByUserOrgAsync(currentUserId.Value, organizationId, CancellationToken.None);
            if (membership == null)
            {
                return Option.None<ExportListResponse, Error>(
                    Error.NotFound("Membership.NotFound", "User is not a member of this organization"));
            }

            // Check membership status is Active
            if (membership.Status != MembershipStatusEnum.Active)
            {
                return Option.None<ExportListResponse, Error>(
                    Error.Forbidden("Membership.Inactive", "Membership is not active"));
            }

            // Get all exports for the organization
            var exports = await _exportRepository.GetByOrganizationIdAsync(organizationId);
            var exportDtos = exports.Select(e => MapToDto(e)).ToList();

            return Option.Some<ExportListResponse, Error>(new ExportListResponse
            {
                Exports = exportDtos,
                Total = exportDtos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exports for organization {OrganizationId}", organizationId);
            return Option.None<ExportListResponse, Error>(
                Error.Failure("Export.GetFailed", $"Failed to get exports: {ex.Message}"));
        }
    }

    public async Task ProcessPendingExportsAsync()
    {
        try
        {
            var pendingExports = await _exportRepository.GetPendingExportsAsync();
            
            foreach (var export in pendingExports)
            {
                try
                {
                    var map = await _mapRepository.GetMapById(export.MapId);
                    if (map == null)
                    {
                        await UpdateExportStatusAsync(export.ExportId, ExportStatusEnum.Failed, 
                            "Map not found");
                        continue;
                    }

                    await ProcessExportAsync(export.ExportId, export.ExportType, map);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing export {ExportId}", export.ExportId);
                    await UpdateExportStatusAsync(export.ExportId, ExportStatusEnum.Failed, 
                        ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending exports");
        }
    }

    // Internal method that can be called from background tasks with scoped services
    internal async Task ProcessExportAsyncInternal(int exportId, ExportTypeEnum format, CusomMapOSM_Domain.Entities.Maps.Map map, object? exportConfig = null)
    {
        await ProcessExportAsync(exportId, format, map, exportConfig);
    }

    private async Task ProcessExportAsync(int exportId, ExportTypeEnum format, CusomMapOSM_Domain.Entities.Maps.Map map, object? exportConfig = null)
    {
        try
        {
            var export = await _exportRepository.GetByIdAsync(exportId);
            if (export == null)
            {
                _logger.LogWarning("Export {ExportId} not found for processing", exportId);
                return;
            }

            // Update status to Processing
            export.Status = ExportStatusEnum.Processing;
            await _exportRepository.UpdateAsync(export);

            _logger.LogInformation("Starting export generation for map {MapId}, format: {Format}", map.MapId, format);

            // Extract export configuration if provided
            Dictionary<string, bool>? visibleLayerIds = null;
            Dictionary<string, bool>? visibleFeatureIds = null;
            ExportOptions? options = null;
            string? mapImageData = null;
            string? svgPathData = null;
            
            if (exportConfig != null)
            {
                try
                {
                    var configJson = JsonSerializer.Serialize(exportConfig);
                    var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);
                    
                    if (configDict != null)
                    {
                        if (configDict.ContainsKey("MapImageData") && configDict["MapImageData"] != null)
                        {
                            mapImageData = configDict["MapImageData"]?.ToString();
                        }
                        
                        if (configDict.ContainsKey("SvgPathData") && configDict["SvgPathData"] != null)
                        {
                            svgPathData = configDict["SvgPathData"]?.ToString();
                        }
                        
                        if (configDict.ContainsKey("VisibleLayerIds") && configDict["VisibleLayerIds"] != null)
                        {
                            var layerDictJson = configDict["VisibleLayerIds"].ToString();
                            if (!string.IsNullOrEmpty(layerDictJson))
                            {
                                visibleLayerIds = JsonSerializer.Deserialize<Dictionary<string, bool>>(layerDictJson);
                            }
                        }
                        
                        if (configDict.ContainsKey("VisibleFeatureIds") && configDict["VisibleFeatureIds"] != null)
                        {
                            var featureDictJson = configDict["VisibleFeatureIds"].ToString();
                            if (!string.IsNullOrEmpty(featureDictJson))
                            {
                                visibleFeatureIds = JsonSerializer.Deserialize<Dictionary<string, bool>>(featureDictJson);
                            }
                        }
                        
                        if (configDict.ContainsKey("Options") && configDict["Options"] != null)
                        {
                            var optionsJson = configDict["Options"].ToString();
                            if (!string.IsNullOrEmpty(optionsJson))
                            {
                                options = JsonSerializer.Deserialize<ExportOptions>(optionsJson);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse export configuration, using defaults");
                }
            }

            // Generate export file based on format
            byte[] fileData;
            string fileExtension;
            string folder;

            switch (format)
            {
                case ExportTypeEnum.PDF:
                    fileData = await GeneratePdfAsync(map, visibleLayerIds, visibleFeatureIds, options, mapImageData);
                    fileExtension = "pdf";
                    folder = "exports/pdf";
                    break;
                case ExportTypeEnum.PNG:
                    fileData = await GeneratePngAsync(map, visibleLayerIds, visibleFeatureIds, options, mapImageData);
                    fileExtension = "png";
                    folder = "exports/png";
                    break;
                case ExportTypeEnum.SVG:
                    fileData = await GenerateSvgAsync(map, visibleLayerIds, visibleFeatureIds, options, mapImageData, svgPathData);
                    fileExtension = "svg";
                    folder = "exports/svg";
                    break;
                case ExportTypeEnum.GeoJSON:
                    fileData = await GenerateGeoJsonAsync(map, visibleLayerIds, visibleFeatureIds);
                    fileExtension = "geojson";
                    folder = "exports/geojson";
                    break;
                default:
                    throw new NotSupportedException($"Export format {format} is not supported");
            }

            _logger.LogInformation("Export file generated. Size: {Size} bytes", fileData.Length);

            // Check quota before uploading
            var fileSizeKB = (int)Math.Ceiling(fileData.Length / 1024.0);
            var fileType = ExportQuotaExtensions.GetFileTypeFromExtension($".{fileExtension}");
            
            if (!await _exportQuotaService.CanExportAsync(export.UserId, fileType, fileSizeKB))
            {
                await UpdateExportStatusAsync(exportId, ExportStatusEnum.Failed, 
                    "Export quota exceeded");
                return;
            }

            // Upload to Firebase Storage
            var fileName = $"map_{map.MapId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";
            using var fileStream = new MemoryStream(fileData);
            var fileUrl = await _firebaseStorageService.UploadFileAsync(fileName, fileStream, folder);

            // Ensure we have a full URL (not just a path)
            if (string.IsNullOrEmpty(fileUrl) || (!fileUrl.StartsWith("http://") && !fileUrl.StartsWith("https://")))
            {
                _logger.LogError("UploadFileAsync returned invalid URL: {FileUrl} for export {ExportId}", fileUrl, exportId);
                await UpdateExportStatusAsync(exportId, ExportStatusEnum.Failed, 
                    "Failed to upload file to Firebase Storage - invalid URL returned");
                return;
            }

            // Note: Export count quota was already consumed when export was created (in CreateExportAsync)
            // Token quota will be consumed when admin approves the export (in ApproveExportAsync)
            // We check token quota here to ensure user has enough tokens before processing

            // GeoJSON exports are auto-approved (no admin approval needed)
            // PNG and PDF exports require admin approval
            var isGeoJson = format == ExportTypeEnum.GeoJSON;
            
            // Update export record
            // Store the full Firebase URL
            export.FilePath = fileUrl;
            export.FileSize = fileData.Length;
            export.CompletedAt = DateTime.UtcNow;
            
            if (isGeoJson)
            {
                // Auto-approve GeoJSON exports
                export.Status = ExportStatusEnum.Approved;
                _logger.LogInformation("Export {ExportId} (GeoJSON) file uploaded to Firebase: {FileUrl}. File size: {FileSize}KB. Auto-approved (no admin approval required).", 
                    exportId, fileUrl, fileSizeKB);
            }
            else
            {
                // PNG and PDF require admin approval
                export.Status = ExportStatusEnum.PendingApproval;
                _logger.LogInformation("Export {ExportId} file uploaded to Firebase: {FileUrl}. File size: {FileSize}KB. Waiting for admin approval before consuming quota tokens.", 
                    exportId, fileUrl, fileSizeKB);
            }
            
            await _exportRepository.UpdateAsync(export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing export {ExportId}", exportId);
            await UpdateExportStatusAsync(exportId, ExportStatusEnum.Failed, ex.Message);
        }
    }

    private async Task UpdateExportStatusAsync(int exportId, ExportStatusEnum status, string? errorMessage = null)
    {
        var export = await _exportRepository.GetByIdAsync(exportId);
        if (export != null)
        {
            var previousStatus = export.Status;
            export.Status = status;
            export.ErrorMessage = errorMessage;
            if (status == ExportStatusEnum.PendingApproval || status == ExportStatusEnum.Approved)
            {
                export.CompletedAt = DateTime.UtcNow;
            }
            
            // If status is changing to Failed, and previous status was one where quota was consumed, refund the quota
            if (status == ExportStatusEnum.Failed && 
                (previousStatus == ExportStatusEnum.Pending || 
                 previousStatus == ExportStatusEnum.PendingApproval || 
                 previousStatus == ExportStatusEnum.Processing))
            {
                // Refund export quota when export fails
                await RefundExportQuotaAsync(exportId, export);
            }
            
            await _exportRepository.UpdateAsync(export);
        }
    }

    private async Task RefundExportQuotaAsync(int exportId, Export export)
    {
        try
        {
            // Get membership to find organization ID
            DomainMembership? membership = null;
            Guid? orgId = null;
            
            if (export.MembershipId != Guid.Empty)
            {
                membership = await _membershipRepository.GetByIdAsync(export.MembershipId, CancellationToken.None);
                if (membership != null)
                {
                    orgId = membership.OrgId;
                }
            }
            
            // If membership not found by ID, try to get from map's workspace organization
            if (membership == null)
            {
                var map = await _mapRepository.GetMapById(export.MapId);
                if (map?.WorkspaceId.HasValue == true)
                {
                    var workspace = await _workspaceRepository.GetByIdAsync(map.WorkspaceId.Value);
                    if (workspace?.OrgId.HasValue == true)
                    {
                        orgId = workspace.OrgId.Value;
                        membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(
                            export.UserId,
                            workspace.OrgId.Value,
                            CancellationToken.None);
                    }
                }
            }

            // Refund quota by consuming -1 (decrementing)
            if (membership != null && orgId.HasValue)
            {
                var refundQuotaResult = await _usageService.ConsumeUserQuotaAsync(
                    export.UserId,
                    orgId.Value,
                    "exports",
                    -1, // Negative amount to refund
                    CancellationToken.None);
                
                if (!refundQuotaResult.HasValue)
                {
                    _logger.LogWarning("Failed to refund export quota for failed export {ExportId}, user {UserId}, org {OrgId}.", 
                        exportId, export.UserId, orgId.Value);
                }
                else
                {
                    var refunded = refundQuotaResult.Match(
                        some: result => result,
                        none: error => false
                    );
                    
                    if (!refunded)
                    {
                        _logger.LogWarning("Quota refund failed for failed export {ExportId}, user {UserId}, org {OrgId}.", 
                            exportId, export.UserId, orgId.Value);
                    }
                    else
                    {
                        _logger.LogInformation("Refunded export quota: decremented ExportsThisCycle for user {UserId}, org {OrgId}, export {ExportId} (failed)", 
                            export.UserId, orgId.Value, exportId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding export quota for export {ExportId}", exportId);
            // Don't throw - quota refund failure shouldn't prevent status update
        }
    }

    private static (string extension, string folder) GetFileInfo(ExportTypeEnum format)
    {
        return format switch
        {
            ExportTypeEnum.PDF => ("pdf", "exports/pdf"),
            ExportTypeEnum.PNG => ("png", "exports/png"),
            ExportTypeEnum.SVG => ("svg", "exports/svg"),
            ExportTypeEnum.GeoJSON => ("geojson", "exports/geojson"),
            ExportTypeEnum.MBTiles => ("mbtiles", "exports/mbtiles"),
            _ => ("pdf", "exports/pdf")
        };
    }

    // Export generation methods - fetch actual map data
    private async Task<byte[]> GeneratePdfAsync(
        CusomMapOSM_Domain.Entities.Maps.Map map,
        Dictionary<string, bool>? visibleLayerIds = null,
        Dictionary<string, bool>? visibleFeatureIds = null,
        ExportOptions? options = null,
        string? mapImageData = null)
    {
        // Generate PNG first (which will use captured image if available)
        var pngData = await GeneratePngAsync(map, visibleLayerIds, visibleFeatureIds, options, mapImageData);
        
        var mapData = await GetMapDataForExportAsync(map, visibleLayerIds, visibleFeatureIds);
        var title = options?.Title ?? map.MapName ?? "Map Export";
        var description = options?.Description ?? map.Description ?? "N/A";
        
        // Convert PNG to JPEG for PDF embedding (PDF supports JPEG natively)
        byte[] jpegData;
        int imageWidth, imageHeight;
        
        using (var pngStream = new MemoryStream(pngData))
        using (var bitmap = new Bitmap(pngStream))
        {
            imageWidth = bitmap.Width;
            imageHeight = bitmap.Height;
            
            // Convert to JPEG
            using (var jpegStream = new MemoryStream())
            {
                bitmap.Save(jpegStream, ImageFormat.Jpeg);
                jpegData = jpegStream.ToArray();
            }
        }
        
        // Create PDF with embedded JPEG image
        var pdfBytes = GeneratePdfWithJpegImage(jpegData, imageWidth, imageHeight, title, description, mapData.Layers.Count, mapData.Features.Count);
        
        return pdfBytes;
    }
    
    private byte[] GeneratePdfWithJpegImage(byte[] jpegData, int imageWidth, int imageHeight, string title, string description, int layerCount, int featureCount)
    {
        // Create a PDF that embeds the JPEG image
        // PDF natively supports JPEG with DCTDecode filter
        // Note: This is a simplified PDF generator. For production, use a proper PDF library.
        
        var imageLength = jpegData.Length;
        var pageWidth = 612; // 8.5 inches at 72 DPI
        var pageHeight = 792; // 11 inches at 72 DPI
        
        // Scale image to fit page while maintaining aspect ratio
        var scaleX = (double)(pageWidth - 100) / imageWidth;
        var scaleY = (double)(pageHeight - 150) / imageHeight;
        var scale = Math.Min(scaleX, scaleY);
        var scaledWidth = (int)(imageWidth * scale);
        var scaledHeight = (int)(imageHeight * scale);
        var imageX = (pageWidth - scaledWidth) / 2;
        var imageY = pageHeight - scaledHeight - 50;
        
        // Escape special characters in title for PDF text
        var escapedTitle = title.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        
        using var pdfStream = new MemoryStream();
        using var writer = new StreamWriter(pdfStream, System.Text.Encoding.ASCII);
        
        // PDF Header
        writer.WriteLine("%PDF-1.4");
        
        // Object 1: Catalog
        var catalogOffset = pdfStream.Position;
        writer.WriteLine("1 0 obj");
        writer.WriteLine("<<");
        writer.WriteLine("/Type /Catalog");
        writer.WriteLine("/Pages 2 0 R");
        writer.WriteLine(">>");
        writer.WriteLine("endobj");
        
        // Object 2: Pages
        var pagesOffset = pdfStream.Position;
        writer.WriteLine("2 0 obj");
        writer.WriteLine("<<");
        writer.WriteLine("/Type /Pages");
        writer.WriteLine("/Kids [3 0 R]");
        writer.WriteLine("/Count 1");
        writer.WriteLine(">>");
        writer.WriteLine("endobj");
        
        // Object 3: Page
        var pageOffset = pdfStream.Position;
        writer.WriteLine("3 0 obj");
        writer.WriteLine("<<");
        writer.WriteLine("/Type /Page");
        writer.WriteLine("/Parent 2 0 R");
        writer.WriteLine($"/MediaBox [0 0 {pageWidth} {pageHeight}]");
        writer.WriteLine("/Contents 4 0 R");
        writer.WriteLine("/Resources <<");
        writer.WriteLine("/XObject <<");
        writer.WriteLine("/Im1 5 0 R");
        writer.WriteLine(">>");
        writer.WriteLine("/Font <<");
        writer.WriteLine("/F1 6 0 R");
        writer.WriteLine(">>");
        writer.WriteLine(">>");
        writer.WriteLine(">>");
        writer.WriteLine("endobj");
        
        // Object 4: Contents
        var contentsStream = $@"q
{scaledWidth} 0 0 {scaledHeight} {imageX} {imageY} cm
/Im1 Do
Q
BT
/F1 18 Tf
50 {pageHeight - 30} Td
({escapedTitle}) Tj
0 -20 Td
/F1 10 Tf
(Layers: {layerCount} | Features: {featureCount}) Tj
ET";
        var contentsOffset = pdfStream.Position;
        writer.WriteLine("4 0 obj");
        writer.WriteLine("<<");
        writer.WriteLine($"/Length {System.Text.Encoding.ASCII.GetByteCount(contentsStream)}");
        writer.WriteLine(">>");
        writer.WriteLine("stream");
        writer.Write(contentsStream);
        writer.WriteLine();
        writer.WriteLine("endstream");
        writer.WriteLine("endobj");
        
        // Object 5: Image XObject
        var imageOffset = pdfStream.Position;
        writer.WriteLine("5 0 obj");
        writer.WriteLine("<<");
        writer.WriteLine("/Type /XObject");
        writer.WriteLine("/Subtype /Image");
        writer.WriteLine($"/Width {imageWidth}");
        writer.WriteLine($"/Height {imageHeight}");
        writer.WriteLine("/ColorSpace /DeviceRGB");
        writer.WriteLine("/BitsPerComponent 8");
        writer.WriteLine("/Filter /DCTDecode");
        writer.WriteLine($"/Length {imageLength}");
        writer.WriteLine(">>");
        writer.WriteLine("stream");
        writer.Flush();
        // Write JPEG bytes directly to stream (not as text)
        pdfStream.Write(jpegData, 0, jpegData.Length);
        // Continue writing text after binary data using direct stream writes
        var endStreamText = "\nendstream\nendobj\n";
        var endStreamBytes = System.Text.Encoding.ASCII.GetBytes(endStreamText);
        pdfStream.Write(endStreamBytes, 0, endStreamBytes.Length);
        
        // Object 6: Font
        var fontOffset = pdfStream.Position;
        var fontText = $@"6 0 obj
<<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica
>>
endobj
";
        var fontBytes = System.Text.Encoding.ASCII.GetBytes(fontText);
        pdfStream.Write(fontBytes, 0, fontBytes.Length);
        
        // XRef table
        var xrefOffset = pdfStream.Position;
        var xrefText = $@"xref
0 7
0000000000 65535 f 
{catalogOffset:D10} 00000 n 
{pagesOffset:D10} 00000 n 
{pageOffset:D10} 00000 n 
{contentsOffset:D10} 00000 n 
{imageOffset:D10} 00000 n 
{fontOffset:D10} 00000 n 
";
        var xrefBytes = System.Text.Encoding.ASCII.GetBytes(xrefText);
        pdfStream.Write(xrefBytes, 0, xrefBytes.Length);
        
        // Trailer
        var trailerText = $@"trailer
<<
/Size 7
/Root 1 0 R
>>
startxref
{xrefOffset}
%%EOF
";
        var trailerBytes = System.Text.Encoding.ASCII.GetBytes(trailerText);
        pdfStream.Write(trailerBytes, 0, trailerBytes.Length);
        
        return pdfStream.ToArray();
    }

    private async Task<byte[]> GeneratePngAsync(
        CusomMapOSM_Domain.Entities.Maps.Map map,
        Dictionary<string, bool>? visibleLayerIds = null,
        Dictionary<string, bool>? visibleFeatureIds = null,
        ExportOptions? options = null,
        string? mapImageData = null)
    {
        // If we have captured image data from frontend, use it directly
        if (!string.IsNullOrEmpty(mapImageData))
        {
            try
            {
                // Remove data URL prefix if present (e.g., "data:image/png;base64,")
                var base64Data = mapImageData;
                if (mapImageData.Contains(","))
                {
                    base64Data = mapImageData.Substring(mapImageData.IndexOf(",") + 1);
                }
                
                // Convert base64 to byte array
                var imageBytes = Convert.FromBase64String(base64Data);
                _logger.LogInformation("Using captured map image from frontend. Size: {Size} bytes", imageBytes.Length);
                return imageBytes;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse map image data, falling back to generated image");
            }
        }
        
        // Fallback: Generate placeholder image
        var mapData = await GetMapDataForExportAsync(map, visibleLayerIds, visibleFeatureIds);
        var title = options?.Title ?? map.MapName ?? "Map Export";
        var width = options?.Width ?? 1920;
        var height = options?.Height ?? 1080;
        
        // Create a bitmap image
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        
        // Set high quality rendering
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        
        // Fill background
        graphics.Clear(Color.White);
        
        // Draw title
        using var titleFont = new Font("Arial", 32, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.Black);
        var titleRect = new RectangleF(50, 50, width - 100, 100);
        graphics.DrawString(title, titleFont, titleBrush, titleRect);
        
        // Draw map information
        using var infoFont = new Font("Arial", 16, FontStyle.Regular);
        var infoY = 180;
        var lineHeight = 35;
        
        graphics.DrawString($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}", infoFont, titleBrush, 50, infoY);
        infoY += lineHeight;
        graphics.DrawString($"Layers: {mapData.Layers.Count}", infoFont, titleBrush, 50, infoY);
        infoY += lineHeight;
        graphics.DrawString($"Features: {mapData.Features.Count}", infoFont, titleBrush, 50, infoY);
        infoY += lineHeight;
        graphics.DrawString($"Base Layer: {map.BaseLayer}", infoFont, titleBrush, 50, infoY);
        
        // Draw a placeholder rectangle for the map area
        var mapAreaRect = new Rectangle(50, infoY + lineHeight, width - 100, height - infoY - lineHeight - 100);
        using var mapAreaBrush = new SolidBrush(Color.LightGray);
        using var mapAreaPen = new Pen(Color.Gray, 2);
        graphics.FillRectangle(mapAreaBrush, mapAreaRect);
        graphics.DrawRectangle(mapAreaPen, mapAreaRect);
        
        // Draw placeholder text in map area
        using var placeholderFont = new Font("Arial", 24, FontStyle.Italic);
        using var placeholderBrush = new SolidBrush(Color.Gray);
        var placeholderText = "Map visualization area\n(Full map rendering requires map rendering service)";
        var placeholderFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        graphics.DrawString(placeholderText, placeholderFont, placeholderBrush, mapAreaRect, placeholderFormat);
        
        // Convert to PNG bytes
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, ImageFormat.Png);
        return memoryStream.ToArray();
    }

    private async Task<byte[]> GenerateSvgAsync(
        CusomMapOSM_Domain.Entities.Maps.Map map,
        Dictionary<string, bool>? visibleLayerIds = null,
        Dictionary<string, bool>? visibleFeatureIds = null,
        ExportOptions? options = null,
        string? mapImageData = null,
        string? svgPathData = null)
    {
        var svgWidth = options?.Width ?? 1920;
        var svgHeight = options?.Height ?? 1080;
        
        // html2canvas captures at scale 2, so the actual image dimensions are 2x
        // The frontend sends coordinates already scaled by 2, so we need to use 2x dimensions for viewBox
        var imageScale = 2;
        var actualWidth = svgWidth * imageScale;
        var actualHeight = svgHeight * imageScale;
        
        // Parse SVG path data if provided
        List<SvgPathInfo>? svgPaths = null;
        if (!string.IsNullOrEmpty(svgPathData))
        {
            try
            {
                svgPaths = JsonSerializer.Deserialize<List<SvgPathInfo>>(svgPathData);
                _logger.LogInformation("Parsed {Count} SVG paths from frontend", svgPaths?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse SVG path data");
            }
        }
        
        // Build SVG content with image and paths
        // Use actualWidth/actualHeight for viewBox to match the scaled coordinates
        var svgBuilder = new System.Text.StringBuilder();
        svgBuilder.AppendLine($@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{actualWidth}"" height=""{actualHeight}"" viewBox=""0 0 {actualWidth} {actualHeight}"">");
        
        // Add background image if available
        // Image is already at 2x scale, so use actualWidth/actualHeight
        if (!string.IsNullOrEmpty(mapImageData))
        {
            svgBuilder.AppendLine($@"    <image href=""{mapImageData}"" width=""{actualWidth}"" height=""{actualHeight}"" />");
        }
        
        // Add SVG paths if available
        if (svgPaths != null && svgPaths.Count > 0)
        {
            foreach (var path in svgPaths)
            {
                var transformAttr = !string.IsNullOrEmpty(path.Transform) ? $@" transform=""{path.Transform}""" : "";
                
                switch (path.Type.ToLower())
                {
                    case "path":
                        svgBuilder.AppendLine($@"    <path d=""{path.Data}"" fill=""{path.Fill}"" stroke=""{path.Stroke}"" stroke-width=""{path.StrokeWidth}"" fill-opacity=""{path.FillOpacity}"" stroke-opacity=""{path.StrokeOpacity}""{transformAttr} />");
                        break;
                    case "circle":
                        var circleParts = path.Data.Split(',');
                        if (circleParts.Length >= 3)
                        {
                            svgBuilder.AppendLine($@"    <circle cx=""{circleParts[0]}"" cy=""{circleParts[1]}"" r=""{circleParts[2]}"" fill=""{path.Fill}"" stroke=""{path.Stroke}"" stroke-width=""{path.StrokeWidth}"" fill-opacity=""{path.FillOpacity}"" stroke-opacity=""{path.StrokeOpacity}""{transformAttr} />");
                        }
                        break;
                    case "polygon":
                        svgBuilder.AppendLine($@"    <polygon points=""{path.Data}"" fill=""{path.Fill}"" stroke=""{path.Stroke}"" stroke-width=""{path.StrokeWidth}"" fill-opacity=""{path.FillOpacity}"" stroke-opacity=""{path.StrokeOpacity}""{transformAttr} />");
                        break;
                    case "polyline":
                        svgBuilder.AppendLine($@"    <polyline points=""{path.Data}"" fill=""{path.Fill}"" stroke=""{path.Stroke}"" stroke-width=""{path.StrokeWidth}"" fill-opacity=""{path.FillOpacity}"" stroke-opacity=""{path.StrokeOpacity}""{transformAttr} />");
                        break;
                }
            }
        }
        
        svgBuilder.AppendLine("</svg>");
        
        var svgContent = svgBuilder.ToString();
        return System.Text.Encoding.UTF8.GetBytes(svgContent);
    }

    private async Task<byte[]> GenerateGeoJsonAsync(
        CusomMapOSM_Domain.Entities.Maps.Map map,
        Dictionary<string, bool>? visibleLayerIds = null,
        Dictionary<string, bool>? visibleFeatureIds = null)
    {
        // Generate actual GeoJSON from map data
        var mapData = await GetMapDataForExportAsync(map, visibleLayerIds, visibleFeatureIds);
        
        var features = new List<object>();
        // Track feature IDs to prevent duplicates (features can exist in both MongoDB and layer data)
        var addedFeatureIds = new HashSet<string>();
        
        // Add features from MongoDB (map annotations, markers, etc.)
        _logger.LogInformation("Processing {FeatureCount} features from MongoDB for GeoJSON export", mapData.Features.Count);
        
        foreach (var feature in mapData.Features)
        {
            try
            {
                // Skip Text annotation types as they don't display correctly in exports
                if (feature.AnnotationType != null && 
                    feature.AnnotationType.Equals("Text", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Skipping Text annotation feature {FeatureId} from export", feature.Id);
                    continue;
                }
                
                _logger.LogDebug("Processing feature {FeatureId}, Geometry: {HasGeometry}, GeometryType: {GeometryType}, GeometryTypeName: {GeometryTypeName}", 
                    feature.Id, feature.Geometry != null, feature.GeometryType, feature.Geometry?.GetType().Name);
                
                // Convert geometry to proper GeoJSON format
                // Geometry from MongoDB is stored as a JSON string (from BsonValue.ToJson())
                Dictionary<string, object>? geometryDict = null;
                
                if (feature.Geometry != null)
                {
                    try
                    {
                        // Geometry is stored as a JSON string from MongoDB
                        if (feature.Geometry is string geometryJsonString)
                        {
                            _logger.LogDebug("Feature {FeatureId} geometry is a JSON string, parsing: {Preview}", 
                                feature.Id, geometryJsonString.Length > 200 ? geometryJsonString.Substring(0, 200) + "..." : geometryJsonString);
                            
                            geometryDict = JsonSerializer.Deserialize<Dictionary<string, object>>(geometryJsonString);
                        }
                        else if (feature.Geometry is JsonElement jsonElement)
                        {
                            // Convert JsonElement to dictionary
                            geometryDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText());
                        }
                        else if (feature.Geometry is Dictionary<string, object> dict)
                        {
                            // Already a dictionary
                            geometryDict = dict;
                        }
                        else
                        {
                            // Try to serialize and deserialize
                            var geometryJson = JsonSerializer.Serialize(feature.Geometry);
                            geometryDict = JsonSerializer.Deserialize<Dictionary<string, object>>(geometryJson);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse geometry for feature {FeatureId}. Geometry type: {Type}, Value: {Value}", 
                            feature.Id, feature.Geometry?.GetType().Name, 
                            feature.Geometry is string str ? (str.Length > 200 ? str.Substring(0, 200) + "..." : str) : feature.Geometry?.ToString());
                        continue;
                    }
                }
                
                if (geometryDict == null)
                {
                    _logger.LogWarning("Feature {FeatureId} has null geometry after conversion, skipping", feature.Id);
                    continue;
                }
                
                // Ensure geometry is in GeoJSON format (must have type and coordinates)
                if (!geometryDict.ContainsKey("type") || !geometryDict.ContainsKey("coordinates"))
                {
                    _logger.LogWarning("Feature {FeatureId} geometry is not in valid GeoJSON format (missing type or coordinates), skipping. Geometry: {Geometry}", 
                        feature.Id, JsonSerializer.Serialize(geometryDict));
                    continue;
                }
                
                var geoJsonFeature = new
                {
                    type = "Feature",
                    id = feature.Id,
                    geometry = geometryDict,
                    properties = new Dictionary<string, object>
                    {
                        ["name"] = feature.Name ?? "",
                        ["featureCategory"] = feature.FeatureCategory,
                        ["annotationType"] = feature.AnnotationType ?? "",
                        ["geometryType"] = feature.GeometryType,
                        ["layerId"] = feature.LayerId?.ToString() ?? "",
                        ["createdBy"] = feature.CreatedBy.ToString(),
                        ["createdAt"] = feature.CreatedAt.ToString("O"),
                        ["zIndex"] = feature.ZIndex
                    }
                };
                
                // Merge additional properties if they exist
                if (feature.Properties != null)
                {
                    foreach (var prop in feature.Properties)
                    {
                        geoJsonFeature.properties[prop.Key] = prop.Value;
                    }
                }
                
                // Check for duplicates before adding
                if (addedFeatureIds.Contains(feature.Id))
                {
                    _logger.LogWarning("Skipping duplicate feature {FeatureId} from MongoDB (already added)", feature.Id);
                    continue;
                }
                
                features.Add(geoJsonFeature);
                addedFeatureIds.Add(feature.Id);
                _logger.LogDebug("Successfully added feature {FeatureId} to export", feature.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process feature {FeatureId} for GeoJSON export", feature.Id);
            }
        }
        
        _logger.LogInformation("Added {FeatureCount} features from MongoDB to GeoJSON export", features.Count);
        
        // Add features from layers (layer data contains GeoJSON)
        // Layer data might be stored in MongoDB (via DataStoreKey) or in relational DB (LayerData)
        _logger.LogInformation("Processing {LayerCount} layers for GeoJSON export", mapData.Layers.Count);
        
        foreach (var layer in mapData.Layers)
        {
            try
            {
                _logger.LogInformation("Processing layer {LayerId} ({LayerName}), DataStoreKey: {DataStoreKey}, HasLayerData: {HasLayerData}", 
                    layer.LayerId, layer.LayerName, layer.DataStoreKey ?? "null", !string.IsNullOrEmpty(layer.LayerData));
                
                // Get layer data using ILayerDataStore (handles both MongoDB and relational storage)
                var layerDataString = await _layerDataStore.GetDataAsync(layer);
                
                if (string.IsNullOrEmpty(layerDataString))
                {
                    _logger.LogWarning("Layer {LayerId} ({LayerName}) has no data after retrieval from store", layer.LayerId, layer.LayerName);
                    continue;
                }
                
                _logger.LogInformation("Retrieved layer data for {LayerId} ({LayerName}), data length: {DataLength}, preview: {Preview}", 
                    layer.LayerId, layer.LayerName, layerDataString.Length, 
                    layerDataString.Length > 200 ? layerDataString.Substring(0, 200) + "..." : layerDataString);
                
                var layerDataDoc = JsonDocument.Parse(layerDataString);
                var rootElement = layerDataDoc.RootElement;
                
                // Handle different GeoJSON structures:
                // 1. FeatureCollection: { "type": "FeatureCollection", "features": [...] }
                // 2. Direct features array: [...]
                // 3. Single feature: { "type": "Feature", ... }
                
                JsonElement featuresElement;
                bool hasFeatures = false;
                
                if (rootElement.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "FeatureCollection")
                {
                    // Standard GeoJSON FeatureCollection
                    if (rootElement.TryGetProperty("features", out featuresElement))
                    {
                        hasFeatures = true;
                    }
                    else
                    {
                        _logger.LogWarning("Layer {LayerId} has FeatureCollection type but no features array", layer.LayerId);
                        continue;
                    }
                }
                else if (rootElement.ValueKind == JsonValueKind.Array)
                {
                    // Direct features array
                    featuresElement = rootElement;
                    hasFeatures = true;
                }
                else if (rootElement.TryGetProperty("type", out var featureType) && featureType.GetString() == "Feature")
                {
                    // Single feature - wrap in array for processing
                    featuresElement = JsonDocument.Parse($"[{rootElement.GetRawText()}]").RootElement;
                    hasFeatures = true;
                }
                else
                {
                    _logger.LogWarning("Layer {LayerId} has unrecognized GeoJSON structure. Root element type: {ValueKind}", 
                        layer.LayerId, rootElement.ValueKind);
                    continue;
                }
                
                if (!hasFeatures)
                {
                    continue;
                }
                
                // Process features
                int featureCount = 0;
                int totalFeaturesInArray = 0;
                
                // Count total features first
                foreach (var _ in featuresElement.EnumerateArray())
                {
                    totalFeaturesInArray++;
                }
                
                _logger.LogInformation("Found {TotalFeatures} features in layer {LayerId} array", totalFeaturesInArray, layer.LayerId);
                
                foreach (var layerFeature in featuresElement.EnumerateArray())
                {
                    try
                    {
                        // Check if it's a valid feature
                        if (!layerFeature.TryGetProperty("type", out var featureTypeElement) || 
                            featureTypeElement.GetString() != "Feature")
                        {
                            _logger.LogDebug("Skipping non-Feature element in layer {LayerId}", layer.LayerId);
                            continue;
                        }
                        
                        // Check if it has geometry
                        if (!layerFeature.TryGetProperty("geometry", out var geometryElement))
                        {
                            _logger.LogDebug("Skipping feature without geometry in layer {LayerId}", layer.LayerId);
                            continue;
                        }
                        
                        // Check if it's a Text annotation type and skip it (they don't display correctly)
                        if (layerFeature.TryGetProperty("properties", out var propsElement) && 
                            propsElement.ValueKind == JsonValueKind.Object)
                        {
                            if (propsElement.TryGetProperty("annotationType", out var annTypeElement))
                            {
                                var annType = annTypeElement.GetString();
                                if (!string.IsNullOrEmpty(annType) && 
                                    annType.Equals("Text", StringComparison.OrdinalIgnoreCase))
                                {
                                    _logger.LogDebug("Skipping Text annotation feature from layer {LayerId}", layer.LayerId);
                                    continue;
                                }
                            }
                        }
                        
                        // Parse feature as dictionary - use JsonElement directly to preserve structure
                        var featureJson = layerFeature.GetRawText();
                        var featureObj = JsonSerializer.Deserialize<Dictionary<string, object>>(featureJson);
                        
                        if (featureObj == null)
                        {
                            _logger.LogWarning("Failed to deserialize feature in layer {LayerId}", layer.LayerId);
                            continue;
                        }
                        
                        // Ensure properties exist
                        if (!featureObj.ContainsKey("properties") || featureObj["properties"] == null)
                        {
                            featureObj["properties"] = new Dictionary<string, object>();
                        }
                        
                        // Get or create properties dictionary
                        var props = featureObj["properties"] as Dictionary<string, object>;
                        if (props == null)
                        {
                            try
                            {
                                var propsJson = featureObj["properties"]?.ToString() ?? "{}";
                                props = JsonSerializer.Deserialize<Dictionary<string, object>>(propsJson);
                            }
                            catch (Exception propEx)
                            {
                                _logger.LogWarning(propEx, "Failed to parse properties for feature in layer {LayerId}, creating empty properties", layer.LayerId);
                                props = new Dictionary<string, object>();
                            }
                        }
                        
                        if (props != null)
                        {
                            // Double-check for Text annotation after parsing (in case it wasn't caught earlier)
                            if (props.ContainsKey("annotationType") && props["annotationType"] != null)
                            {
                                var annType = props["annotationType"]?.ToString();
                                if (!string.IsNullOrEmpty(annType) && 
                                    annType.Equals("Text", StringComparison.OrdinalIgnoreCase))
                                {
                                    _logger.LogDebug("Skipping Text annotation feature from layer {LayerId} (found in parsed properties)", layer.LayerId);
                                    continue;
                                }
                            }
                            
                            // Add layer metadata to properties (only if not already present)
                            if (!props.ContainsKey("layerId"))
                            {
                                props["layerId"] = layer.LayerId.ToString();
                            }
                            if (!props.ContainsKey("layerName"))
                            {
                                props["layerName"] = layer.LayerName;
                            }
                            featureObj["properties"] = props;
                        }
                        
                        // Extract feature ID to check for duplicates
                        // Check both the JsonElement and the parsed object
                        string? featureId = null;
                        if (layerFeature.TryGetProperty("id", out var idElement))
                        {
                            if (idElement.ValueKind == JsonValueKind.String)
                            {
                                featureId = idElement.GetString();
                            }
                            else
                            {
                                featureId = idElement.GetRawText().Trim('"');
                            }
                        }
                        else if (featureObj.ContainsKey("id") && featureObj["id"] != null)
                        {
                            featureId = featureObj["id"]?.ToString();
                        }
                        
                        // Skip if this feature was already added (from MongoDB or another layer)
                        // IMPORTANT: Check BEFORE adding to the list
                        if (!string.IsNullOrEmpty(featureId))
                        {
                            if (addedFeatureIds.Contains(featureId))
                            {
                                _logger.LogWarning("Skipping duplicate feature {FeatureId} from layer {LayerId} (already added)", 
                                    featureId, layer.LayerId);
                                continue;
                            }
                            // Add to tracking set BEFORE adding to features list
                            addedFeatureIds.Add(featureId);
                        }
                        else
                        {
                            // If no ID, generate a temporary one based on geometry to avoid duplicates
                            // This handles features without IDs from layer data
                            try
                            {
                                var geometryJson = layerFeature.TryGetProperty("geometry", out var geom) 
                                    ? geom.GetRawText() 
                                    : "";
                                if (!string.IsNullOrEmpty(geometryJson))
                                {
                                    // Use geometry hash as a fallback ID for deduplication
                                    var geometryHash = geometryJson.GetHashCode().ToString();
                                    if (addedFeatureIds.Contains(geometryHash))
                                    {
                                        _logger.LogWarning("Skipping duplicate feature (by geometry) from layer {LayerId} (already added)", layer.LayerId);
                                        continue;
                                    }
                                    addedFeatureIds.Add(geometryHash);
                                }
                            }
                            catch
                            {
                                // If we can't generate a hash, just add it (should be rare)
                            }
                        }
                        
                        features.Add(featureObj);
                        featureCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse individual feature from layer {LayerId}. Feature JSON: {FeatureJson}", 
                            layer.LayerId, layerFeature.GetRawText().Substring(0, Math.Min(200, layerFeature.GetRawText().Length)));
                    }
                }
                
                _logger.LogInformation("Extracted {FeatureCount} features from layer {LayerId} ({LayerName})", 
                    featureCount, layer.LayerId, layer.LayerName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process layer data for layer {LayerId} ({LayerName})", 
                    layer.LayerId, layer.LayerName);
            }
        }
        
        _logger.LogInformation("GeoJSON export complete. Total features: {FeatureCount}, Total layers processed: {LayerCount}", 
            features.Count, mapData.Layers.Count);
        
        // Create GeoJSON FeatureCollection
        // Note: metadata is included but upload functionality only uses the features array
        var geoJson = new
        {
            type = "FeatureCollection",
            features = features,
            metadata = new
            {
                mapId = map.MapId.ToString(),
                mapName = map.MapName,
                description = map.Description,
                baseLayer = map.BaseLayer,
                viewState = map.ViewState,
                generatedAt = DateTime.UtcNow.ToString("O"),
                featureCount = features.Count,
                layerCount = mapData.Layers.Count
            }
        };
        
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var result = JsonSerializer.SerializeToUtf8Bytes(geoJson, jsonOptions);
        _logger.LogInformation("Generated GeoJSON size: {Size} bytes", result.Length);
        
        return result;
    }
    
    // Helper method to get all map data for export
    private async Task<(List<CusomMapOSM_Domain.Entities.Layers.Layer> Layers, List<CusomMapOSM_Application.Models.Documents.MapFeatureDocument> Features)> GetMapDataForExportAsync(
        CusomMapOSM_Domain.Entities.Maps.Map map, 
        Dictionary<string, bool>? visibleLayerIds = null,
        Dictionary<string, bool>? visibleFeatureIds = null)
    {
        // Get map layers
        var allLayers = await _mapRepository.GetMapLayers(map.MapId);
        
        // Filter layers based on visibleLayerIds if provided
        var layers = visibleLayerIds != null && visibleLayerIds.Count > 0
            ? allLayers.Where(l => visibleLayerIds.ContainsKey(l.LayerId.ToString()) && visibleLayerIds[l.LayerId.ToString()])
                .ToList()
            : allLayers.Where(l => l.IsVisible).ToList(); // Default to visible layers
        
        // Get map features from MongoDB
        var allFeatures = await _mapFeatureStore.GetByMapAsync(map.MapId);
        
        _logger.LogInformation("Retrieved {TotalFeatures} total features from MongoDB for map {MapId}", allFeatures.Count, map.MapId);
        
        // Log feature details
        foreach (var f in allFeatures)
        {
            _logger.LogDebug("Feature {FeatureId}: Geometry={HasGeometry}, GeometryType={GeometryType}, IsVisible={IsVisible}", 
                f.Id, f.Geometry != null, f.GeometryType, f.IsVisible);
        }
        
        // Filter features based on visibleFeatureIds if provided
        // If visibleFeatureIds is provided and not empty, use it for filtering
        // Otherwise, include all features (don't filter by IsVisible for export)
        List<CusomMapOSM_Application.Models.Documents.MapFeatureDocument> features;
        
        if (visibleFeatureIds != null && visibleFeatureIds.Count > 0)
        {
            _logger.LogInformation("Applying visibleFeatureIds filter with {Count} entries: {Keys}", 
                visibleFeatureIds.Count, string.Join(", ", visibleFeatureIds.Keys));
            
            features = allFeatures.Where(f => 
            {
                // Check both with and without "feature-" prefix
                // Frontend may send keys with "feature-" prefix, but MongoDB IDs don't have it
                var hasKeyDirect = visibleFeatureIds.ContainsKey(f.Id);
                var hasKeyWithPrefix = visibleFeatureIds.ContainsKey($"feature-{f.Id}");
                
                // Also check if any key ends with the feature ID (handles various prefix formats)
                var matchingKey = visibleFeatureIds.Keys.FirstOrDefault(key => 
                    key == f.Id || 
                    key == $"feature-{f.Id}" ||
                    key.EndsWith($"-{f.Id}") ||
                    key.EndsWith(f.Id));
                
                bool isVisible = false;
                if (hasKeyDirect)
                {
                    isVisible = visibleFeatureIds[f.Id];
                }
                else if (hasKeyWithPrefix)
                {
                    isVisible = visibleFeatureIds[$"feature-{f.Id}"];
                }
                else if (matchingKey != null)
                {
                    isVisible = visibleFeatureIds[matchingKey];
                }
                
                _logger.LogDebug("Feature {FeatureId}: hasKeyDirect={HasKeyDirect}, hasKeyWithPrefix={HasKeyWithPrefix}, matchingKey={MatchingKey}, isVisible={IsVisible}", 
                    f.Id, hasKeyDirect, hasKeyWithPrefix, matchingKey ?? "null", isVisible);
                return isVisible;
            }).ToList();
        }
        else
        {
            _logger.LogInformation("No visibleFeatureIds filter provided, including all {Count} features", allFeatures.Count);
            features = allFeatures.ToList(); // Include all features for export if no filter specified
        }
        
        _logger.LogInformation("Retrieved {LayerCount} layers (filtered from {TotalLayers}) and {FeatureCount} features (filtered from {TotalFeatures}) for map {MapId}", 
            layers.Count, allLayers.Count, features.Count, allFeatures.Count, map.MapId);
        
        // Log which features were filtered out
        if (allFeatures.Count > features.Count)
        {
            var filteredOutIds = allFeatures.Where(f => !features.Contains(f)).Select(f => f.Id).ToList();
            _logger.LogWarning("Filtered out {Count} features: {FeatureIds}", filteredOutIds.Count, string.Join(", ", filteredOutIds));
        }
        
        return (layers, features);
    }

    public async Task<Option<ExportListResponse, Error>> GetPendingApprovalExportsAsync()
    {
        try
        {
            var exports = await _exportRepository.GetPendingApprovalExportsAsync();
            // Admin view: always show file URLs
            var exportDtos = exports.Select(e => MapToDto(e, isAdminView: true)).ToList();

            return Option.Some<ExportListResponse, Error>(new ExportListResponse
            {
                Exports = exportDtos,
                Total = exportDtos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approval exports");
            return Option.None<ExportListResponse, Error>(
                Error.Failure("Export.GetFailed", $"Failed to get pending approval exports: {ex.Message}"));
        }
    }

    public async Task<Option<ExportListResponse, Error>> GetAllExportsAsync(int page = 1, int pageSize = 20, ExportStatusEnum? status = null)
    {
        try
        {
            var (exports, totalCount) = await _exportRepository.GetAllExportsAsync(page, pageSize, status);

            // Admin view: always show file URLs
            var exportDtos = exports.Select(e => MapToDto(e, isAdminView: true)).ToList();

            return Option.Some<ExportListResponse, Error>(new ExportListResponse
            {
                Exports = exportDtos,
                Total = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all exports for admin. Page: {Page}, PageSize: {PageSize}, Status: {Status}",
                page, pageSize, status?.ToString() ?? "All");
            return Option.None<ExportListResponse, Error>(
                Error.Failure("Export.GetAllFailed", $"Failed to get all exports: {ex.Message}"));
        }
    }

    public async Task<Option<ExportResponse, Error>> ApproveExportAsync(int exportId, Guid adminUserId)
    {
        try
        {
            var export = await _exportRepository.GetByIdAsync(exportId);
            if (export == null)
            {
                return Option.None<ExportResponse, Error>(
                    Error.NotFound("Export.NotFound", "Export not found"));
            }

            // Allow approval if status is PendingApproval (normal case)
            // Also allow if status is Pending or Processing but file exists and is valid (processing completed but status not updated)
            // Check if file path exists and is a valid URL (not empty string)
            var hasValidFile = !string.IsNullOrEmpty(export.FilePath) && 
                              (export.FilePath.StartsWith("http://") || export.FilePath.StartsWith("https://"));
            
            var canApprove = export.Status == ExportStatusEnum.PendingApproval ||
                            (export.Status == ExportStatusEnum.Pending && hasValidFile) ||
                            (export.Status == ExportStatusEnum.Processing && hasValidFile);

            if (!canApprove)
            {
                if (export.Status == ExportStatusEnum.Pending || export.Status == ExportStatusEnum.Processing)
                {
                    // Check if file exists but path is not set (processing might have failed)
                    if (!hasValidFile)
                    {
                        return Option.None<ExportResponse, Error>(
                            Error.ValidationError("Export.NotReady", 
                                $"Export is still being processed. Current status: {export.Status}. File has not been uploaded yet. Please wait for processing to complete."));
                    }
                    else
                    {
                        // File exists but status check failed - this shouldn't happen, but handle it
                        _logger.LogWarning("Export {ExportId} has file but status check failed. FilePath: {FilePath}", exportId, export.FilePath);
                    }
                }
                
                if (export.Status == ExportStatusEnum.Approved)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.ValidationError("Export.AlreadyApproved", 
                            "Export has already been approved."));
                }

                if (export.Status == ExportStatusEnum.Rejected)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.ValidationError("Export.AlreadyRejected", 
                            "Export has been rejected and cannot be approved."));
                }

                return Option.None<ExportResponse, Error>(
                    Error.ValidationError("Export.InvalidStatus", 
                        $"Export cannot be approved in current status: {export.Status}"));
            }

            // If status is Pending or Processing but file exists, update status to PendingApproval first
            // (This handles edge case where processing completed but status wasn't updated)
            if ((export.Status == ExportStatusEnum.Pending || export.Status == ExportStatusEnum.Processing) 
                && !string.IsNullOrEmpty(export.FilePath))
            {
                export.Status = ExportStatusEnum.PendingApproval;
                export.CompletedAt = DateTime.UtcNow;
                await _exportRepository.UpdateAsync(export);
                _logger.LogInformation("Export {ExportId} status updated to PendingApproval before approval", exportId);
            }

            // Get membership to find organization ID for quota checking
            DomainMembership? membership = null;
            Guid? orgId = null;
            
            if (export.MembershipId != Guid.Empty)
            {
                membership = await _membershipRepository.GetByIdAsync(export.MembershipId, CancellationToken.None);
                if (membership != null)
                {
                    orgId = membership.OrgId;
                }
            }
            
            // If membership not found by ID, try to get from user's organization
            if (membership == null)
            {
                // Try to get membership from map's workspace organization
                var map = await _mapRepository.GetMapById(export.MapId);
                if (map?.WorkspaceId.HasValue == true)
                {
                    var workspace = await _workspaceRepository.GetByIdAsync(map.WorkspaceId.Value);
                    if (workspace?.OrgId.HasValue == true)
                    {
                        orgId = workspace.OrgId.Value;
                        membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(
                            export.UserId,
                            workspace.OrgId.Value,
                            CancellationToken.None);
                    }
                }
            }

            // Check count-based export quota before approving (if membership found)
            if (membership != null && orgId.HasValue)
            {
                var quotaCheck = await _usageService.CheckUserQuotaAsync(
                    export.UserId,
                    orgId.Value,
                    "exports",
                    1,
                    CancellationToken.None);
                
                if (!quotaCheck.HasValue)
                {
                    _logger.LogWarning("Failed to check export quota for export {ExportId}, user {UserId}", exportId, export.UserId);
                }
                else
                {
                    var quotaResult = quotaCheck.Match(
                        some: quota => quota,
                        none: error => throw new InvalidOperationException($"Quota check failed: {error.Description}")
                    );
                    
                    if (!quotaResult.IsAllowed)
                    {
                        _logger.LogWarning("Export {ExportId} cannot be approved: user {UserId} export count quota exceeded. {Message}", 
                            exportId, export.UserId, quotaResult.Message);
                        
                        // Reject the export due to insufficient count quota
                        export.Status = ExportStatusEnum.Rejected;
                        export.ApprovedBy = adminUserId;
                        export.ApprovedAt = DateTime.UtcNow;
                        export.RejectionReason = $"Export quota exceeded. {quotaResult.Message} Please upgrade your membership or wait for quota reset.";
                        await _exportRepository.UpdateAsync(export);
                        
                        var rejectedExport = await _exportRepository.GetByIdWithIncludesAsync(exportId);
                        return Option.None<ExportResponse, Error>(
                            Error.ValidationError("Export.QuotaExceeded", 
                                $"Cannot approve export: Export quota exceeded. {quotaResult.Message} The export has been rejected."));
                    }
                }
            }

            // Calculate and consume quota tokens when admin approves
            // Determine file type from export format
            var fileExtension = export.ExportType switch
            {
                ExportTypeEnum.PDF => "pdf",
                ExportTypeEnum.PNG => "png",
                ExportTypeEnum.SVG => "svg",
                ExportTypeEnum.GeoJSON => "geojson",
                _ => "unknown"
            };
            
            var fileType = ExportQuotaExtensions.GetFileTypeFromExtension($".{fileExtension}");
            var fileSizeKB = (int)Math.Ceiling(export.FileSize / 1024.0);
            
            // Re-check token quota before consuming (in case user's quota changed since export was created)
            if (!await _exportQuotaService.CanExportAsync(export.UserId, fileType, fileSizeKB))
            {
                _logger.LogWarning("Export {ExportId} cannot be approved: user {UserId} token quota exceeded. File size: {FileSizeKB}KB", 
                    exportId, export.UserId, fileSizeKB);
                
                // Reject the export due to insufficient token quota
                export.Status = ExportStatusEnum.Rejected;
                export.ApprovedBy = adminUserId;
                export.ApprovedAt = DateTime.UtcNow;
                export.RejectionReason = "Insufficient quota tokens. Please upgrade your membership or wait for quota reset.";
                await _exportRepository.UpdateAsync(export);
                
                var rejectedExport = await _exportRepository.GetByIdWithIncludesAsync(exportId);
                return Option.None<ExportResponse, Error>(
                    Error.ValidationError("Export.QuotaExceeded", 
                        "Cannot approve export: User token quota exceeded. The export has been rejected."));
            }
            
            // Calculate and consume quota tokens
            var tokenCost = await _exportQuotaService.CalculateTokenCostAsync(fileType, fileSizeKB);
            var tokensConsumed = await _exportQuotaService.ConsumeTokensAsync(export.UserId, tokenCost);
            
            if (!tokensConsumed)
            {
                _logger.LogError("Failed to consume {TokenCost} tokens for user {UserId} when approving export {ExportId}", 
                    tokenCost, export.UserId, exportId);
                
                // Reject the export if token consumption fails
                export.Status = ExportStatusEnum.Rejected;
                export.ApprovedBy = adminUserId;
                export.ApprovedAt = DateTime.UtcNow;
                export.RejectionReason = "Failed to consume quota tokens. Please contact support.";
                await _exportRepository.UpdateAsync(export);
                
                var failedExport = await _exportRepository.GetByIdWithIncludesAsync(exportId);
                return Option.None<ExportResponse, Error>(
                    Error.Failure("Export.TokenConsumptionFailed", 
                        "Failed to consume quota tokens. The export has been rejected."));
            }
            
            _logger.LogInformation("Consumed {TokenCost} tokens for user {UserId} when approving export {ExportId}", 
                tokenCost, export.UserId, exportId);

            // Note: Export quota (count-based) was already consumed when export was created
            // We don't need to consume again here to avoid double-counting
            // Only token quota is consumed on approval

            // For PNG and PDF exports, add IMOS logo before finalizing approval
            if (export.ExportType == ExportTypeEnum.PNG || export.ExportType == ExportTypeEnum.PDF)
            {
                try
                {
                    var logoAdded = await AddLogoToExportAsync(export);
                    if (logoAdded)
                    {
                        _logger.LogInformation("IMOS logo added to export {ExportId}", exportId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to add IMOS logo to export {ExportId}, but continuing with approval", exportId);
                    }
                }
                catch (Exception logoEx)
                {
                    _logger.LogWarning(logoEx, "Error adding logo to export {ExportId}, but continuing with approval", exportId);
                    // Don't fail approval if logo addition fails
                }
            }

            export.Status = ExportStatusEnum.Approved;
            export.ApprovedBy = adminUserId;
            export.ApprovedAt = DateTime.UtcNow;
            export.RejectionReason = null;

            await _exportRepository.UpdateAsync(export);

            var updatedExport = await _exportRepository.GetByIdWithIncludesAsync(exportId);
            return Option.Some<ExportResponse, Error>(MapToDto(updatedExport!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving export {ExportId}", exportId);
            return Option.None<ExportResponse, Error>(
                Error.Failure("Export.ApproveFailed", $"Failed to approve export: {ex.Message}"));
        }
    }

    public async Task<Option<ExportResponse, Error>> RejectExportAsync(int exportId, Guid adminUserId, string reason)
    {
        try
        {
            var export = await _exportRepository.GetByIdAsync(exportId);
            if (export == null)
            {
                return Option.None<ExportResponse, Error>(
                    Error.NotFound("Export.NotFound", "Export not found"));
            }

            // Allow rejection if status is PendingApproval (normal case)
            // Also allow if status is Pending or Processing but file exists
            // Disallow if already approved, rejected, or failed
            var canReject = export.Status == ExportStatusEnum.PendingApproval ||
                           (export.Status == ExportStatusEnum.Pending && !string.IsNullOrEmpty(export.FilePath)) ||
                           (export.Status == ExportStatusEnum.Processing && !string.IsNullOrEmpty(export.FilePath));

            if (!canReject)
            {
                if (export.Status == ExportStatusEnum.Pending || export.Status == ExportStatusEnum.Processing)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.ValidationError("Export.NotReady", 
                            $"Export is still being processed. Current status: {export.Status}. Please wait for processing to complete."));
                }
                
                if (export.Status == ExportStatusEnum.Approved)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.ValidationError("Export.AlreadyApproved", 
                            "Export has already been approved and cannot be rejected."));
                }

                if (export.Status == ExportStatusEnum.Rejected)
                {
                    return Option.None<ExportResponse, Error>(
                        Error.ValidationError("Export.AlreadyRejected", 
                            "Export has already been rejected."));
                }

                return Option.None<ExportResponse, Error>(
                    Error.ValidationError("Export.InvalidStatus", 
                        $"Export cannot be rejected in current status: {export.Status}"));
            }

            // If status is Pending or Processing but file exists, update status to PendingApproval first
            if ((export.Status == ExportStatusEnum.Pending || export.Status == ExportStatusEnum.Processing) 
                && !string.IsNullOrEmpty(export.FilePath))
            {
                export.Status = ExportStatusEnum.PendingApproval;
                export.CompletedAt = DateTime.UtcNow;
                await _exportRepository.UpdateAsync(export);
                _logger.LogInformation("Export {ExportId} status updated to PendingApproval before rejection", exportId);
            }

            // Refund export quota (decrement usage) when export is rejected
            // Only refund if the export was in a state where quota was consumed (Pending, PendingApproval, Processing)
            // Don't refund if already Approved (quota should remain consumed) or already Rejected (already refunded)
            var shouldRefundQuota = export.Status == ExportStatusEnum.Pending ||
                                   export.Status == ExportStatusEnum.PendingApproval ||
                                   export.Status == ExportStatusEnum.Processing;
            
            if (shouldRefundQuota)
            {
                // Get membership to find organization ID
                DomainMembership? membership = null;
                Guid? orgId = null;
                
                if (export.MembershipId != Guid.Empty)
                {
                    membership = await _membershipRepository.GetByIdAsync(export.MembershipId, CancellationToken.None);
                    if (membership != null)
                    {
                        orgId = membership.OrgId;
                    }
                }
                
                // If membership not found by ID, try to get from map's workspace organization
                if (membership == null)
                {
                    var map = await _mapRepository.GetMapById(export.MapId);
                    if (map?.WorkspaceId.HasValue == true)
                    {
                        var workspace = await _workspaceRepository.GetByIdAsync(map.WorkspaceId.Value);
                        if (workspace?.OrgId.HasValue == true)
                        {
                            orgId = workspace.OrgId.Value;
                            membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(
                                export.UserId,
                                workspace.OrgId.Value,
                                CancellationToken.None);
                        }
                    }
                }

                // Refund quota by consuming -1 (decrementing)
                if (membership != null && orgId.HasValue)
                {
                    // Use negative amount to refund/decrement
                    var refundQuotaResult = await _usageService.ConsumeUserQuotaAsync(
                        export.UserId,
                        orgId.Value,
                        "exports",
                        -1, // Negative amount to refund
                        CancellationToken.None);
                    
                    if (!refundQuotaResult.HasValue)
                    {
                        _logger.LogWarning("Failed to refund export quota for export {ExportId}, user {UserId}, org {OrgId}. Export rejected but quota not refunded.", 
                            exportId, export.UserId, orgId.Value);
                        // Note: We don't fail the rejection here since the export should still be rejected
                        // This is a tracking issue, but the export should still be rejected
                    }
                    else
                    {
                        var refunded = refundQuotaResult.Match(
                            some: result => result,
                            none: error => false
                        );
                        
                        if (!refunded)
                        {
                            _logger.LogWarning("Quota refund failed for export {ExportId}, user {UserId}, org {OrgId}. Export rejected but quota not refunded.", 
                                exportId, export.UserId, orgId.Value);
                        }
                        else
                        {
                            _logger.LogInformation("Refunded export quota: decremented ExportsThisCycle for user {UserId}, org {OrgId}, export {ExportId} (rejected)", 
                                export.UserId, orgId.Value, exportId);
                        }
                    }
                }
            }
            else
            {
                _logger.LogInformation("Export {ExportId} rejection: quota not refunded because status is {Status} (quota was not consumed or already refunded)", 
                    exportId, export.Status);
            }

            export.Status = ExportStatusEnum.Rejected;
            export.ApprovedBy = adminUserId;
            export.ApprovedAt = DateTime.UtcNow;
            export.RejectionReason = reason;

            await _exportRepository.UpdateAsync(export);

            var updatedExport = await _exportRepository.GetByIdWithIncludesAsync(exportId);
            return Option.Some<ExportResponse, Error>(MapToDto(updatedExport!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting export {ExportId}", exportId);
            return Option.None<ExportResponse, Error>(
                Error.Failure("Export.RejectFailed", $"Failed to reject export: {ex.Message}"));
        }
    }

    public async Task<Option<string, Error>> GetExportDownloadUrlAsync(int exportId)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<string, Error>(
                    Error.Unauthorized("Export.Unauthorized", "User must be authenticated"));
            }

            var export = await _exportRepository.GetByIdWithIncludesAsync(exportId);
            if (export == null)
            {
                return Option.None<string, Error>(
                    Error.NotFound("Export.NotFound", "Export not found"));
            }

            // Check if user owns the export or is admin
            var isOwner = export.UserId == currentUserId.Value;
            // TODO: Add admin check here if needed

            if (!isOwner)
            {
                return Option.None<string, Error>(
                    Error.Forbidden("Export.AccessDenied", "You don't have permission to download this export"));
            }

            // Only allow download if approved
            if (export.Status != ExportStatusEnum.Approved)
            {
                return Option.None<string, Error>(
                    Error.Forbidden("Export.NotApproved", 
                        $"Export is not approved. Current status: {export.Status}. Please wait for admin approval."));
            }

            if (string.IsNullOrEmpty(export.FilePath))
            {
                return Option.None<string, Error>(
                    Error.NotFound("Export.FileNotFound", "Export file not found"));
            }

            // If FilePath is already a full URL, return it as is
            // If it's just a path, convert it to a full Firebase URL
            var downloadUrl = export.FilePath;
            if (!downloadUrl.StartsWith("http://") && !downloadUrl.StartsWith("https://"))
            {
                // It's a path stored in the database (legacy data or error case)
                // Convert it to a full Firebase URL
                try
                {
                    _logger.LogInformation("Converting file path to Firebase URL: {FilePath}", export.FilePath);
                    
                    // Handle the path format: "exports/exports/png/filename" or "exports/png/filename"
                    // The actual Firebase object name is: "exports/png/{guid}_{timestamp}_filename"
                    var objectPath = export.FilePath;
                    var fileName = Path.GetFileName(objectPath); // Extract just the filename
                    
                    // Determine the folder from the path
                    string folder;
                    if (objectPath.StartsWith("exports/exports/"))
                    {
                        // Remove duplicate "exports/" prefix
                        var pathWithoutPrefix = objectPath.Substring("exports/".Length);
                        var lastSlashIndex = pathWithoutPrefix.LastIndexOf('/');
                        folder = lastSlashIndex > 0 ? pathWithoutPrefix.Substring(0, lastSlashIndex) : pathWithoutPrefix;
                    }
                    else if (objectPath.StartsWith("exports/"))
                    {
                        var lastSlashIndex = objectPath.LastIndexOf('/');
                        folder = lastSlashIndex > 0 ? objectPath.Substring(0, lastSlashIndex) : objectPath;
                    }
                    else
                    {
                        folder = "exports/png"; // Default fallback
                    }
                    
                    // Try to find the file by searching for the filename pattern in the folder
                    // Firebase stores files as: {folder}/{guid}_{timestamp}_{filename}
                    // So we search for files containing the filename
                    _logger.LogInformation("Searching for file in folder '{Folder}' with pattern '{FileNamePattern}'", folder, fileName);
                    string? foundUrl = null;
                    try
                    {
                        foundUrl = await _firebaseStorageService.FindFileByPatternAsync(folder, fileName);
                    }
                    catch (Exception searchEx)
                    {
                        _logger.LogWarning(searchEx, "Error searching for file in Firebase Storage. This might indicate credential or permission issues.");
                    }
                    
                    if (!string.IsNullOrEmpty(foundUrl))
                    {
                        _logger.LogInformation("Found file in Firebase Storage: {Url}", foundUrl);
                        downloadUrl = foundUrl;
                    }
                    else
                    {
                        _logger.LogWarning("File not found in Firebase Storage folder '{Folder}' with pattern '{FileNamePattern}'. Trying direct access.", folder, fileName);
                        // If search fails, try direct access (in case the path is correct)
                        try
                        {
                            downloadUrl = await _firebaseStorageService.DownloadFileAsync(objectPath);
                        }
                        catch
                        {
                            // Last resort: construct URL manually (may not work if file doesn't exist)
                            var bucketName = _configuration["Firebase:StorageBucket"] 
                                ?? Environment.GetEnvironmentVariable("FIREBASE_STORAGE_BUCKET");
                            
                            if (!string.IsNullOrEmpty(bucketName))
                            {
                                // Try with the corrected path (without duplicate exports/)
                                var correctedPath = objectPath.StartsWith("exports/exports/") 
                                    ? objectPath.Substring("exports/".Length) 
                                    : objectPath;
                                downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(correctedPath)}?alt=media";
                            }
                            else
                            {
                                throw new InvalidOperationException("Firebase Storage Bucket is not configured");
                            }
                        }
                    }
                    
                    // Ensure we have a valid URL
                    if (string.IsNullOrEmpty(downloadUrl) || (!downloadUrl.StartsWith("http://") && !downloadUrl.StartsWith("https://")))
                    {
                        throw new InvalidOperationException("Failed to generate valid Firebase URL");
                    }
                    
                    // Update the database with the full URL for future use
                    export.FilePath = downloadUrl;
                    await _exportRepository.UpdateAsync(export);
                    _logger.LogInformation("Updated export {ExportId} with full Firebase URL: {Url}", exportId, downloadUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert path to Firebase URL for export {ExportId}, path: {FilePath}", 
                        exportId, export.FilePath);
                    return Option.None<string, Error>(
                        Error.NotFound("Export.FileUrlConversionFailed", 
                            $"Failed to generate download URL for file path: {export.FilePath}. Error: {ex.Message}"));
                }
            }

            return Option.Some<string, Error>(downloadUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download URL for export {ExportId}", exportId);
            return Option.None<string, Error>(
                Error.Failure("Export.GetDownloadUrlFailed", $"Failed to get download URL: {ex.Message}"));
        }
    }

    /// <summary>
    /// Adds IMOS logo to PNG/PDF export images before approval
    /// </summary>
    private async Task<bool> AddLogoToExportAsync(Export export)
    {
        try
        {
            // Get logo path from configuration (default to assets folder if not configured)
            var logoPath = _configuration["ExportSettings:LogoPath"] ?? 
                          Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "imos-logo.svg");
            
            // For now, we'll create a simple text-based logo if the file doesn't exist
            // In production, you should upload the actual IMOS logo to Firebase Storage or a CDN
            // and reference it via URL
            
            // Download the current export file from Firebase
            if (string.IsNullOrEmpty(export.FilePath) || !export.FilePath.StartsWith("http"))
            {
                _logger.LogWarning("Export {ExportId} has invalid file path for logo addition: {FilePath}", 
                    export.ExportId, export.FilePath);
                return false;
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            var fileBytes = await httpClient.GetByteArrayAsync(export.FilePath);
            
            byte[] modifiedBytes;
            if (export.ExportType == ExportTypeEnum.PNG)
            {
                modifiedBytes = await AddLogoToPngAsync(fileBytes, logoPath);
            }
            else if (export.ExportType == ExportTypeEnum.PDF)
            {
                // For PDF, we need to extract the image, add logo, and rebuild PDF
                // For simplicity, we'll add logo to the PNG image used in PDF generation
                // This requires re-generating the PDF with the logo
                modifiedBytes = await AddLogoToPdfAsync(fileBytes, logoPath);
            }
            else
            {
                return false;
            }

            // Re-upload the modified file to Firebase
            var fileName = Path.GetFileName(new Uri(export.FilePath).LocalPath);
            var folder = export.ExportType == ExportTypeEnum.PNG ? "exports/png" : "exports/pdf";
            
            using var fileStream = new MemoryStream(modifiedBytes);
            var newFileUrl = await _firebaseStorageService.UploadFileAsync(fileName, fileStream, folder);
            
            if (string.IsNullOrEmpty(newFileUrl) || (!newFileUrl.StartsWith("http://") && !newFileUrl.StartsWith("https://")))
            {
                _logger.LogError("Failed to upload modified export file for export {ExportId}", export.ExportId);
                return false;
            }

            // Update export with new file URL
            export.FilePath = newFileUrl;
            export.FileSize = modifiedBytes.Length;
            await _exportRepository.UpdateAsync(export);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding logo to export {ExportId}", export.ExportId);
            return false;
        }
    }

    /// <summary>
    /// Adds IMOS logo to PNG image
    /// </summary>
    private async Task<byte[]> AddLogoToPngAsync(byte[] pngBytes, string logoPath)
    {
        using var inputStream = new MemoryStream(pngBytes);
        using var bitmap = new Bitmap(inputStream);
        using var graphics = Graphics.FromImage(bitmap);
        
        // Set high quality rendering
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Try to load logo from file or URL
        Image? logoImage = null;
        try
        {
            if (logoPath.StartsWith("http://") || logoPath.StartsWith("https://"))
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var logoBytes = await httpClient.GetByteArrayAsync(logoPath);
                using var logoStream = new MemoryStream(logoBytes);
                logoImage = Image.FromStream(logoStream);
            }
            else if (File.Exists(logoPath))
            {
                logoImage = Image.FromFile(logoPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load logo from {LogoPath}, using text-based logo", logoPath);
        }

        // If logo image not available, create a professional text-based logo
        if (logoImage == null)
        {
            // Calculate logo size and position (top-right corner)
            var logoWidth = Math.Min(bitmap.Width / 6, 150); // Max 150px or 1/6 of width
            var logoHeight = logoWidth / 3; // 3:1 aspect ratio
            var logoX = bitmap.Width - logoWidth - 20; // 20px margin from right
            var logoY = 20; // 20px margin from top
            
            // Draw rounded rectangle background with emerald color
            var cornerRadius = 8;
            var rect = new Rectangle(logoX, logoY, logoWidth, logoHeight);
            
            // Create rounded rectangle path
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            path.CloseAllFigures();
            
            // Fill with emerald color (semi-transparent)
            using var logoBrush = new SolidBrush(Color.FromArgb(230, 16, 185, 129)); // Emerald with slight transparency
            graphics.FillPath(logoBrush, path);
            
            // Draw border
            using var borderPen = new Pen(Color.FromArgb(16, 185, 129), 2);
            graphics.DrawPath(borderPen, path);
            
            // Draw "IMOS" text in white, bold
            var fontSize = logoHeight / 2.5f;
            using var font = new Font("Arial", fontSize, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            
            var textSize = graphics.MeasureString("IMOS", font);
            var textX = logoX + (logoWidth - textSize.Width) / 2;
            var textY = logoY + (logoHeight - textSize.Height) / 2;
            
            // Draw text with slight shadow for depth
            using var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
            graphics.DrawString("IMOS", font, shadowBrush, textX + 1, textY + 1);
            graphics.DrawString("IMOS", font, textBrush, textX, textY);
            
            // Draw decorative line under text
            var lineY = textY + textSize.Height - 2;
            var lineX1 = textX + 5;
            var lineX2 = textX + textSize.Width - 5;
            using var linePen = new Pen(Color.White, 1.5f);
            graphics.DrawLine(linePen, lineX1, lineY, lineX2, lineY);
        }
        else
        {
            // Draw logo image
            var logoSize = Math.Min(bitmap.Width, bitmap.Height) / 10; // 10% of smaller dimension
            var logoX = bitmap.Width - logoSize - 20; // 20px margin from right
            var logoY = 20; // 20px margin from top
            
            // Maintain aspect ratio
            var aspectRatio = (double)logoImage.Width / logoImage.Height;
            var logoWidth = logoSize;
            var logoHeight = (int)(logoSize / aspectRatio);
            
            graphics.DrawImage(logoImage, logoX, logoY, logoWidth, logoHeight);
            logoImage.Dispose();
        }

        // Convert back to PNG bytes
        using var outputStream = new MemoryStream();
        bitmap.Save(outputStream, ImageFormat.Png);
        return outputStream.ToArray();
    }

    /// <summary>
    /// Adds IMOS logo to PDF (by modifying the embedded image)
    /// Note: This is a simplified implementation. For production, consider using a proper PDF library.
    /// </summary>
    private async Task<byte[]> AddLogoToPdfAsync(byte[] pdfBytes, string logoPath)
    {
        // For PDF, we'll extract the JPEG image, add logo, and rebuild PDF
        // This is a simplified approach - in production, use a proper PDF library like iTextSharp or PdfSharp
        
        // For now, we'll just return the original PDF
        // A proper implementation would:
        // 1. Parse PDF structure
        // 2. Extract JPEG image from PDF
        // 3. Add logo to JPEG using AddLogoToPngAsync logic
        // 4. Rebuild PDF with modified JPEG
        
        _logger.LogWarning("PDF logo addition not fully implemented. Returning original PDF.");
        return pdfBytes;
    }

    private static ExportResponse MapToDto(Export export, bool isAdminView = false)
    {
        // For admin view, always show file URL if it exists
        // For user view, only show file URL if approved
        var canDownload = export.Status == ExportStatusEnum.Approved;
        var fileUrl = isAdminView
            ? export.FilePath  // Admin can always see the file URL
            : (canDownload ? export.FilePath : null);  // Users only see if approved

        return new ExportResponse
        {
            ExportId = export.ExportId,
            MapId = export.MapId,
            MapName = export.Map?.MapName,
            UserId = export.UserId,
            UserName = export.User?.FullName,
            Format = export.ExportType,
            Status = export.Status,
            FileUrl = fileUrl,
            CanDownload = canDownload,
            FileSize = export.FileSize,
            ErrorMessage = export.ErrorMessage,
            ApprovedBy = export.ApprovedBy,
            ApprovedByName = null, // TODO: Load admin name if needed
            ApprovedAt = export.ApprovedAt,
            RejectionReason = export.RejectionReason,
            CreatedAt = export.CreatedAt,
            CompletedAt = export.CompletedAt
        };
    }
}

