namespace CusomMapOSM_Application.Interfaces;

public interface IMapHub
{
    Task JoinMap(string mapId);
    Task UpdateMap(string mapId, MapEditOperation operation);
    Task UpdateCursor(string mapId, double lat, double lng);
}