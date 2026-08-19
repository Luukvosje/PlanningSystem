namespace Planning.Domain.Organizations;

public sealed class ImportantWorkTime
{
    public string? Label { get; init; }
    public TimeOnly StartTime { get; init; }

    private ImportantWorkTime()
    {
    }

    public ImportantWorkTime(string? label, TimeOnly startTime)
    {
        Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        StartTime = startTime;
    }
}
