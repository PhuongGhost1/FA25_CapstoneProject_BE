namespace CusomMapOSM_Application.Models.DTOs.Features.Sessions.Response;

public class WordCloudDataDto
{
    public Guid SessionQuestionId { get; set; }
    public List<WordCloudEntryDto> Entries { get; set; } = new();
    public int TotalResponses { get; set; }
}

public class WordCloudEntryDto
{
    public string Word { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Frequency { get; set; } // Percentage 0-100
}

