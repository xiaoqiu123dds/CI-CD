using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Prism;
using Prism.Dialogs;
using Prism.Events;
using Prism.Navigation.Regions;
using PrismReactiveDemo.Core.Constants;
using PrismReactiveDemo.Core.Events;
using PrismReactiveDemo.Core.Interfaces;
using PrismReactiveDemo.Core.Navigation;
using PrismReactiveDemo.Presentation.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PrismReactiveDemo.Modules.Editor.ViewModels;

/// <summary>
/// Editor 页面示例。
/// Example Editor page.
/// </summary>
public sealed class EditorViewModel
    : ReactiveNavigationViewModelBase,
        IConfirmNavigationRequest,
        IActiveAware
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IDialogService _dialogService;
    private readonly SerialDisposable _activeWork = new();
    private Guid _documentId;

    public EditorViewModel(
        IEventAggregator eventAggregator,
        IDialogService dialogService,
        IRxExceptionDispatcher exceptionDispatcher
    )
    {
        _eventAggregator = eventAggregator;
        _dialogService = dialogService;
        InstanceId = Guid.NewGuid().ToString("N")[..8];

        PickFileInteraction = new Interaction<Unit, string?>();

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await Task.Delay(1200, DestroyToken);

            if (EditableText.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "示例异常：文本包含 error，故意触发 ReactiveCommand 异常流。"
                );
            }

            IsDirty = false;
            _eventAggregator
                .GetEvent<EditorSavedEvent>()
                .Publish(
                    new EditorSavedPayload(
                        _documentId,
                        DocumentTitle,
                        InstanceId,
                        DateTimeOffset.Now
                    )
                );
            _eventAggregator
                .GetEvent<StatusMessageEvent>()
                .Publish($"Editor 已保存：{DocumentTitle}，实例 {InstanceId}。");
        });

        PickFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var selectedPath = await PickFileInteraction.Handle(Unit.Default);
            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                FilePath = selectedPath;
                IsDirty = true;
            }
        });

        // 用纯 Rx CombineLatest 聚合忙碌态，消灭有状态的 BusyMonitor 类。
        // Aggregate busy state with pure Rx CombineLatest, eliminating the stateful BusyMonitor class.
        Observable
            .CombineLatest(SaveCommand.IsExecuting, PickFileCommand.IsExecuting, (a, b) => a || b)
            .ToPropertyEx(this, x => x.IsBusy)
            .DisposeWith(LifetimeDisposables);

        this.WhenAnyValue(x => x.IsActive)
            .Skip(1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(isActive =>
            {
                ActivityState = isActive
                    ? "IActiveAware：当前页面是 Prism 业务激活页，后台心跳开始运行。"
                    : "IActiveAware：当前页面不是业务激活页，后台心跳已暂停。";

                IsActiveChanged?.Invoke(this, EventArgs.Empty);
                _activeWork.Disposable = isActive
                    ? Observable
                        .Interval(TimeSpan.FromSeconds(1))
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => HeartbeatText = DateTimeOffset.Now.ToString("HH:mm:ss"))
                    : Disposable.Empty;
            })
            .DisposeWith(LifetimeDisposables);

        exceptionDispatcher
            .Bind(SaveCommand.ThrownExceptions, nameof(EditorViewModel))
            .DisposeWith(LifetimeDisposables);

        exceptionDispatcher
            .Bind(PickFileCommand.ThrownExceptions, nameof(EditorViewModel))
            .DisposeWith(LifetimeDisposables);

        DocumentTitle = "尚未接收导航参数";
        FilePath = "未选择文件";
        EditableText = "请修改这段文本，然后尝试导航回 Dashboard。";
        ActivityState = "IActiveAware：等待 Prism 激活通知。";
        HeartbeatText = "--:--:--";
    }

    public event EventHandler? IsActiveChanged;
    public string InstanceId { get; }
    public override bool KeepAlive => false;
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> PickFileCommand { get; }
    public Interaction<Unit, string?> PickFileInteraction { get; }

    // 聚合后的忙碌态，由 Fody 织入 PropertyChanged，无需手动 OAPH 三件套。
    // Aggregated busy state; Fody injects PropertyChanged, no manual OAPH boilerplate needed.
    [ObservableAsProperty]
    public bool IsBusy { get; }

    [Reactive]
    public string DocumentTitle { get; set; } = string.Empty;

    [Reactive]
    public string FilePath { get; set; } = string.Empty;

    [Reactive]
    public string EditableText { get; set; } = string.Empty;

    [Reactive]
    public bool IsDirty { get; set; }

    [Reactive]
    public bool IsActive { get; set; }

    [Reactive]
    public string ActivityState { get; set; } = string.Empty;

    [Reactive]
    public string HeartbeatText { get; set; } = string.Empty;

    protected override void OnActivated(CompositeDisposable disposables)
    {
        this.WhenAnyValue(x => x.EditableText)
            .Skip(1)
            .Subscribe(_ => IsDirty = true)
            .DisposeWith(disposables);
    }

    protected override void OnNavigatedToCore(NavigationContext navigationContext)
    {
        _documentId = navigationContext.GetRequired<Guid>("DocumentId");
        DocumentTitle =
            navigationContext.GetValueOrDefault<string>("DocumentTitle") ?? $"文档 {_documentId:N}";
        _eventAggregator
            .GetEvent<StatusMessageEvent>()
            .Publish($"进入 Editor，实例 {InstanceId}，文档 {_documentId}.");
    }

    public override bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void ConfirmNavigationRequest(
        NavigationContext navigationContext,
        Action<bool> continuationCallback
    )
    {
        if (!IsDirty)
        {
            continuationCallback(true);
            return;
        }

        _dialogService.ShowDialog(
            DialogNames.Confirm,
            new DialogParameters
            {
                { "Title", "未保存内容" },
                { "Message", "当前编辑内容尚未保存，确认离开此页面吗？" },
            },
            result => continuationCallback(result.Result == ButtonResult.OK)
        );
    }

    protected override void OnDestroyedCore()
    {
        _activeWork.Dispose();
        _eventAggregator
            .GetEvent<StatusMessageEvent>()
            .Publish($"Editor 实例 {InstanceId} 已销毁。");
    }
}
