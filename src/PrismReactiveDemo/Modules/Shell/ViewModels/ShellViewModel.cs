using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Prism.Events;
using Prism.Navigation;
using Prism.Navigation.Regions;
using PrismReactiveDemo.Core.Constants;
using PrismReactiveDemo.Core.Events;
using PrismReactiveDemo.Presentation.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PrismReactiveDemo.Modules.Shell.ViewModels;

/// <summary>
/// Shell 负责展示 Region 与跨模块状态。
/// Shell displays the Region and cross-module state.
/// </summary>
public sealed class ShellViewModel : ReactiveObject, IDisposable
{
    private readonly IRegionManager _regionManager;
    private readonly CompositeDisposable _disposables = new();
    private int _editorSequence = 1;

    public ShellViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
    {
        _regionManager = regionManager;

        NavigateDashboardCommand = ReactiveCommand.Create(() =>
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, NavigationTargets.Dashboard);
        });

        NavigateEditorCommand = ReactiveCommand.Create(() =>
        {
            var parameters = new NavigationParameters
            {
                { "DocumentId", Guid.NewGuid() },
                { "DocumentTitle", $"示例文档 {_editorSequence++}" },
            };

            _regionManager.RequestNavigate(
                RegionNames.MainRegion,
                NavigationTargets.Editor,
                parameters
            );
        });

        eventAggregator
            .Observe<StatusMessageEvent, string>()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(message => StatusText = message)
            .DisposeWith(_disposables);

        eventAggregator
            .Observe<ErrorOccurredEvent, ErrorPayload>()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(payload => LatestError = $"{payload.Source}: {payload.Message}")
            .DisposeWith(_disposables);

        StatusText = "应用已启动，当前默认导航到 Dashboard。";
        LatestError = "暂无错误。";
    }

    public ReactiveCommand<Unit, Unit> NavigateDashboardCommand { get; }

    public ReactiveCommand<Unit, Unit> NavigateEditorCommand { get; }

    [Reactive]
    public string StatusText { get; set; } = string.Empty;

    [Reactive]
    public string LatestError { get; set; } = string.Empty;

    // 应用退出时释放全部 EventAggregator 订阅，防止内存泄漏。
    // Dispose all EventAggregator subscriptions on application exit to prevent memory leaks.
    public void Dispose() => _disposables.Dispose();
}
