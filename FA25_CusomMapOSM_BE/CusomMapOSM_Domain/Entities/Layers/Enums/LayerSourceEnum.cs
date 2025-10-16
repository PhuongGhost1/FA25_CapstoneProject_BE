using System;

namespace CusomMapOSM_Domain.Entities.Layers.Enums;

public enum LayerSourceEnum
{
    OpenStreetMap = 1,
    UserUploaded = 2,
    ExternalAPI = 3,
    Database = 4,
    WebMapService = 5,
    InDbVector = 6
}