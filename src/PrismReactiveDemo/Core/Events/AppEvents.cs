using Prism.Events;

namespace PrismReactiveDemo.Core.Events;

/// <summary>
/// 用于状态栏展示的文本事件。
/// Text event used by the status bar.
/// </summary>
public sealed class StatusMessageEvent : PubSubEvent<string>
{
}

/// <summary>
/// 编辑器保存完成事件。
/// Event raised when the editor finishes saving.
/// </summary>
public sealed class EditorSavedEvent : PubSubEvent<EditorSavedPayload>
{
}

/// <summary>
/// 应用级错误通知事件。
/// Application-level error notification event.
/// </summary>
public sealed class ErrorOccurredEvent : PubSubEvent<ErrorPayload>
{
}

/// <summary>
/// 编辑器保存载荷。
/// Payload published after an editor save.
/// </summary>
public sealed record EditorSavedPayload(Guid DocumentId, string Title, string InstanceId, DateTimeOffset SavedAt);

/// <summary>
/// 错误通知载荷。
/// Payload used for error notifications.
/// </summary>
public sealed record ErrorPayload(string Source, string Message);
