using System.Reactive.Disposables;
using System.Reactive.Linq;
using Prism.Events;

namespace PrismReactiveDemo.Presentation.Core;

/// <summary>
/// Prism 事件到 Rx 流的桥接扩展。
/// Bridge extensions from Prism events to Rx streams.
/// </summary>
public static class EventAggregatorExtensions
{
    public static IObservable<TPayload> Observe<TEvent, TPayload>(this IEventAggregator eventAggregator)
        where TEvent : PubSubEvent<TPayload>, new()
    {
        return Observable.Create<TPayload>(observer =>
        {
            void Handler(TPayload payload)
            {
                observer.OnNext(payload);
            }

            var prismEvent = eventAggregator.GetEvent<TEvent>();
            prismEvent.Subscribe(Handler);

            return Disposable.Create(() => prismEvent.Unsubscribe(Handler));
        });
    }
}
