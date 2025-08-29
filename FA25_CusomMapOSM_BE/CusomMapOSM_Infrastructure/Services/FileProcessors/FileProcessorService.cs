using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using Microsoft.AspNetCore.Http;

namespace CusomMapOSM_Infrastructure.Services.FileProcessors;

public class FileProcessorService : IFileProcessorService
{
    private readonly IVectorProcessor _vectorProcessor;
    private readonly IRasterProcessor _rasterProcessor; 
    private readonly ISpreadsheetProcessor _spreadsheetProcessor;

    public FileProcessorService(
        IVectorProcessor vectorProcessor,
        IRasterProcessor rasterProcessor,
        ISpreadsheetProcessor spreadsheetProcessor)
    {
        _vectorProcessor = vectorProcessor;
        _rasterProcessor = rasterProcessor;
        _spreadsheetProcessor = spreadsheetProcessor;
    }

    public async Task<FileProcessingResult> ProcessUploadedFile(IFormFile file, string layerName)
    {
        try
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            var contentType = file.ContentType.ToLower();
            
            if (!IsSupported(file.FileName, contentType))
            {
                return new FileProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"Unsupported file format: {fileExtension}"
                };
            }

            var layerType = DetectFileType(file.FileName, contentType);

            // Route to appropriate processor
            return layerType switch
            {
                // Vector formats
                LayerTypeEnum.GEOJSON => await _vectorProcessor.ProcessGeoJSON(file, layerName),
                LayerTypeEnum.KML => await _vectorProcessor.ProcessKML(file, layerName),
                LayerTypeEnum.Shapefile => await _vectorProcessor.ProcessShapefile(file, layerName),
                LayerTypeEnum.GPX => await _vectorProcessor.ProcessGPX(file, layerName),
                
                // Spreadsheet formats
                LayerTypeEnum.CSV => await _spreadsheetProcessor.ProcessCSV(file, layerName, new SpreadsheetConfig()),
                LayerTypeEnum.Excel => await _spreadsheetProcessor.ProcessExcel(file, layerName, new SpreadsheetConfig()),
                
                // Raster formats
                LayerTypeEnum.GeoTIFF => await _rasterProcessor.ProcessGeoTIFF(file, layerName),
                LayerTypeEnum.PNG or LayerTypeEnum.JPG => await _rasterProcessor.ProcessImageFile(file, layerName, new double[4]),
                
                _ => new FileProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"No processor available for {layerType}"
                }
            };
        }
        catch (Exception ex)
        {
            return new FileProcessingResult
            {
                Success = false,
                ErrorMessage = $"File processing failed: {ex.Message}"
            };
        }
    }

    public LayerTypeEnum DetectFileType(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        
        return extension switch
        {
            ".geojson" or ".json" => LayerTypeEnum.GEOJSON,
            ".kml" => LayerTypeEnum.KML,
            ".shp" => LayerTypeEnum.Shapefile,
            ".gpx" => LayerTypeEnum.GPX,
            ".csv" => LayerTypeEnum.CSV,
            ".xlsx" or ".xls" => LayerTypeEnum.Excel,
            ".tif" or ".tiff" => LayerTypeEnum.GeoTIFF,
            ".png" => LayerTypeEnum.PNG,
            ".jpg" or ".jpeg" => LayerTypeEnum.JPG,
            _ => LayerTypeEnum.GEOJSON // Default fallback
        };
    }

    public bool IsSupported(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        var supportedExtensions = new[]
        {
            // Vector
            ".geojson", ".json", ".kml", ".shp", ".gpx",
            // Spreadsheet  
            ".csv", ".xlsx", ".xls",
            // Raster
            ".tif", ".tiff", ".png", ".jpg", ".jpeg"
        };

        return supportedExtensions.Contains(extension);
    }
}
