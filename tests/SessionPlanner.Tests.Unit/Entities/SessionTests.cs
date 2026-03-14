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
        session.OpenedAt.Should().BeNull();
        session.ClosedAt.Should().BeNull();
        session.ArchivedAt.Should().BeNull();
        session.CreatedByUserId.Should().BeNull();
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
            EndDate = end,
            CreatedByUserId = 42
        };

        session.Id.Should().Be(1);
        session.Title.Should().Be("Hiver 2026");
        session.Status.Should().Be(SessionStatus.Open);
        session.StartDate.Should().Be(start);
        session.EndDate.Should().Be(end);
        session.CreatedByUserId.Should().Be(42);
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

    [Fact]
    public void Session_TeachingNeeds_ShouldInitializeAsEmptyCollection()
    {
        var session = new Session();
        session.TeachingNeeds.Should().NotBeNull().And.BeEmpty();
    }

    #region TransitionTo — Valid transitions

    [Fact]
    public void TransitionTo_DraftToOpen_ShouldSucceed()
    {
        var session = new Session { Status = SessionStatus.Draft };

        session.TransitionTo(SessionStatus.Open);

        session.Status.Should().Be(SessionStatus.Open);
        session.OpenedAt.Should().NotBeNull();
        session.OpenedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TransitionTo_OpenToClosed_ShouldSucceed()
    {
        var session = new Session { Status = SessionStatus.Open };

        session.TransitionTo(SessionStatus.Closed);

        session.Status.Should().Be(SessionStatus.Closed);
        session.ClosedAt.Should().NotBeNull();
        session.ClosedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TransitionTo_ClosedToArchived_ShouldSucceed()
    {
        var session = new Session { Status = SessionStatus.Closed };

        session.TransitionTo(SessionStatus.Archived);

        session.Status.Should().Be(SessionStatus.Archived);
        session.ArchivedAt.Should().NotBeNull();
        session.ArchivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void TransitionTo_FullLifecycle_ShouldRecordAllTimestamps()
    {
        var session = new Session();

        session.TransitionTo(SessionStatus.Open);
        session.TransitionTo(SessionStatus.Closed);
        session.TransitionTo(SessionStatus.Archived);

        session.Status.Should().Be(SessionStatus.Archived);
        session.OpenedAt.Should().NotBeNull();
        session.ClosedAt.Should().NotBeNull();
        session.ArchivedAt.Should().NotBeNull();
    }

    #endregion

    #region TransitionTo — Invalid transitions

    [Theory]
    [InlineData(SessionStatus.Draft, SessionStatus.Closed)]
    [InlineData(SessionStatus.Draft, SessionStatus.Archived)]
    [InlineData(SessionStatus.Draft, SessionStatus.Draft)]
    [InlineData(SessionStatus.Open, SessionStatus.Draft)]
    [InlineData(SessionStatus.Open, SessionStatus.Open)]
    [InlineData(SessionStatus.Open, SessionStatus.Archived)]
    [InlineData(SessionStatus.Closed, SessionStatus.Draft)]
    [InlineData(SessionStatus.Closed, SessionStatus.Open)]
    [InlineData(SessionStatus.Closed, SessionStatus.Closed)]
    [InlineData(SessionStatus.Archived, SessionStatus.Draft)]
    [InlineData(SessionStatus.Archived, SessionStatus.Open)]
    [InlineData(SessionStatus.Archived, SessionStatus.Closed)]
    [InlineData(SessionStatus.Archived, SessionStatus.Archived)]
    public void TransitionTo_InvalidTransition_ShouldThrow(SessionStatus from, SessionStatus to)
    {
        var session = new Session { Status = from };

        var act = () => session.TransitionTo(to);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Cannot transition from {from} to {to}.");
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ShouldNotChangeStatus()
    {
        var session = new Session { Status = SessionStatus.Draft };

        try { session.TransitionTo(SessionStatus.Archived); } catch { }

        session.Status.Should().Be(SessionStatus.Draft);
        session.ArchivedAt.Should().BeNull();
    }

    #endregion
}
