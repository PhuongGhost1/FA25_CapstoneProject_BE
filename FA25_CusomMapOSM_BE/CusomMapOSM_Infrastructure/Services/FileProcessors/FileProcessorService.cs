using CusomMapOSM_Application.Interfaces.Services.FileProcessors;
using CusomMapOSM_Application.Models.DTOs.Features.FileProcessor.Response;
using CusomMapOSM_Commons.Constant;
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
            
            if (!IsSupported(file.FileName))
            {
                return new FileProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"Unsupported file format: {fileExtension}"
                };
            }

            var layerType = DetectFileType(file.FileName);

            // Route to appropriate processor
            return layerType switch
            {
                // Vector formats
                LayerType.GEOJSON => await _vectorProcessor.ProcessGeoJSON(file, layerName),
                LayerType.KML => await _vectorProcessor.ProcessKML(file, layerName),
                LayerType.Shapefile => await _vectorProcessor.ProcessShapefile(file, layerName),
                LayerType.GPX => await _vectorProcessor.ProcessGPX(file, layerName),
                
                // Spreadsheet formats
                LayerType.CSV => await _spreadsheetProcessor.ProcessCSV(file, layerName, new SpreadsheetConfig()),
                LayerType.Excel => await _spreadsheetProcessor.ProcessExcel(file, layerName, new SpreadsheetConfig()),
                
                // Raster formats
                LayerType.GeoTIFF => await _rasterProcessor.ProcessGeoTIFF(file, layerName),
                LayerType.PNG or LayerType.JPG => await _rasterProcessor.ProcessImageFile(file, layerName, new double[4]),
                
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

    public LayerType DetectFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        
        return extension switch
        {
            FileExtensionConstant.Vector.GEOJSON or FileExtensionConstant.Vector.JSON => LayerType.GEOJSON,
            FileExtensionConstant.Vector.KML => LayerType.KML,
            FileExtensionConstant.Vector.SHAPEFILE => LayerType.Shapefile,
            FileExtensionConstant.Vector.GPX => LayerType.GPX,
            FileExtensionConstant.Spreadsheet.CSV => LayerType.CSV,
            FileExtensionConstant.Spreadsheet.XLSX or FileExtensionConstant.Spreadsheet.XLS => LayerType.Excel,
            FileExtensionConstant.Raster.TIF or FileExtensionConstant.Raster.TIFF => LayerType.GeoTIFF,
            FileExtensionConstant.Raster.PNG => LayerType.PNG,
            FileExtensionConstant.Raster.JPG or FileExtensionConstant.Raster.JPEG => LayerType.JPG,
            _ => LayerType.GEOJSON // Default fallback
        };
    }

    public bool IsSupported(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return FileExtensionConstant.ALL_SUPPORTED.Contains(extension);
    }
}
