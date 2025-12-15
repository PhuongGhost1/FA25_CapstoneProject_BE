using System;

namespace CusomMapOSM_Commons.Constant;

public static class MongoDatabaseConstant
{
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
        ?? "mongodb://localhost:27017";  // Fallback for development

    public static string DatabaseName =>
        Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME")
        ?? "custommap_osm_dev";  // Fallback for development

    public static string LayerDataCollectionName =>
        Environment.GetEnvironmentVariable("MONGO_LAYER_COLLECTION")
        ?? "layer_data";
        
    public static string MapFeatureCollectionName =>
        Environment.GetEnvironmentVariable("MONGO_FEATURE_COLLECTION")
        ?? "map_features";
        
    public static string MapHistoryCollectionName =>
        Environment.GetEnvironmentVariable("MONGO_HISTORY_COLLECTION")
        ?? "map_history";
        
    public static string LocationCollectionName =>
        Environment.GetEnvironmentVariable("MONGO_LOCATION_COLLECTION")
        ?? "segment_locations";
}
