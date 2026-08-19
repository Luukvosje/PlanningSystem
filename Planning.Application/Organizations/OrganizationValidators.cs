using System.Globalization;
using FluentValidation;
using Planning.Domain.Enums;
using Planning.Domain.Organizations;

namespace Planning.Application.Organizations;

/// <summary>
/// One rule set for both the create and update request; they validate the same fields, and
/// keeping two copies meant a rule could be tightened on one path and not the other.
/// </summary>
public abstract class OrganizationRequestValidator<T> : AbstractValidator<T>
    where T : IOrganizationRequestFields
{
    protected OrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
    }
}

public class CreateOrganizationRequestValidator : OrganizationRequestValidator<CreateOrganizationRequest>;

public class UpdateOrganizationRequestValidator : OrganizationRequestValidator<UpdateOrganizationRequest>;

public class UpdateOrganizationPlanningSettingsRequestValidator
    : AbstractValidator<UpdateOrganizationPlanningSettingsRequest>
{
    public UpdateOrganizationPlanningSettingsRequestValidator()
    {
        RuleForEach(x => x.ImportantWorkTimes).ChildRules(entry =>
        {
            entry.RuleFor(x => x.Label).MaximumLength(50);

            entry.RuleFor(x => x.StartTime)
                .Must(BeValidTime)
                .WithMessage("Start time must use HH:mm format.");
        });

        RuleFor(x => x.ImportantWorkTimes)
            .Must(times => times.Count <= 24)
            .WithMessage("At most 24 important work times are allowed.");

        RuleForEach(x => x.OpeningHours).ChildRules(entry =>
        {
            entry.RuleFor(x => x.OpenTime)
                .Must(BeValidTime)
                .WithMessage("Open time must use HH:mm format.");

            entry.RuleFor(x => x.CloseTime)
                .Must(BeValidTime)
                .WithMessage("Close time must use HH:mm format.");

            entry.RuleFor(x => x)
                .Must(x => ParseTime(x.OpenTime) < ParseTime(x.CloseTime))
                .WithMessage("Open time must be before close time.");
        });

        RuleFor(x => x.OpeningHours)
            .Must(hours => hours.Select(entry => entry.Day).Distinct().Count() == hours.Count)
            .WithMessage("Each weekday can only appear once in opening hours.");
    }

    private static bool BeValidTime(string value) =>
        TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private static TimeOnly ParseTime(string value) =>
        TimeOnly.ParseExact(value, "HH:mm", CultureInfo.InvariantCulture);
}

internal static class OrganizationPlanningSettingsParser
{
    public static IReadOnlyList<ImportantWorkTime> ParseImportantWorkTimes(
        IReadOnlyList<ImportantWorkTimeRequest> values) =>
        values
            .Select(entry => new ImportantWorkTime(
                entry.Label,
                TimeOnly.ParseExact(entry.StartTime, "HH:mm", CultureInfo.InvariantCulture)))
            .ToList();

    public static IReadOnlyList<DayOpeningHours> ParseOpeningHours(
        IReadOnlyList<OpeningHoursEntryRequest> values) =>
        values
            .Select(entry => new DayOpeningHours(
                entry.Day,
                TimeOnly.ParseExact(entry.OpenTime, "HH:mm", CultureInfo.InvariantCulture),
                TimeOnly.ParseExact(entry.CloseTime, "HH:mm", CultureInfo.InvariantCulture)))
            .ToList();
}
