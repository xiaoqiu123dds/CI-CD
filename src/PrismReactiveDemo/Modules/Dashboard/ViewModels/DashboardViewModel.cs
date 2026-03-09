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
using PrismReactiveDemo.Presentation.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PrismReactiveDemo.Modules.Dashboard.ViewModels;

/// <summary>
/// Dashboard 页面示例。
/// Example Dashboard page.
/// </summary>
public sealed class DashboardViewModel : ReactiveNavigationViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IDialogService _dialogService;
    private int _visitCount;

    public DashboardViewModel(
        IEventAggregator eventAggregator,
        IDialogService dialogService,
        IRxExceptionDispatcher exceptionDispatcher
    )
    {
        _eventAggregator = eventAggregator;
        _dialogService = dialogService;
        InstanceId = Guid.NewGuid().ToString("N")[..8];

        RefreshCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await Task.Delay(900, DestroyToken);
            LastRefreshAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _eventAggregator
                .GetEvent<StatusMessageEvent>()
                .Publish($"Dashboard 已刷新，实例 {InstanceId}。");
        });

        ShowInfoDialogCommand = ReactiveCommand.CreateFromObservable(() =>
            _dialogService
                .ShowDialogAsObservable(
                    DialogNames.Info,
                    new DialogParameters
                    {
                        { "Title", "响应式对话框示例" },
                        {
                            "Message",
                            $"当前 Dashboard 实例是 {InstanceId}。此对话框通过 ShowDialogAsObservable 接入 Rx 管道。"
                        },
                    }
                )
                .Do(result =>
                    _eventAggregator
                        .GetEvent<StatusMessageEvent>()
                        .Publish($"Info 对话框关闭，结果：{result.Result}")
                )
                .Select(_ => Unit.Default)
        );
        
        // 用纯 Rx CombineLatest 聚合忙碌态，消灭有状态的 BusyMonitor 类。
        // Aggregate busy state with pure Rx CombineLatest, eliminating the stateful BusyMonitor class.
        Observable
            .CombineLatest(
                RefreshCommand.IsExecuting,
                ShowInfoDialogCommand.IsExecuting,
                (a, b) => a || b
            )
            .ToPropertyEx(this, x => x.IsBusy)
            .DisposeWith(LifetimeDisposables);

        eventAggregator
            .Observe<EditorSavedEvent, EditorSavedPayload>()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(payload =>
                LastEditorMessage =
                    $"文档 {payload.Title} 于 {payload.SavedAt:HH:mm:ss} 保存。来源实例：{payload.InstanceId}"
            )
            .DisposeWith(LifetimeDisposables);

        exceptionDispatcher
            .Bind(RefreshCommand.ThrownExceptions, nameof(DashboardViewModel))
            .DisposeWith(LifetimeDisposables);

        exceptionDispatcher
            .Bind(ShowInfoDialogCommand.ThrownExceptions, nameof(DashboardViewModel))
            .DisposeWith(LifetimeDisposables);

        WelcomeMessage =
            "这是 KeepAlive=true 的 Dashboard 页面。反复返回本页时，实例 Id 不会变化。";
        LastEditorMessage = "尚未收到 Editor 的保存事件。";
        LastRefreshAt = "尚未刷新。";
    }

    public string InstanceId { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowInfoDialogCommand { get; }

    // 聚合后的忙碌态，由 Fody 织入 PropertyChanged，无需手动 OAPH 三件套。
    // Aggregated busy state; Fody injects PropertyChanged, no manual OAPH boilerplate needed.
    [ObservableAsProperty]
    public bool IsBusy { get; }

    [Reactive]
    public string WelcomeMessage { get; set; } = string.Empty;

    [Reactive]
    public string LastEditorMessage { get; set; } = string.Empty;

    [Reactive]
    public string LastRefreshAt { get; set; } = string.Empty;

    [Reactive]
    public int VisitCount { get; set; }

    protected override void OnNavigatedToCore(NavigationContext navigationContext)
    {
        _visitCount++;
        VisitCount = _visitCount;
        _eventAggregator
            .GetEvent<StatusMessageEvent>()
            .Publish($"进入 Dashboard，实例 {InstanceId}，访问次数 {_visitCount}。");
    }
}
