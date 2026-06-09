using Conduit.Domain.Contracts;
using Shouldly;

namespace Conduit.UnitTests.UnitTests;

public sealed class DomainContractsTests
{
    [Fact]
    public void IDomainEvent_ShouldBePublicMarkerInterface()
    {
        Type domainEventType = typeof(IDomainEvent);

        domainEventType.IsInterface.ShouldBeTrue();
        domainEventType.IsPublic.ShouldBeTrue();
        domainEventType.GetMembers()
            .Where(member => member.DeclaringType == domainEventType)
            .ShouldBeEmpty();
    }

    [Fact]
    public void DomainEventImplementation_ShouldBeAssignableToIDomainEvent()
    {
        var domainEvent = new SampleDomainEvent(Guid.NewGuid());

        domainEvent.ShouldBeAssignableTo<IDomainEvent>();
    }

    private sealed record SampleDomainEvent(Guid EventId) : IDomainEvent;
}
