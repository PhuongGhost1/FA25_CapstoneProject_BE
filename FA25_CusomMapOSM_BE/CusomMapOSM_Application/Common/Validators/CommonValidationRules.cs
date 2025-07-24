using FluentValidation;

namespace CusomMapOSM_Application.Common.Validators;

/// <summary>
/// Common validation rules that can be reused across validators
/// </summary>
public static class CommonValidationRules
{
    /// <summary>
    /// Validates a required string with length constraints
    /// </summary>
    public static IRuleBuilderOptions<T, string> RequiredString<T>(this IRuleBuilder<T, string> ruleBuilder, int minLength = 1, int maxLength = 255)
    {
        return ruleBuilder
                .NotEmpty().WithMessage("Field is required")
                .Length(minLength, maxLength).WithMessage($"Length must be between {minLength} and {maxLength} characters");
    }

    /// <summary>
    /// Validates ISO 8601 timestamp format
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidTimestamp<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
                .NotEmpty().WithMessage("Timestamp is required")
                .Must(BeValidTimestamp).WithMessage("Invalid timestamp format");
    }

    private static bool BeValidTimestamp(string timestamp)
    {
        if (string.IsNullOrEmpty(timestamp)) return true;

        return DateTimeOffset.TryParse(timestamp, out var parsedDate) &&
                     parsedDate >= DateTimeOffset.UtcNow.AddYears(-1) &&
                     parsedDate <= DateTimeOffset.UtcNow.AddHours(1);
    }
}
