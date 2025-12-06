using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Exports;
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
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Features.Exports;

public class ExportService : IExportService
{
    private readonly IExportRepository _exportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapRepository _mapRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IFirebaseStorageService _firebaseStorageService;
    private readonly IExportQuotaService _exportQuotaService;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMapFeatureStore _mapFeatureStore;
    private readonly ILayerDataStore _layerDataStore;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IExportRepository exportRepository,
        ICurrentUserService currentUserService,
        IMapRepository mapRepository,
        IMembershipRepository membershipRepository,
        IWorkspaceRepository workspaceRepository,
        IFirebaseStorageService firebaseStorageService,
        IExportQuotaService exportQuotaService,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        IMapFeatureStore mapFeatureStore,
        ILayerDataStore layerDataStore,
        ILogger<ExportService> logger)
    {
        _exportRepository = exportRepository;
        _currentUserService = currentUserService;
        _mapRepository = mapRepository;
        _membershipRepository = membershipRepository;
        _workspaceRepository = workspaceRepository;
        _firebaseStorageService = firebaseStorageService;
        _exportQuotaService = exportQuotaService;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _mapFeatureStore = mapFeatureStore;
        _layerDataStore = layerDataStore;
        _logger = logger;
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
            
            if (request.MembershipId.HasValue)
            {
                membership = await _membershipRepository.GetByIdAsync(request.MembershipId.Value, CancellationToken.None);
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
                        membership = await _membershipRepository.GetByUserOrgWithIncludesAsync(
                            currentUserId.Value,
                            workspace.OrgId.Value,
                            CancellationToken.None);
                    }
                }
                
                // If no membership found, try to get any active membership for the user
                // (This handles personal maps or maps without organization)
                if (membership == null || membership.Status != MembershipStatusEnum.Active)
                {
                    // For now, we require an active membership
                    // In the future, we could allow exports without membership for free tier
                }
            }

            if (membership == null || membership.Status != MembershipStatusEnum.Active)
            {
                return Option.None<ExportResponse, Error>(
                    Error.NotFound("Export.MembershipNotFound", "Active membership not found. Please provide a membershipId or ensure you have an active membership for the map's organization."));
            }

            // Determine file extension and folder
            var (fileExtension, folder) = GetFileInfo(request.Format);
            var fileName = $"map_{request.MapId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";

            // Create export record with Pending status
            // Use empty string for FilePath initially - will be set to full Firebase URL after upload
            var export = new Export
            {
                UserId = currentUserId.Value,
                MembershipId = membership.MembershipId,
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

            // Store export configuration to pass to background processing
            var exportConfig = new
            {
                ViewState = request.ViewState,
                MapImageData = request.MapImageData,
                VisibleLayerIds = request.VisibleLayerIds,
                VisibleFeatureIds = request.VisibleFeatureIds,
                Options = request.Options
            };

            // Process export asynchronously in a background scope to avoid DbContext disposal issues
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

            return Option.Some<ExportResponse, Error>(MapToDto(createdExport));
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
            var exportDtos = exports.Select(MapToDto).ToList();

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
            var exportDtos = exports.Select(MapToDto).ToList();

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
                    fileData = await GenerateSvgAsync(map, visibleLayerIds, visibleFeatureIds, options, mapImageData);
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

            // Note: Quota tokens will be consumed when admin approves the export
            // We only check quota here to ensure user has enough tokens before processing
            // The actual consumption happens in ApproveExportAsync

            // Update export record - set to PendingApproval for admin review
            // Store the full Firebase URL
            export.FilePath = fileUrl;
            export.FileSize = fileData.Length;
            export.Status = ExportStatusEnum.PendingApproval;
            export.CompletedAt = DateTime.UtcNow;
            await _exportRepository.UpdateAsync(export);

            _logger.LogInformation("Export {ExportId} file uploaded to Firebase: {FileUrl}. File size: {FileSize}KB. Waiting for admin approval before consuming quota tokens.", 
                exportId, fileUrl, fileSizeKB);
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
            export.Status = status;
            export.ErrorMessage = errorMessage;
            if (status == ExportStatusEnum.PendingApproval || status == ExportStatusEnum.Approved)
            {
                export.CompletedAt = DateTime.UtcNow;
            }
            await _exportRepository.UpdateAsync(export);
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
        string? mapImageData = null)
    {
        // If we have captured image data, embed it in SVG
        if (!string.IsNullOrEmpty(mapImageData))
        {
            try
            {
                var svgWidth = options?.Width ?? 1920;
                var svgHeight = options?.Height ?? 1080;
                
                // Embed the base64 image in SVG
                var svgContent = $@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{svgWidth}"" height=""{svgHeight}"" viewBox=""0 0 {svgWidth} {svgHeight}"">
                    <image href=""{mapImageData}"" width=""{svgWidth}"" height=""{svgHeight}"" />
                </svg>";
                return System.Text.Encoding.UTF8.GetBytes(svgContent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to embed map image in SVG, falling back to placeholder");
            }
        }
        
        // Fallback: Generate SVG with map information
        // TODO: Render actual map features as SVG paths
        await Task.Delay(100);
        
        var mapData = await GetMapDataForExportAsync(map, visibleLayerIds, visibleFeatureIds);
        var title = options?.Title ?? map.MapName;
        var width = options?.Width ?? 800;
        var height = options?.Height ?? 600;
        
        var svg = $@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{width}"" height=""{height}"" viewBox=""0 0 {width} {height}"">
            <rect width=""{width}"" height=""{height}"" fill=""#f0f0f0""/>
            <text x=""50"" y=""50"" font-family=""Arial"" font-size=""20"" font-weight=""bold"">{title}</text>
            <text x=""50"" y=""80"" font-family=""Arial"" font-size=""14"">Generated at: {DateTime.UtcNow:O}</text>
            <text x=""50"" y=""110"" font-family=""Arial"" font-size=""14"">Layers: {mapData.Layers.Count}</text>
            <text x=""50"" y=""140"" font-family=""Arial"" font-size=""14"">Features: {mapData.Features.Count}</text>
            <text x=""50"" y=""170"" font-family=""Arial"" font-size=""12"" fill=""#666"">Note: This is a placeholder. Full map rendering requires rendering library.</text>
        </svg>";
        return System.Text.Encoding.UTF8.GetBytes(svg);
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
            var exportDtos = exports.Select(MapToDto).ToList();

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
            
            // Re-check quota before consuming (in case user's quota changed since export was created)
            if (!await _exportQuotaService.CanExportAsync(export.UserId, fileType, fileSizeKB))
            {
                _logger.LogWarning("Export {ExportId} cannot be approved: user {UserId} quota exceeded. File size: {FileSizeKB}KB", 
                    exportId, export.UserId, fileSizeKB);
                
                // Reject the export due to insufficient quota
                export.Status = ExportStatusEnum.Rejected;
                export.ApprovedBy = adminUserId;
                export.ApprovedAt = DateTime.UtcNow;
                export.RejectionReason = "Insufficient quota tokens. Please upgrade your membership or wait for quota reset.";
                await _exportRepository.UpdateAsync(export);
                
                var rejectedExport = await _exportRepository.GetByIdWithIncludesAsync(exportId);
                return Option.None<ExportResponse, Error>(
                    Error.ValidationError("Export.QuotaExceeded", 
                        "Cannot approve export: User quota exceeded. The export has been rejected."));
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

    private static ExportResponse MapToDto(Export export)
    {
        // Only show file URL if approved
        var canDownload = export.Status == ExportStatusEnum.Approved;
        var fileUrl = canDownload ? export.FilePath : null;

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

