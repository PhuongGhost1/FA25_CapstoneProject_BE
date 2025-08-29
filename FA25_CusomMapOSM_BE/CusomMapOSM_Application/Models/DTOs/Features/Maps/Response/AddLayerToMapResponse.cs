namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class AddLayerToMapResponse
    {
        public Guid MapLayerId { get; set; }
        public string Message { get; set; } = "Layer added to map successfully";
    }
}
