namespace CusomMapOSM_Domain.Entities.QuestionBanks.Enums;

public enum QuestionTypeEnum
{
    /// <summary>
    /// Multiple choice question with options (A, B, C, D)
    /// </summary>
    MULTIPLE_CHOICE = 1,

    /// <summary>
    /// True/False question
    /// </summary>
    TRUE_FALSE = 2,

    /// <summary>
    /// Short answer text response
    /// </summary>
    SHORT_ANSWER = 3,

    /// <summary>
    /// Pin on map - students click on the map to mark a location
    /// </summary>
    PIN_ON_MAP = 5
}