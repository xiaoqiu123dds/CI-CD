# 示例覆盖说明 / Example Coverage Guide

## 目标 / Goal

- 本文档用于将 `docs/prism-reactiveui-deep-integration.md` 中的关键条目映射到示例项目的具体文件与操作路径。 This document maps the key items in `docs/prism-reactiveui-deep-integration.md` to concrete files and demo flows in the sample project.

## 覆盖映射 / Coverage Map

- 容器边界与禁用 `Locator.Current`：示例项目所有业务依赖均由 `PrismApplication` 注册和解析，入口见 `src/PrismReactiveDemo/App.xaml.cs`。 Container boundaries and avoiding `Locator.Current`: all business dependencies are registered and resolved through `PrismApplication`, with the entry at `src/PrismReactiveDemo/App.xaml.cs`.
- `PrismReactiveView<TViewModel>` / `PrismReactiveWindow<TViewModel>`：见 `src/PrismReactiveDemo/Presentation/Core/PrismReactiveView.cs` 与 `src/PrismReactiveDemo/Presentation/Core/PrismReactiveWindow.cs`。 `PrismReactiveView<TViewModel>` / `PrismReactiveWindow<TViewModel>`: see `src/PrismReactiveDemo/Presentation/Core/PrismReactiveView.cs` and `src/PrismReactiveDemo/Presentation/Core/PrismReactiveWindow.cs`.
- `WhenActivated` 与强绑定：`DashboardView.xaml.cs`、`EditorView.xaml.cs`、`ShellWindow.xaml.cs`。 `WhenActivated` and strong bindings: `DashboardView.xaml.cs`, `EditorView.xaml.cs`, and `ShellWindow.xaml.cs`.
- 设计时数据与 XAML 普通 Binding：`DashboardView.xaml` + `DashboardDesignViewModel.cs`，`EditorView.xaml` + `EditorDesignViewModel.cs`。 Design-time data and standard XAML bindings: `DashboardView.xaml` + `DashboardDesignViewModel.cs`, and `EditorView.xaml` + `EditorDesignViewModel.cs`.
- `DestroyToken`、实例销毁、长任务取消：`ReactiveNavigationViewModelBase.cs` 与 `EditorViewModel.cs`。 `DestroyToken`, instance destruction, and long-task cancellation: `ReactiveNavigationViewModelBase.cs` and `EditorViewModel.cs`.
- `INavigationAware` / 强类型导航参数：`EditorViewModel.cs` + `NavigationContextExtensions.cs`。 `INavigationAware` / typed navigation parameters: `EditorViewModel.cs` + `NavigationContextExtensions.cs`.
- `IRegionMemberLifetime`：`DashboardViewModel.cs` 的 `KeepAlive=true` 对比 `EditorViewModel.cs` 的 `KeepAlive=false`。 `IRegionMemberLifetime`: compare `KeepAlive=true` in `DashboardViewModel.cs` with `KeepAlive=false` in `EditorViewModel.cs`.
- `IConfirmNavigationRequest`：在 `EditorViewModel.cs` 中修改文本后切回 Dashboard，会弹出确认对话框。 `IConfirmNavigationRequest`: after editing text in `EditorViewModel.cs`, switching back to Dashboard triggers a confirmation dialog.
- `Interaction<TInput, TOutput>`：`EditorViewModel.cs` 与 `EditorView.xaml.cs` 的文件选择示例，视图侧通过 `WhenAnyValue(ViewModel)` 动态注册处理器，避免导航激活早于 `ViewModelLocator` 注入时静默失效。 `Interaction<TInput, TOutput>`: the file-picking example in `EditorViewModel.cs` and `EditorView.xaml.cs`, where the view dynamically registers handlers through `WhenAnyValue(ViewModel)` to avoid silent failure when navigation activation happens before `ViewModelLocator` injection.
- `IDialogService`：`EditorViewModel.cs` 的确认离开，以及 `DashboardViewModel.cs` 的信息弹窗。 `IDialogService`: confirm-leave flow in `EditorViewModel.cs` and the info dialog in `DashboardViewModel.cs`.
- `ShowDialogAsObservable`：`DashboardViewModel.cs`。 `ShowDialogAsObservable`: `DashboardViewModel.cs`.
- `IEventAggregator` → Rx 桥接：`EventAggregatorExtensions.cs`、`ShellViewModel.cs`、`DashboardViewModel.cs`。 `IEventAggregator` → Rx bridge: `EventAggregatorExtensions.cs`, `ShellViewModel.cs`, and `DashboardViewModel.cs`.
- UI 线程显式切换：`ShellViewModel.cs` 与 `EditorViewModel.cs` 中的 `.ObserveOn(RxApp.MainThreadScheduler)`，以及 `DashboardViewModel.cs` / `EditorViewModel.cs` 中的 `ToPropertyEx(..., scheduler: RxApp.MainThreadScheduler)`。 Explicit UI-thread switching: `.ObserveOn(RxApp.MainThreadScheduler)` in `ShellViewModel.cs` and `EditorViewModel.cs`, plus `ToPropertyEx(..., scheduler: RxApp.MainThreadScheduler)` in `DashboardViewModel.cs` and `EditorViewModel.cs`.
- `ReactiveCommand.ThrownExceptions` 与异常收口：`RxExceptionDispatcher.cs`、`DashboardViewModel.cs`、`EditorViewModel.cs`。 `ReactiveCommand.ThrownExceptions` and exception aggregation: `RxExceptionDispatcher.cs`, `DashboardViewModel.cs`, and `EditorViewModel.cs`.
- `RxApp.DefaultExceptionHandler` 与全局异常：`App.xaml.cs`、`GlobalExceptionHandler.cs`。 `RxApp.DefaultExceptionHandler` and global exceptions: `App.xaml.cs` and `GlobalExceptionHandler.cs`.
- 命令忙碌态聚合：`DashboardViewModel.cs` 与 `EditorViewModel.cs` 使用 `ReactiveCommand.IsExecuting` + `Observable.CombineLatest` + `ToPropertyEx` 生成单一 `IsBusy`。 Command busy-state aggregation: `DashboardViewModel.cs` and `EditorViewModel.cs` use `ReactiveCommand.IsExecuting` + `Observable.CombineLatest` + `ToPropertyEx` to produce a single `IsBusy`.
- `IActiveAware` 与 `IActivatableViewModel` 区分：`EditorViewModel.cs` 中的 `IsActive`/`HeartbeatText` 与 `OnActivated(...)`。 `IActiveAware` vs `IActivatableViewModel`: `IsActive`/`HeartbeatText` and `OnActivated(...)` in `EditorViewModel.cs`.

## 建议演示步骤 / Suggested Demo Steps

- 启动应用后先观察 `Dashboard` 的实例 Id 和访问次数。 After startup, first observe the `Dashboard` instance ID and visit count.
- 点击“打开 Editor”，记录 `Editor` 的实例 Id。 Click `Open Editor` and note the `Editor` instance ID.
- 修改文本后直接点击“打开 Dashboard”，触发 `IConfirmNavigationRequest`。 Modify the text and click `Open Dashboard` to trigger `IConfirmNavigationRequest`.
- 返回 `Dashboard` 后观察状态栏和最近一次保存/销毁消息。 After returning to `Dashboard`, observe the status bar and the latest save/destroy messages.
- 在 `Editor` 文本中输入 `error` 再保存，观察 `ThrownExceptions` 与应用级异常出口。 Enter `error` in the `Editor` text and save again to observe `ThrownExceptions` and the application-level exception exit.
- 点击 `Dashboard` 的“打开响应式对话框”，观察 `ShowDialogAsObservable` 的使用。 Click `Open Reactive Dialog` in `Dashboard` to observe `ShowDialogAsObservable` in action.
