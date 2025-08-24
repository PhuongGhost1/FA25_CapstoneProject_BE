using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using FluentValidation;

namespace CusomMapOSM_Application.Models.Validators.Organization;

public class OrganizationReqDtoValidator : AbstractValidator<OrganizationReqDto>
{
    public OrganizationReqDtoValidator()
    {
        RuleFor(x => x.OrgName)
            .NotEmpty().WithMessage("Organization name is required")
            .Length(1, 100).WithMessage("Organization name must be between 1 and 100 characters");

        RuleFor(x => x.Abbreviation)
            .NotEmpty().WithMessage("Abbreviation is required")
            .Length(1, 10).WithMessage("Abbreviation must be between 1 and 10 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}