using Planning.Domain.Availability;
using Planning.Domain.Users;

namespace Planning.Application.Availability;

internal static class AvailabilityMapper
{
    public static AvailabilityResponse ToResponse(
        EmployeeAvailability availability,
        IReadOnlyDictionary<Guid, User> usersById)
    {
        var modifier = usersById.GetValueOrDefault(availability.LastModifiedByUserId);

        return new AvailabilityResponse(
            availability.Id,
            availability.UserId,
            availability.Date,
            availability.Type,
            availability.DayPart,
            availability.StartTime,
            availability.EndTime,
            availability.IsAvailable,
            availability.Source,
            availability.LastModifiedByUserId,
            FormatUserName(modifier),
            availability.Note,
            availability.CreatedAtUtc,
            availability.UpdatedAtUtc);
    }

    private static string FormatUserName(User? user)
    {
        if (user is null)
        {
            return "Onbekend";
        }

        return $"{user.FirstName} {user.LastName}".Trim();
    }
}
