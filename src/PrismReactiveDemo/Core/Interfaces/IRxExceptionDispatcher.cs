using System;

namespace PrismReactiveDemo.Core.Interfaces;

/// <summary>
/// 用于收口 ReactiveUI 异常流的分发器。
/// Dispatcher that centralizes ReactiveUI exception streams.
/// </summary>
public interface IRxExceptionDispatcher
{
    IDisposable Bind(IObservable<Exception> exceptionStream, string source);

    void Dispatch(string source, Exception exception);
}
