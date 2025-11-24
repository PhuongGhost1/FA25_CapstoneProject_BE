namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;

public class MapPinsDataDto
{
    public Guid SessionQuestionId { get; set; }
    public List<MapPinEntryDto> Pins { get; set; } = new();
    public int TotalResponses { get; set; }
    public decimal? CorrectLatitude { get; set; }
    public decimal? CorrectLongitude { get; set; }
    public int? AcceptanceRadiusMeters { get; set; }
}

public class MapPinEntryDto
{
    public Guid ParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool IsCorrect { get; set; }
    public double DistanceFromCorrect { get; set; } // in meters
    public int PointsEarned { get; set; }
}

