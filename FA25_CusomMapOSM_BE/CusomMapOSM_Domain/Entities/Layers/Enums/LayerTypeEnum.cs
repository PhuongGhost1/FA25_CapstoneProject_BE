namespace CusomMapOSM_Domain.Entities.Layers.Enums;

public enum LayerTypeEnum
{
    // Vector Types
    Roads = 1,
    Buildings = 2,
    POI = 3,
    GEOJSON = 4,
    KML = 5,
    Shapefile = 6,
    GPX = 7,
    
    // Spreadsheet Types  
    CSV = 10,
    Excel = 11,
    
    // Raster Types
    GeoTIFF = 20,
    PNG = 21,
    JPG = 22,
    Satellite = 23,
    DEM = 24,  // Digital Elevation Model
    
    // Tile Services
    WMS = 30,
    WMTS = 31,
    TMS = 32
}