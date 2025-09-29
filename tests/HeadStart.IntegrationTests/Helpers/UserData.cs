namespace HeadStart.IntegrationTests.Helpers;

public static class Users
{
    public static UserData User1  => new UserData
    {
        IdpId = Guid.Parse("580B8928-84DF-465C-9A02-33E9422841A1"),
        UserFirstName = "FirstName1",
        UserLastName = "LastName1",
        UserName = "user1@example.com",
        UserEmail = "user1@example.com",
        UserPassword = "user1"
    };

    public static UserData User2  => new UserData
    {
        IdpId = Guid.Parse("09AE6917-0BAE-4D1C-A757-20CE3417152F"),
        UserFirstName = "FirstName2",
        UserLastName = "LastName2",
        UserName = "user2@example.com",
        UserEmail = "user2@example.com",
        UserPassword = "user2"
    };

    public static UserData UserApiTest1  => new UserData
    {
        IdpId = Guid.Parse("A599B326-C0BF-4F29-91CF-463ADA378253"),
        UserFirstName = "UserApiTest1P",
        UserLastName = "UserApiTest1N",
        UserName = "user.api.1@test.com",
        UserEmail = "user.api.1@test.com",
        UserPassword = "user.api.1"
    };

    public static UserData UserUiTest1  => new UserData
    {
        IdpId = Guid.Parse("51DC8821-CED1-434B-9D35-22A1B6BD6080"),
        UserFirstName = "UserUiTest1P",
        UserLastName = "UserUiTest1N",
        UserName = "user.ui.1@test.com",
        UserEmail = "user.ui.1@test.com",
        UserPassword = "user.ui.1"
    };

    public static UserData AdminApiTest1  => new UserData
    {
        IdpId = Guid.Parse("1CD0057D-F96B-4D6F-A2FF-393C1CFEF71C"),
        UserFirstName = "AdminApiTest1P",
        UserLastName = "AdminApiTest1N",
        UserName = "admin.api.1@test.com",
        UserEmail = "admin.api.1@test.com",
        UserPassword = "admin.api.1"
    };

    public static UserData AdminUiTest1  => new UserData
    {
        IdpId = Guid.Parse("AD9B256F-6541-485E-B17B-6451449AD980"),
        UserFirstName = "AdminUiTest1P",
        UserLastName = "AdminUiTest1N",
        UserName = "admin.ui.1@test.com",
        UserEmail = "admin.ui.1@test.com",
        UserPassword = "admin.ui.1"
    };
}

public class UserData
{
        public Guid IdpId { get; set; }
        public string UserFirstName { get; init; } = string.Empty;
        public string UserLastName { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string UserEmail { get; init; } = string.Empty;
        public string UserPassword { get; init; } = string.Empty;
}
