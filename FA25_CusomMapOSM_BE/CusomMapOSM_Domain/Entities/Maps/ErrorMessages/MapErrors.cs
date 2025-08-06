using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Maps.ErrorMessages;

public class MapErrors
{
    public const string MapNotFound = "The requested map could not be found.";
    public const string MapCreationFailed = "Failed to create the map due to an internal error.";
    public const string MapUpdateFailed = "Failed to update the map due to an internal error.";
    public const string MapDeletionFailed = "Failed to delete the map due to an internal error.";
    public const string InvalidMapId = "The provided map ID is invalid.";
    public const string MapAlreadyExists = "A map with the same name already exists.";
}

public class MapHistoryErrors
{
    public const string HistoryNotFound = "The requested map history could not be found.";
    public const string HistoryCreationFailed = "Failed to create the map history due to an internal error.";
    public const string HistoryUpdateFailed = "Failed to update the map history due to an internal error.";
    public const string HistoryDeletionFailed = "Failed to delete the map history due to an internal error.";
    public const string InvalidHistoryId = "The provided history ID is invalid.";
}

public class MapLayerErrors
{
    public const string LayerNotFound = "The requested map layer could not be found.";
    public const string LayerCreationFailed = "Failed to create the map layer due to an internal error.";
    public const string LayerUpdateFailed = "Failed to update the map layer due to an internal error.";
    public const string LayerDeletionFailed = "Failed to delete the map layer due to an internal error.";
    public const string InvalidLayerId = "The provided layer ID is invalid.";
}

public class MapTemplateErrors
{
    public const string TemplateNotFound = "The requested map template could not be found.";
    public const string TemplateCreationFailed = "Failed to create the map template due to an internal error.";
    public const string TemplateUpdateFailed = "Failed to update the map template due to an internal error.";
    public const string TemplateDeletionFailed = "Failed to delete the map template due to an internal error.";
    public const string InvalidTemplateId = "The provided template ID is invalid.";
}