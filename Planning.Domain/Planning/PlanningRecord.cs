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
        PlanningStatus status = PlanningStatus.Planned)
    {
        ValidateTitle(title);
        ValidateDateRange(startUtc, endUtc);
        ValidateColor(color);

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
        string color,
        DateTime utcNow)
    {
        ValidateTitle(title);
        ValidateDateRange(startUtc, endUtc);
        ValidateColor(color);

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

    public void Resize(DateTime startUtc, DateTime endUtc, DateTime utcNow)
    {
        ValidateDateRange(startUtc, endUtc);

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

    public void ChangeStatus(PlanningStatus status, DateTime utcNow)
    {
        Status = status;
        Touch(utcNow);
    }

    public void UpdateNotes(string? notes, DateTime utcNow)
    {
        Notes = notes?.Trim();
        Touch(utcNow);
    }

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

    private static void ValidateColor(string? color)
    {
        if (color is null)
        {
            return;
        }

        if (!IsValidHexColor(color))
        {
            throw new ArgumentException("Color must be a valid hex color (e.g. #6366F1).", nameof(color));
        }
    }

    private static string NormalizeColor(string? color) =>
        string.IsNullOrWhiteSpace(color) ? DefaultColor : color.Trim().ToUpperInvariant();

    private static bool IsValidHexColor(string color) =>
        color.Length == 7 && color[0] == '#' && color[1..].All(Uri.IsHexDigit);
}
