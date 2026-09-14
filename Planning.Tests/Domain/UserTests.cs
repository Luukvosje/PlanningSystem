using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Tests.Domain;

public class UserTests
{
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 14, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void A_member_without_an_account_can_be_created_by_name_alone()
    {
        var user = User.CreateWithoutAccount(OrganizationId, " Kevin ", "Jansen", "  ", UserRole.Employee, Now);

        Assert.False(user.HasAccount);
        Assert.Null(user.Email);
        Assert.Equal("Kevin", user.FirstName);
    }

    [Fact]
    public void Linking_keeps_the_planner_entered_email_and_fills_a_missing_one()
    {
        var withEmail = User.CreateWithoutAccount(OrganizationId, "Kevin", "Jansen", "werk@acme.nl", UserRole.Employee, Now);
        var withoutEmail = User.CreateWithoutAccount(OrganizationId, "Kevin", "Jansen", null, UserRole.Employee, Now);
        var accountId = Guid.NewGuid();

        withEmail.LinkAccount(accountId, "Prive@Mail.nl", Now);
        withoutEmail.LinkAccount(accountId, "Prive@Mail.nl", Now);

        Assert.Equal("werk@acme.nl", withEmail.Email);
        Assert.Equal("prive@mail.nl", withoutEmail.Email);
        Assert.True(withEmail.HasAccount);
    }

    [Fact]
    public void A_member_can_only_be_linked_once()
    {
        var user = User.CreateWithoutAccount(OrganizationId, "Kevin", "Jansen", null, UserRole.Employee, Now);
        user.LinkAccount(Guid.NewGuid(), "kevin@acme.nl", Now);

        Assert.Throws<InvalidOperationException>(() => user.LinkAccount(Guid.NewGuid(), "ander@acme.nl", Now));
    }
}
