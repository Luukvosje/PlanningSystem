using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Planning.Application.Common;
using Planning.Domain.Auth;
using Planning.Domain.Availability;
using Planning.Domain.Customers;
using Planning.Domain.Enums;
using Planning.Domain.Modules;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Infrastructure.Data;

public sealed class TestDataSeeder : ITestDataSeeder
{
    public const string DefaultPassword = "Test1234!";
    public const string OrganizationName = "Demo Administratie";
    public const string OrganizationEmail = "info@demo-administratie.nl";
    public const string OwnerEmail = "medewerker1@test.local";

    private static readonly AppModule[] AllModules =
        [AppModule.Planning, AppModule.Klant, AppModule.Beheer];

    private static readonly (string FirstName, string LastName, UserRole Role)[] Employees =
    [
        ("Jan", "de Vries", UserRole.Owner),
        ("Sanne", "Bakker", UserRole.Admin),
        ("Tom", "Jansen", UserRole.Planner),
        ("Lisa", "Visser", UserRole.Employee),
        ("Mark", "Smit", UserRole.Employee),
        ("Emma", "Meijer", UserRole.Employee),
        ("Daan", "Mulder", UserRole.Employee),
        ("Sophie", "de Boer", UserRole.Employee),
        ("Ruben", "Peters", UserRole.Employee),
        ("Anna", "Hendriks", UserRole.Employee),
    ];

    private static readonly (string Name, string Email, string Address)[] Customers =
    [
        ("Bakkerij Van Dijk", "contact@bakkerij-vandijk.nl", "Hoofdstraat 12, Utrecht"),
        ("Groen & Co Tuincentrum", "info@groenenco.nl", "Kerkweg 45, Amersfoort"),
        ("TechFix IT Solutions", "support@techfix.nl", "Innovatielaan 8, Eindhoven"),
        ("Restaurant De Haven", "reserveren@dehaven.nl", "Havenkade 3, Rotterdam"),
        ("AutoService Peeters", "werkplaats@peetersauto.nl", "Industrieweg 22, Breda"),
        ("Kapsalon Style", "afspraak@kapsalonstyle.nl", "Markt 7, Den Bosch"),
        ("FitLife Sportschool", "info@fitlife.nl", "Sportlaan 19, Arnhem"),
        ("Boekhandel Pagina", "bestellingen@boekhandelpagina.nl", "Schoolstraat 31, Zwolle"),
        ("Dierenartspraktijk Noord", "info@dierenartsnoord.nl", "Dorpsweg 14, Groningen"),
        ("Schilderbedrijf Kleur", "offerte@schilderkleur.nl", "Ambachtsstraat 6, Haarlem"),
    ];

    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<TestDataSeeder> _logger;

    public TestDataSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<TestDataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test mode: resetting database and applying migrations.");
        await _context.Database.EnsureDeletedAsync(cancellationToken);
        await _context.Database.MigrateAsync(cancellationToken);

        var utcNow = DateTime.UtcNow;
        var passwordHash = _passwordHasher.Hash(DefaultPassword);

        var organization = Organization.Create(OrganizationName, OrganizationEmail, utcNow);
        organization.UpdatePlanningSettings(
            OrganizationPlanningDefaults.ImportantWorkTimes,
            CreateDefaultOpeningHours(),
            utcNow);

        var users = new List<User>();
        var accounts = new List<Account>();

        for (var index = 0; index < Employees.Length; index++)
        {
            var (firstName, lastName, role) = Employees[index];
            var email = $"medewerker{index + 1}@test.local";

            var account = Account.Create(email, passwordHash, firstName, lastName, utcNow);
            accounts.Add(account);

            users.Add(User.Create(
                account.Id,
                organization.Id,
                firstName,
                lastName,
                email,
                role,
                utcNow));
        }

        var organizationModules = AllModules
            .Select(module => OrganizationModule.Create(organization.Id, module, isEnabled: true))
            .ToList();

        var userModules = users
            .SelectMany(user => AllModules.Select(module => UserModule.Create(user.Id, module, isEnabled: true)))
            .ToList();

        var customers = Customers
            .Select(entry => Customer.Create(
                organization.Id,
                entry.Name,
                entry.Email,
                entry.Address,
                utcNow))
            .ToList();

        var availabilityRules = CreateAvailabilityRules(organization.Id, users, utcNow);

        _context.Organizations.Add(organization);
        _context.Accounts.AddRange(accounts);
        _context.Users.AddRange(users);
        _context.OrganizationModules.AddRange(organizationModules);
        _context.UserModules.AddRange(userModules);
        _context.Customers.AddRange(customers);
        _context.AvailabilityRules.AddRange(availabilityRules);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Test database seeded with organization '{OrganizationName}', {EmployeeCount} medewerkers and {CustomerCount} klanten.",
            OrganizationName,
            users.Count,
            customers.Count);
        _logger.LogInformation(
            "Login as owner with email '{OwnerEmail}' and password '{DefaultPassword}'.",
            OwnerEmail,
            DefaultPassword);
    }

    private static IReadOnlyList<DayOpeningHours> CreateDefaultOpeningHours() =>
    [
        new(Weekday.Monday, new TimeOnly(6, 0), new TimeOnly(22, 0)),
        new(Weekday.Tuesday, new TimeOnly(6, 0), new TimeOnly(22, 0)),
        new(Weekday.Wednesday, new TimeOnly(6, 0), new TimeOnly(22, 0)),
        new(Weekday.Thursday, new TimeOnly(6, 0), new TimeOnly(22, 0)),
        new(Weekday.Friday, new TimeOnly(6, 0), new TimeOnly(22, 0)),
    ];

    private static List<AvailabilityRule> CreateAvailabilityRules(
        Guid organizationId,
        IReadOnlyList<User> users,
        DateTime utcNow)
    {
        var rules = new List<AvailabilityRule>();
        var weekdays = new[] { Weekday.Monday, Weekday.Tuesday, Weekday.Wednesday, Weekday.Thursday, Weekday.Friday };

        foreach (var user in users.Where(user => user.Role == UserRole.Employee))
        {
            foreach (var weekday in weekdays)
            {
                rules.Add(AvailabilityRule.CreateWeekly(
                    organizationId,
                    user.Id,
                    weekday,
                    new TimeOnly(12, 0),
                    new TimeOnly(13, 0),
                    AvailabilityRuleStatus.Unavailable,
                    "Lunchpauze",
                    utcNow));
            }
        }

        var fridayEmployee = users.First(user => user.Email == "medewerker4@test.local");
        rules.Add(AvailabilityRule.CreateWeekly(
            organizationId,
            fridayEmployee.Id,
            Weekday.Friday,
            new TimeOnly(15, 0),
            new TimeOnly(17, 0),
            AvailabilityRuleStatus.Unavailable,
            "Vrijdagmiddag",
            utcNow));

        rules.Add(AvailabilityRule.CreateOneTime(
            organizationId,
            users.First(user => user.Email == "medewerker5@test.local").Id,
            DateOnly.FromDateTime(utcNow.AddDays(14)),
            new TimeOnly(8, 0),
            new TimeOnly(17, 0),
            AvailabilityRuleStatus.Unavailable,
            "Vakantie",
            utcNow));

        return rules;
    }
}
