namespace CusomMapOSM_Commons.Constant;

public static class FileExtensionConstant
{
    // Vector file extensions
    public static class Vector
    {
        public const string GEOJSON = ".geojson";
        public const string JSON = ".json";
        public const string KML = ".kml";
        public const string SHAPEFILE = ".shp";
        public const string GPX = ".gpx";
        
        public static readonly string[] ALL = { GEOJSON, JSON, KML, SHAPEFILE, GPX };
    }
    
    // Spreadsheet file extensions
    public static class Spreadsheet
    {
        public const string CSV = ".csv";
        public const string XLSX = ".xlsx";
        public const string XLS = ".xls";
        
        public static readonly string[] ALL = { CSV, XLSX, XLS };
    }
    
    // Raster file extensions
    public static class Raster
    {
        public const string TIF = ".tif";
        public const string TIFF = ".tiff";
        public const string PNG = ".png";
        public const string JPG = ".jpg";
        public const string JPEG = ".jpeg";
        
        public static readonly string[] ALL = { TIF, TIFF, PNG, JPG, JPEG };
    }
    
    public static readonly string[] ALL_SUPPORTED = Vector.ALL
        .Concat(Spreadsheet.ALL)
        .Concat(Raster.ALL)
        .ToArray();
}
