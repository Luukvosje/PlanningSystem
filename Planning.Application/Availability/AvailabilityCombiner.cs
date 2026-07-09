using Planning.Domain.Enums;

namespace Planning.Application.Availability;

internal static class AvailabilityCombiner
{
    private static readonly IReadOnlyDictionary<DayPart, (TimeOnly Start, TimeOnly End)> DayPartRanges =
        new Dictionary<DayPart, (TimeOnly Start, TimeOnly End)>
        {
            [DayPart.Morning] = (new TimeOnly(6, 0), new TimeOnly(12, 0)),
            [DayPart.Afternoon] = (new TimeOnly(12, 0), new TimeOnly(17, 0)),
            [DayPart.Evening] = (new TimeOnly(17, 0), new TimeOnly(23, 0)),
        };

    public static IReadOnlyList<AvailabilityResponse> CombineAdjacent(
        IEnumerable<AvailabilityResponse> items)
    {
        var passthrough = new List<AvailabilityResponse>();
        var byUserAndDate = new Dictionary<(Guid UserId, DateOnly Date), List<AvailabilityResponse>>();

        foreach (var item in items)
        {
            if (!TryToUnavailablePeriod(item, out var period))
            {
                passthrough.Add(item);
                continue;
            }

            var key = (item.UserId, item.Date);
            if (!byUserAndDate.TryGetValue(key, out var dayItems))
            {
                dayItems = [];
                byUserAndDate[key] = dayItems;
            }

            dayItems.Add(period);
        }

        var merged = byUserAndDate.Values
            .SelectMany(MergeAdjacentPeriods)
            .ToList();

        return passthrough
            .Concat(merged)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ThenBy(x => x.Type)
            .ToList();
    }

    private static bool TryToUnavailablePeriod(
        AvailabilityResponse item,
        out AvailabilityResponse period)
    {
        period = item;

        if (item.IsAvailable)
        {
            return false;
        }

        if (item.Type == AvailabilityType.DayPart && item.DayPart is { } dayPart)
        {
            var range = DayPartRanges[dayPart];
            period = item with
            {
                StartTime = range.Start,
                EndTime = range.End,
            };
            return true;
        }

        if (item.Type == AvailabilityType.TimeBlock && item.StartTime is not null && item.EndTime is not null)
        {
            return true;
        }

        return false;
    }

    private static IEnumerable<AvailabilityResponse> MergeAdjacentPeriods(
        IReadOnlyList<AvailabilityResponse> periods)
    {
        if (periods.Count == 0)
        {
            yield break;
        }

        var sorted = periods
            .OrderBy(x => x.StartTime)
            .ThenBy(x => x.EndTime)
            .ToList();

        var current = sorted[0];

        for (var i = 1; i < sorted.Count; i++)
        {
            var next = sorted[i];

            if (next.StartTime <= current.EndTime)
            {
                current = MergePeriods(current, next);
                continue;
            }

            yield return ToTimeBlockResponse(current);
            current = next;
        }

        yield return ToTimeBlockResponse(current);
    }

    private static AvailabilityResponse ToTimeBlockResponse(AvailabilityResponse item) =>
        item with
        {
            Type = AvailabilityType.TimeBlock,
            DayPart = null,
        };

    private static AvailabilityResponse MergePeriods(
        AvailabilityResponse first,
        AvailabilityResponse second)
    {
        var latest = first.UpdatedAtUtc >= second.UpdatedAtUtc ? first : second;
        var earliestStart = first.StartTime <= second.StartTime ? first.StartTime : second.StartTime;
        var latestEnd = first.EndTime >= second.EndTime ? first.EndTime : second.EndTime;

        return first with
        {
            StartTime = earliestStart,
            EndTime = latestEnd,
            Note = CombineNotes(first.Note, second.Note),
            Source = latest.Source,
            LastModifiedByUserId = latest.LastModifiedByUserId,
            LastModifiedByName = latest.LastModifiedByName,
            UpdatedAtUtc = latest.UpdatedAtUtc,
        };
    }

    private static string? CombineNotes(string? first, string? second)
    {
        if (string.IsNullOrWhiteSpace(first))
        {
            return string.IsNullOrWhiteSpace(second) ? null : second;
        }

        if (string.IsNullOrWhiteSpace(second) || string.Equals(first, second, StringComparison.Ordinal))
        {
            return first;
        }

        return $"{first} · {second}";
    }
}
