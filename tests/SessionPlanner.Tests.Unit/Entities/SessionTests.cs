using FluentAssertions;
using SessionPlanner.Core.Entities;

namespace SessionPlanner.Tests.Unit.Entities;

public class SessionTests
{
    [Fact]
    public void Session_ShouldInitializeWithDefaults()
    {
        var session = new Session();

        session.Id.Should().Be(0);
        session.Title.Should().Be(string.Empty);
        session.Date.Should().Be(default);
    }

    [Fact]
    public void Session_ShouldSetAllProperties()
    {
        var date = new DateTime(2026, 3, 15, 10, 0, 0);

        var session = new Session
        {
            Id = 1,
            Title = "Lab Session",
            Date = date
        };

        session.Id.Should().Be(1);
        session.Title.Should().Be("Lab Session");
        session.Date.Should().Be(date);
    }

    [Theory]
    [InlineData("Morning Session")]
    [InlineData("Afternoon Workshop")]
    [InlineData("")]
    [InlineData("Session with special chars: éàü")]
    public void Session_Title_ShouldAcceptVariousValues(string title)
    {
        var session = new Session { Title = title };

        session.Title.Should().Be(title);
    }

    [Fact]
    public void Session_Date_ShouldHandleVariousDates()
    {
        var dates = new[]
        {
            DateTime.MinValue,
            DateTime.MaxValue,
            new DateTime(2026, 1, 1),
            DateTime.Now
        };

        foreach (var date in dates)
        {
            var session = new Session { Date = date };

            session.Date.Should().Be(date);
        }
    }
}
