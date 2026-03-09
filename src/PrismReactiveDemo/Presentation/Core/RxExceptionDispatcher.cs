using Prism.Events;
using PrismReactiveDemo.Core.Events;
using PrismReactiveDemo.Core.Interfaces;

namespace PrismReactiveDemo.Presentation.Core;

/// <summary>
/// ReactiveUI 异常流统一出口。
/// Unified dispatcher for ReactiveUI exception streams.
/// </summary>
public sealed class RxExceptionDispatcher : IRxExceptionDispatcher
{
    private readonly IEventAggregator _eventAggregator;

    public RxExceptionDispatcher(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public IDisposable Bind(IObservable<Exception> exceptionStream, string source)
    {
        return exceptionStream.Subscribe(exception => Dispatch(source, exception));
    }

    public void Dispatch(string source, Exception exception)
    {
        var message = $"[{source}] {exception.Message}";
        _eventAggregator.GetEvent<StatusMessageEvent>().Publish(message);
        _eventAggregator.GetEvent<ErrorOccurredEvent>().Publish(new ErrorPayload(source, exception.Message));
    }
}
