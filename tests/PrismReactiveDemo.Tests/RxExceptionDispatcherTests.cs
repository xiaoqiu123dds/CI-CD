using Prism.Events;
using PrismReactiveDemo.Core.Events;
using PrismReactiveDemo.Presentation.Core;

namespace PrismReactiveDemo.Tests;

public sealed class RxExceptionDispatcherTests
{
    [Fact]
    public void Dispatch_ShouldPublishStatusAndErrorEvents()
    {
        var eventAggregator = new EventAggregator();
        var dispatcher = new RxExceptionDispatcher(eventAggregator);
        var receivedStatuses = new List<string>();
        var receivedErrors = new List<ErrorPayload>();

        using var statusSubscription = eventAggregator
            .Observe<StatusMessageEvent, string>()
            .Subscribe(receivedStatuses.Add);
        using var errorSubscription = eventAggregator
            .Observe<ErrorOccurredEvent, ErrorPayload>()
            .Subscribe(receivedErrors.Add);

        dispatcher.Dispatch("EditorViewModel", new InvalidOperationException("save failed"));

        var status = Assert.Single(receivedStatuses);
        var error = Assert.Single(receivedErrors);

        Assert.Equal("[EditorViewModel] save failed", status);
        Assert.Equal("EditorViewModel", error.Source);
        Assert.Equal("save failed", error.Message);
    }
}
