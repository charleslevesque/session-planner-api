using FluentAssertions;
using SessionPlanner.Core.Entities;
using SessionPlanner.Core.Enums;

namespace SessionPlanner.Tests.Unit.Entities;

public class SessionTests
{
    [Fact]
    public void Session_ShouldInitializeWithDefaults()
    {
        var session = new Session();

        session.Id.Should().Be(0);
        session.Title.Should().Be(string.Empty);
        session.Status.Should().Be(SessionStatus.Draft);
    }

    [Fact]
    public void Session_ShouldSetAllProperties()
    {
        var start = new DateTime(2026, 1, 15);
        var end = new DateTime(2026, 5, 15);

        var session = new Session
        {
            Id = 1,
            Title = "Hiver 2026",
            Status = SessionStatus.Open,
            StartDate = start,
            EndDate = end
        };

        session.Id.Should().Be(1);
        session.Title.Should().Be("Hiver 2026");
        session.Status.Should().Be(SessionStatus.Open);
        session.StartDate.Should().Be(start);
        session.EndDate.Should().Be(end);
    }

    [Theory]
    [InlineData("Hiver 2026")]
    [InlineData("Automne 2025")]
    [InlineData("")]
    [InlineData("Session with special chars: éàü")]
    public void Session_Title_ShouldAcceptVariousValues(string title)
    {
        var session = new Session { Title = title };

        session.Title.Should().Be(title);
    }

    [Theory]
    [InlineData(SessionStatus.Draft)]
    [InlineData(SessionStatus.Open)]
    [InlineData(SessionStatus.Closed)]
    [InlineData(SessionStatus.Archived)]
    public void Session_Status_ShouldAcceptAllValues(SessionStatus status)
    {
        var session = new Session { Status = status };

        session.Status.Should().Be(status);
    }

    [Fact]
    public void Session_TeachingNeeds_ShouldInitializeAsEmptyCollection()
    {
        var session = new Session();

        session.TeachingNeeds.Should().NotBeNull().And.BeEmpty();
    }
}
