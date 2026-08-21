using Planning.Domain.Common;
using Planning.Domain.Enums;
using Planning.Domain.Planning;

namespace Planning.Domain.Planning;

public class PlanningRecord : TenantEntity
{
    public const string DefaultColor = "#6366F1";
    public static readonly TimeSpan MinimumDuration = TimeSpan.FromMinutes(15);

    public Guid? CustomerId { get; private set; }
    public Guid AssignedUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Notes { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public PlanningStatus Status { get; private set; }
    public string Color { get; private set; } = DefaultColor;

    private PlanningRecord()
    {
    }

    private PlanningRecord(
        Guid id,
        Guid organizationId,
        Guid? customerId,
        Guid assignedUserId,
        string title,
        string? description,
        string? notes,
        DateTime startUtc,
        DateTime endUtc,
        PlanningStatus status,
        string color,
        DateTime utcNow)
        : base(id, organizationId, utcNow, utcNow)
    {
        CustomerId = customerId;
        AssignedUserId = assignedUserId;
        Title = title;
        Description = description;
        Notes = notes;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Status = status;
        Color = color;
    }

    public static PlanningRecord Create(
        Guid organizationId,
        Guid? customerId,
        Guid assignedUserId,
        string title,
        string? description,
        string? notes,
        DateTime startUtc,
        DateTime endUtc,
        string? color,
        DateTime utcNow,
        PlanningStatus status = PlanningStatus.Confirmed)
    {
        ValidateTitle(title);
        ValidateDateRange(startUtc, endUtc);

        return new PlanningRecord(
            Guid.NewGuid(),
            organizationId,
            customerId,
            assignedUserId,
            title.Trim(),
            description?.Trim(),
            notes?.Trim(),
            startUtc,
            endUtc,
            status,
            NormalizeColor(color),
            utcNow);
    }

    public void Update(
        Guid? customerId,
        Guid assignedUserId,
        string title,
        string? description,
        string? notes,
        DateTime startUtc,
        DateTime endUtc,
        string? color,
        DateTime utcNow)
    {
        ValidateTitle(title);
        ValidateDateRange(startUtc, endUtc);

        CustomerId = customerId;
        AssignedUserId = assignedUserId;
        Title = title.Trim();
        Description = description?.Trim();
        Notes = notes?.Trim();
        StartUtc = startUtc;
        EndUtc = endUtc;
        Color = NormalizeColor(color);
        Touch(utcNow);
    }

    public void Move(
        Guid assignedUserId,
        Guid? customerId,
        DateTime startUtc,
        DateTime endUtc,
        DateTime utcNow)
    {
        ValidateDateRange(startUtc, endUtc);

        AssignedUserId = assignedUserId;
        CustomerId = customerId;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Touch(utcNow);
    }

    public PlanningRecord Duplicate(
        Guid? assignedUserId,
        DateTime startUtc,
        DateTime endUtc,
        DateTime utcNow)
    {
        ValidateDateRange(startUtc, endUtc);

        return new PlanningRecord(
            Guid.NewGuid(),
            OrganizationId,
            CustomerId,
            assignedUserId ?? AssignedUserId,
            $"{Title} (kopie)",
            Description,
            Notes,
            startUtc,
            endUtc,
            PlanningStatus.Planned,
            Color,
            utcNow);
    }

    /// <summary>
    /// Allowed status transitions. Completed and Cancelled are terminal: a booking that already
    /// ended cannot be revived. Every path that changes a status goes through
    /// <see cref="ChangeStatus"/>, so the rule cannot be bypassed by updating a record directly.
    /// </summary>
    private static readonly IReadOnlyDictionary<PlanningStatus, PlanningStatus[]> AllowedTransitions =
        new Dictionary<PlanningStatus, PlanningStatus[]>
        {
            [PlanningStatus.Planned] = [PlanningStatus.Confirmed, PlanningStatus.Cancelled],
            [PlanningStatus.Confirmed] = [PlanningStatus.Completed, PlanningStatus.Cancelled],
            [PlanningStatus.Completed] = [],
            [PlanningStatus.Cancelled] = [],
        };

    public void ChangeStatus(PlanningStatus status, DateTime utcNow)
    {
        if (status == Status)
        {
            return;
        }

        if (!AllowedTransitions.GetValueOrDefault(Status, []).Contains(status))
        {
            // Fixed strings rather than interpolated: the frontend translates backend messages by
            // exact match (Planning.Web/app/utils/backendMessages.ts), so an interpolated message
            // would always reach a Dutch user in English.
            throw new ArgumentException(
                Status is PlanningStatus.Completed or PlanningStatus.Cancelled
                    ? "A completed or cancelled booking can no longer change status."
                    : "This status change is not allowed.");
        }

        Status = status;
        Touch(utcNow);
    }

    public void Confirm(DateTime utcNow) => ChangeStatus(PlanningStatus.Confirmed, utcNow);

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }
    }

    private static void ValidateDateRange(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
        {
            throw new ArgumentException("End date must be after start date.");
        }

        if (endUtc - startUtc < MinimumDuration)
        {
            throw new ArgumentException($"Duration must be at least {MinimumDuration.TotalMinutes} minutes.");
        }
    }

    /// <summary>
    /// Trims and upper-cases the colour, falling back to <see cref="DefaultColor"/> when none is
    /// given. Validation happens on the normalized value, so surrounding whitespace is tolerated.
    /// </summary>
    private static string NormalizeColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return DefaultColor;
        }

        var normalized = color.Trim().ToUpperInvariant();

        if (!IsValidHexColor(normalized))
        {
            throw new ArgumentException("Color must be a valid hex color (e.g. #6366F1).", nameof(color));
        }

        return normalized;
    }

    private static bool IsValidHexColor(string color) =>
        color.Length == 7 && color[0] == '#' && color[1..].All(Uri.IsHexDigit);
}
