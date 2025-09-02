namespace CusomMapOSM_Domain.Entities.Layers.Enums;

public enum LayerTypeEnum
{
    GEOJSON = 1,
    KML = 2,
    Shapefile = 3,
    GPX = 4,
    CSV = 5,
    Excel = 6,
    
    GeoTIFF = 10,
    PNG = 11,
    JPG = 12,
    
    XYZ = 20,
    WMS = 21,
    WMTS = 22,
    TMS = 23,
    
    MVT = 30, 
    PMTiles = 31, 
    
    Database = 40,
    API = 41,
    Stream = 42
}