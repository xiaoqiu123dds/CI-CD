using Prism.Events;
using PrismReactiveDemo.Core.Events;
using PrismReactiveDemo.Presentation.Core;

namespace PrismReactiveDemo.Tests;

public sealed class EventAggregatorExtensionsTests
{
    [Fact]
    public void Observe_ShouldForwardPublishedPayload()
    {
        var eventAggregator = new EventAggregator();
        var receivedPayloads = new List<string>();

        using var subscription = eventAggregator
            .Observe<StatusMessageEvent, string>()
            .Subscribe(receivedPayloads.Add);

        eventAggregator.GetEvent<StatusMessageEvent>().Publish("hello-ci");

        var actual = Assert.Single(receivedPayloads);
        Assert.Equal("hello-ci", actual);
    }

    [Fact]
    public void Observe_ShouldStopForwardingAfterDispose()
    {
        var eventAggregator = new EventAggregator();
        var receivedPayloads = new List<string>();

        var subscription = eventAggregator
            .Observe<StatusMessageEvent, string>()
            .Subscribe(receivedPayloads.Add);

        subscription.Dispose();
        eventAggregator.GetEvent<StatusMessageEvent>().Publish("ignored");

        Assert.Empty(receivedPayloads);
    }
}
