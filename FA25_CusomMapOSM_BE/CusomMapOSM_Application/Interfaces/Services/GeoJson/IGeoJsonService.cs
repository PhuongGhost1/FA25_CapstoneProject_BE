using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CusomMapOSM_Application.Models.DTOs.Services.GeoJson;

namespace CusomMapOSM_Application.Interfaces.Services.GeoJson;

public interface IGeoJsonService
{

    GeoJsonLayerData ProcessGeoJsonUpload(string geoJsonString, string layerName);
    
    bool ValidateGeoJson(string geoJsonString);
    
    string CalculateBounds(string geoJsonString);
    
    string GenerateDefaultStyle(string geoJsonString);
}
