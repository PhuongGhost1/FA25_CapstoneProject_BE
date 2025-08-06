CustomMapOSM - Detailed Design

1. Domain Models:
1.1 Map Aggregate:
- Map (root)
  - Layers (1-n)
  - Annotations (1-n)
  - Bookmarks (1-n)
  
1.2 Organization Aggregate:
- Organization (root)
  - Members (1-n)
  - Locations (1-n)
  - Memberships (1-n)

2. Class Specifications:
2.1 MapService:
+ CreateMap(userId, templateId): Map
+ AddLayer(mapId, layerData): void
+ ExportMap(mapId, format): ExportResult

2.2 ExportService:
+ GeneratePdf(mapConfig): byte[]
+ CreateImageTile(mapConfig, zoom): byte[]
+ TrackQuotaUsage(membershipId)

3. Database Design:
- Spatial Indexes: 
  CREATE SPATIAL INDEX idx_locations ON organization_locations(latitude, longitude);
  
- Materialized View:
  CREATE VIEW membership_usage_view AS
  SELECT m.membership_id, p.export_quota, ...
  FROM memberships m JOIN plans p ...

4. Algorithms:
4.1 Spatial Query Optimization:
- Use R-Tree indexing for location searches
- Implement GeoHash for proximity calculations

4.2 Map Rendering:
- Vector tile generation using Mapnik
- PDF composition with Headless Chrome

5. API Contracts:
POST /api/maps/export
{
  "mapId": "uuid",
  "format": "PDF|PNG|SVG",
  "resolution": 300
}

Response:
{
  "downloadUrl": "https://storage/export.pdf",
  "fileSize": 452121,
  "remainingQuota": 12
}