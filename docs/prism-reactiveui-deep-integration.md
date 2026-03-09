# Prism + ReactiveUI 深度集成说明 / Prism + ReactiveUI Deep Integration Guide

## 1. 目标与前提 / Goals and Premises

- 本文档针对 `B` 方案：**单项目、模块化目录、Prism + ReactiveUI、暂不接入数据持久化与额外扩展功能**。 This document targets option `B`: **single project, modular folder structure, Prism + ReactiveUI, with no persistence or extra extensions for now**.
- 核心目标不是“让两个框架都出现”，而是“让两个框架各自负责自己最擅长的层面，并且生命周期互相咬合”。 The core goal is not merely “making both frameworks appear”, but “assigning each framework to the layer it is best at and making their lifecycles interlock cleanly”.
- 本方案以 `PrismApplication` 为唯一应用组合根。 This solution uses `PrismApplication` as the single application composition root.

## 2. 对技能初版的诊断 / Diagnosis of the Current Skill Draft

### 2.1 初版的优点 / Strengths of the Draft

- 已经识别到 `ReactiveObject + INavigationAware + IDestructible` 是融合入口。 It already identifies `ReactiveObject + INavigationAware + IDestructible` as an entry point for integration.
- 已经尝试通过 `PrismReactiveUserControl<TViewModel>` 和 `PrismReactiveWindow<TViewModel>` 解决 `DataContext` 与 `ViewModel` 同步。 It already attempts to solve `DataContext` and `ViewModel` synchronization through `PrismReactiveUserControl<TViewModel>` and `PrismReactiveWindow<TViewModel>`.
- 已经明确提出“Prism 负责导航，ReactiveUI 负责命令和绑定”的方向。 It already points toward the idea that “Prism handles navigation while ReactiveUI handles commands and bindings”.

### 2.2 初版的问题 / Problems in the Draft

- `ReactiveViewModelBase` 只有“实例销毁”概念，没有“激活期”概念。 `ReactiveViewModelBase` has only an “instance destruction” concept and lacks an “activation period” concept.
- `PrismReactiveUserControl` 与 `PrismReactiveWindow` 在 `Unloaded/Closed` 时直接释放字段级 `CompositeDisposable`，这会把“视图暂时离开可视树”与“对象彻底死亡”混成一件事。 `PrismReactiveUserControl` and `PrismReactiveWindow` directly dispose a field-level `CompositeDisposable` on `Unloaded/Closed`, which incorrectly treats “temporarily leaving the visual tree” as “permanent object death”.
- `SetupBindings()` 只是一个扩展点，但没有通过 `WhenActivated` 真正接入 ReactiveUI 的激活模型。 `SetupBindings()` exists only as an extension point, but it is not truly wired into ReactiveUI’s activation model through `WhenActivated`.
- 文档层面对“区域导航”“对话服务”“事件总线”“交互”没有做权威划分，容易让一个项目同时出现多个导航和消息模型。 The documentation does not make authoritative decisions for “region navigation”, “dialog service”, “event bus”, and “interactions”, which makes it easy for a project to end up with multiple navigation and messaging models at once.

### 2.3 这为什么会显得像拼凑 / Why It Feels Like Stitching Instead of Integration

- 如果你只是让类型同时继承两个框架的接口，这只是“表面兼容”。 If you merely make a type implement interfaces from both frameworks, that is only “surface compatibility”.
- 真正的深度集成，必须回答三个问题：**谁管理对象实例、谁管理可视激活、谁管理应用级组合**。 Real deep integration must answer three questions: **who manages object instances, who manages visual activation, and who manages application composition**.
- 在这三个问题上，`Prism` 与 `ReactiveUI` 并不是对等竞争关系，而是“分工互补”关系。 On these three questions, `Prism` and `ReactiveUI` are not equal competitors; they are complementary by design.

## 3. 正确的分工边界 / Correct Responsibility Boundaries

### 3.1 一句话结论 / One-Sentence Conclusion

- `Prism` 是 **组合层权威**，`ReactiveUI` 是 **状态层权威**。 `Prism` is the **authority for composition**, while `ReactiveUI` is the **authority for state**.

### 3.2 Prism 负责什么 / What Prism Owns

- 模块加载与模块边界。 Module loading and module boundaries.
- `Region` 注册、视图发现、视图注入、区域导航。 `Region` registration, view discovery, view injection, and region navigation.
- `INavigationAware`、`IConfirmNavigationRequest`、`IRegionMemberLifetime` 这一组导航生命周期契约。 The navigation lifecycle contract group around `INavigationAware`, `IConfirmNavigationRequest`, and `IRegionMemberLifetime`.
- `IDialogService` 及对话框宿主。 `IDialogService` and the dialog host model.
- `IEventAggregator` 作为跨模块发布/订阅总线。 `IEventAggregator` as the cross-module pub/sub bus.

### 3.3 ReactiveUI 负责什么 / What ReactiveUI Owns

- `ReactiveObject`、`[Reactive]`、`ObservableAsPropertyHelper<T>` 组成的状态传播模型。 The state propagation model built around `ReactiveObject`, `[Reactive]`, and `ObservableAsPropertyHelper<T>`.
- `ReactiveCommand`、`ThrownExceptions`、`IsExecuting` 组成的命令模型。 The command model built around `ReactiveCommand`, `ThrownExceptions`, and `IsExecuting`.
- `WhenActivated`、`IActivatableViewModel`、`ViewModelActivator` 组成的激活模型。 The activation model built around `WhenActivated`, `IActivatableViewModel`, and `ViewModelActivator`.
- `Bind`、`OneWayBind`、`BindCommand` 组成的强类型绑定模型。 The strong-typed binding model built around `Bind`, `OneWayBind`, and `BindCommand`.
- `Interaction<TInput, TOutput>` 组成的视图交互抽象。 The view interaction abstraction based on `Interaction<TInput, TOutput>`.

### 3.4 不应该混用的地方 / Places That Should Not Be Mixed

- 不要让 `ReactiveUI RoutingState` 与 `Prism Region Navigation` 双轨并存。 Do not run `ReactiveUI RoutingState` in parallel with `Prism Region Navigation`.
- 不要让 `MessageBus` 充当整个应用的跨模块总线。 Do not let `MessageBus` become the global cross-module bus for the whole application.
- 不要让页面 ViewModel 与对话框 ViewModel 共用同一个“大而全”基类。 Do not force page ViewModels and dialog ViewModels into the same all-in-one base class.

### 3.5 容器边界与服务定位约束 / Container Boundary and Service Location Rules

- 业务服务、ViewModel、View 的实例化统一交给 `Prism` 容器。 The instantiation of business services, ViewModels, and Views must be unified under the `Prism` container.
- 业务代码中不使用 `Locator.Current` 解析应用服务。 Business code must not use `Locator.Current` to resolve application services.
- `Splat` 仅保留给 ReactiveUI 框架内部机制，例如 `IViewFor<T>`、调度器或框架级定位扩展。 `Splat` should be reserved only for ReactiveUI internal mechanics, such as `IViewFor<T>`, schedulers, or framework-level locator extensions.
- 如果某项能力既可以通过 `Prism` 容器获得，也可以通过 `Splat` 获得，则业务层一律以 `Prism` 为准。 If a capability can be obtained from both the `Prism` container and `Splat`, the business layer must always prefer `Prism`.

## 4. 生命周期模型：这是深度融合的核心 / Lifecycle Model: This Is the Core of Deep Integration

### 4.1 建议你把生命周期拆成四层 / Split Lifetime into Four Layers

1. **应用生命周期 / Application lifetime**  
   - 从程序启动到程序退出。 From application startup to application shutdown.
   - 由 `PrismApplication` 统一承载。 Owned solely by `PrismApplication`.

2. **模块与单例生命周期 / Module and singleton lifetime**  
   - 例如 `ShellViewModel`、全局状态服务、跨模块协调器。 Examples include `ShellViewModel`, global state services, and cross-module coordinators.
   - 一般由 Prism 容器中的单例注册承载。 Usually backed by singleton registrations in the Prism container.

3. **导航实例生命周期 / Navigation instance lifetime**  
   - 一个页面 ViewModel 被导航到区域后，其实例从“创建”到“移除”的时间段。 The timespan from when a page ViewModel is created through when it is removed from a region.
   - 由 `INavigationAware`、`IRegionMemberLifetime`、`IDestructible` 管理。 Managed by `INavigationAware`, `IRegionMemberLifetime`, and `IDestructible`.

4. **激活生命周期 / Activation lifetime**  
   - 页面进入可视树、显示在屏幕上、再离开可视树的过程。 The process of a page entering the visual tree, being shown, and leaving the visual tree.
   - 由 ReactiveUI 的 `WhenActivated` 与 `ViewModelActivator` 管理。 Managed by ReactiveUI’s `WhenActivated` and `ViewModelActivator`.

### 4.2 你的 skill 初版为什么会卡在这里 / Why the Current Draft Gets Stuck Here

- 它把“对象实例释放”和“页面可视激活结束”混在一起。 It mixes “object instance disposal” with “end of visual activation”.
- 这会导致两个常见问题： This causes two common problems:
  - 页面只是暂时切走，但订阅被永久销毁。 The page is only temporarily hidden, but subscriptions are destroyed permanently.
  - 页面实例被缓存重用时，绑定和订阅无法安全重建。 When a cached page instance is reused, bindings and subscriptions cannot be rebuilt safely.

### 4.3 正确的释放职责 / Correct Disposal Responsibilities

- **View 的绑定与可视事件** 放在 `WhenActivated` 中，让它跟随视图显隐自动释放。 **View bindings and visual event subscriptions** belong in `WhenActivated`, so they are automatically disposed when the view deactivates.
- **ViewModel 的热流订阅** 如果只在当前页面可见时有效，也放在 `WhenActivated` 中。 **ViewModel hot-observable subscriptions** also belong in `WhenActivated` if they are only meaningful while the page is visible.
- **ViewModel 的实例级资源** 例如长生命周期服务订阅、`Subject`、`CancellationTokenSource`、桥接注册，应在 `Destroy()` 中兜底释放。 **Instance-level ViewModel resources** such as long-lived service subscriptions, `Subject`s, `CancellationTokenSource`s, and bridge registrations should be released in `Destroy()` as the final safety net.

### 4.4 推荐的判断口诀 / Recommended Rule of Thumb

- “**跟显示状态走**”的资源，归 `WhenActivated`。 Resources that “**follow visibility**” belong to `WhenActivated`.
- “**跟对象实例走**”的资源，归 `Destroy()`。 Resources that “**follow instance lifetime**” belong to `Destroy()`.
- “**跟进程走**”的资源，归容器单例与应用退出流程。 Resources that “**follow the process lifetime**” belong to container singletons and application shutdown.

## 5. View 与 ViewModel 的结合方式 / How Views and ViewModels Should Be Combined

### 5.1 View 基类应承担什么 / What the View Base Class Should Do

- 同步 `DataContext` 到强类型 `ViewModel`。 Synchronize `DataContext` into the strongly typed `ViewModel`.
- 在构造函数中一次性接入 `WhenActivated`。 Wire `WhenActivated` exactly once in the constructor.
- 让子类通过 `SetupBindings(CompositeDisposable disposables)` 注册绑定，而不是手工维护字段级 `CompositeDisposable`。 Let subclasses register bindings through `SetupBindings(CompositeDisposable disposables)` instead of manually maintaining a field-level `CompositeDisposable`.
- 在 WPF 场景下继续尊重 Prism `ViewModelLocator`，因为它对 `Region Navigation` 与 `IDialogService` 的 DataContext 装配本来就是默认路径。 In WPF, continue to respect Prism `ViewModelLocator`, because it is already the default DataContext wiring path for `Region Navigation` and `IDialogService`.

### 5.2 为什么不能在 Unloaded 时把字段级 Disposables Dispose 掉 / Why Not Dispose a Field-Level Disposable on Unloaded

- 对于区域内可复用视图，`Unloaded` 不一定代表实例死亡。 For reusable region views, `Unloaded` does not necessarily mean instance death.
- `ReactiveUI` 已经提供了 `WhenActivated` 来解决“进入/离开可视树”的释放问题，不应重复造一个更粗糙的版本。 `ReactiveUI` already provides `WhenActivated` to solve enter/leave visual tree disposal, so a rough duplicate should not be reinvented.
- 根据 ReactiveUI 官方 `WhenActivated` 指南，XAML 视图中的绑定与依赖属性观察应优先放进 `WhenActivated + DisposeWith`，这能显著降低泄漏风险。 According to the official ReactiveUI `WhenActivated` guidance, bindings and dependency-property observations in XAML views should prefer `WhenActivated + DisposeWith`, which significantly reduces leak risk.

### 5.3 推荐的 View 基类形态 / Recommended Shape of a View Base Class

```csharp
using System.Reactive.Disposables;
using ReactiveUI;

namespace Demo.Presentation.Core;

/// <summary>
/// Prism 与 ReactiveUI 共用的页面基类。
/// Shared page base class for Prism and ReactiveUI.
/// </summary>
public abstract class PrismReactiveView<TViewModel> : ReactiveUserControl<TViewModel>
    where TViewModel : class
{
    protected PrismReactiveView()
    {
        this.DataContextChanged += (_, args) =>
        {
            if (args.NewValue is TViewModel viewModel)
            {
                ViewModel = viewModel;
                return;
            }

            ViewModel = null;
        };

        this.WhenActivated(disposables =>
        {
            SetupBindings(disposables);
            SetupInteractions(disposables);
        });
    }

    /// <summary>
    /// 注册绑定与视觉层订阅。
    /// Registers bindings and visual-layer subscriptions.
    /// </summary>
    protected virtual void SetupBindings(CompositeDisposable disposables)
    {
    }

    /// <summary>
    /// 注册 Interaction 处理器。
    /// Registers interaction handlers.
    /// </summary>
    protected virtual void SetupInteractions(CompositeDisposable disposables)
    {
    }
}
```

#### 5.3.1 View 层绑定策略与设计时数据 / View Binding Strategy and Design-Time Data

- 复杂双向绑定、命令绑定、交互绑定优先使用 `this.Bind`、`this.BindCommand`、`WhenActivated`。 Prefer `this.Bind`, `this.BindCommand`, and `WhenActivated` for complex two-way bindings, command bindings, and interaction bindings.
- 纯展示型属性、设计器预览占位、`d:DataContext` 场景，允许继续使用普通 XAML `Binding`。 Standard XAML `Binding` remains allowed for display-only properties, designer-preview placeholders, and `d:DataContext` scenarios.
- 规范的重点不是“彻底禁用 XAML Binding”，而是“把复杂交互与释放敏感绑定收口到 ReactiveUI 强绑定模型”。 The point of the rule is not to ban XAML binding entirely, but to move complex interactions and disposal-sensitive bindings into the ReactiveUI strong-binding model.

### 5.4 ViewModel 基类应承担什么 / What the ViewModel Base Class Should Do

- 成为 Prism 导航契约与 ReactiveUI 激活契约的交汇点。 Become the convergence point for Prism navigation contracts and ReactiveUI activation contracts.
- 区分“激活期释放桶”和“实例期释放桶”。 Distinguish the “activation-scope disposal bucket” from the “instance-scope disposal bucket”.
- 为派生类提供统一的扩展点，例如 `OnNavigatedToCore`、`OnNavigatedFromCore`、`OnDestroyedCore`。 Provide consistent extension points for derived classes such as `OnNavigatedToCore`, `OnNavigatedFromCore`, and `OnDestroyedCore`.

### 5.5 推荐的页面 ViewModel 基类 / Recommended Base for Page ViewModels

```csharp
using System.Threading;
using System.Reactive.Disposables;
using Prism.Navigation;
using Prism.Regions;
using ReactiveUI;

namespace Demo.Presentation.Core;

/// <summary>
/// 页面级 ViewModel 基类。
/// Page-level ViewModel base class.
/// </summary>
public abstract class ReactiveNavigationViewModelBase : ReactiveObject, INavigationAware, IDestructible, IActivatableViewModel
{
    private bool _isDestroyed;
    private readonly CancellationTokenSource _destroyCts = new();

    protected CompositeDisposable LifetimeDisposables { get; } = new();

    protected CancellationToken DestroyToken => _destroyCts.Token;

    public ViewModelActivator Activator { get; } = new();

    protected ReactiveNavigationViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            OnActivated(disposables);
        });
    }

    protected virtual void OnActivated(CompositeDisposable disposables)
    {
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        OnNavigatedToCore(navigationContext);
    }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        OnNavigatedFromCore(navigationContext);
    }

    protected virtual void OnNavigatedToCore(NavigationContext navigationContext)
    {
    }

    protected virtual void OnNavigatedFromCore(NavigationContext navigationContext)
    {
    }

    public void Destroy()
    {
        if (_isDestroyed)
        {
            return;
        }

        _isDestroyed = true;
        _destroyCts.Cancel();
        OnDestroyedCore();
        LifetimeDisposables.Dispose();
        _destroyCts.Dispose();
    }

    protected virtual void OnDestroyedCore()
    {
    }
}
```

### 5.6 关键理解 / Key Understanding

- `IActivatableViewModel` 不是用来替代导航，而是用来管理“视图在场时应该存在的订阅”。 `IActivatableViewModel` does not replace navigation; it manages subscriptions that should exist only while the view is on stage.
- `INavigationAware` 不是用来做 Rx 绑定，而是用来处理导航参数、实例复用判断与离开时协商。 `INavigationAware` is not for Rx bindings; it is for navigation parameters, instance reuse decisions, and leave-time coordination.
- `DestroyToken` 适合承接网络请求、轮询、后台长任务等实例级异步取消。 `DestroyToken` is suitable for canceling instance-level async work such as network requests, polling, and long-running background tasks.
- 每个 `ReactiveCommand` 的 `ThrownExceptions` 都必须有明确出口，不能依赖“默认没人订阅也没关系”的侥幸逻辑。 Every `ReactiveCommand.ThrownExceptions` stream must have an explicit exit path; it must not rely on the assumption that an unsubscribed stream is harmless.

## 6. 区域、导航、对话服务：应该如何组合 / Regions, Navigation, and Dialogs: How They Should Work Together

### 6.1 区域与导航的选择 / Choice for Regions and Navigation

- 对于 `B` 方案，主工作区、侧边面板、底部状态区、文档区，都应采用 `Prism Region`。 For option `B`, the main workspace, side panel, bottom status area, and document area should all use `Prism Region`.
- 页面切换使用 `RequestNavigate` 或你自己的导航门面，但最终仍然落到 Prism 区域导航。 Page switching should use `RequestNavigate` or your own navigation facade, but it must ultimately land on Prism region navigation.

### 6.2 为什么不推荐把 RoutingState 混进来 / Why Mixing in RoutingState Is Not Recommended

- `RoutingState` 本身就是另一套导航栈模型。 `RoutingState` is itself another navigation stack model.
- 在 Prism 应用中同时保留它，会出现“Region 状态”和“RoutingState 状态”双源真相。 In a Prism application, keeping it as well creates two sources of truth: “Region state” and “RoutingState state”.
- 这不是增强，而是重复建模。 This is not enhancement; it is duplicated modeling.

### 6.3 对话服务的组合策略 / Dialog Composition Strategy

- **模态对话框、业务弹窗、宿主窗口样式统一**：使用 `Prism IDialogService`。 **Modal dialogs, business popups, and unified host window styling**: use `Prism IDialogService`.
- **ViewModel 需要向当前 View 请求一次性的用户输入或确认**：优先使用 `ReactiveUI Interaction<TInput, TOutput>`。 **When a ViewModel needs one-time user input or confirmation from the current View**: prefer `ReactiveUI Interaction<TInput, TOutput>`.

### 6.4 一个很重要的分界 / An Important Boundary

- `IDialogService` 解决的是 **应用级对话框托管**。 `IDialogService` solves **application-level dialog hosting**.
- `Interaction<TInput, TOutput>` 解决的是 **ViewModel 与 View 之间的输入输出协商**。 `Interaction<TInput, TOutput>` solves **input/output negotiation between a ViewModel and its View**.
- 两者不是二选一，而是上下层分工。 They are not mutually exclusive; they operate at different layers.

### 6.5 页面 VM 与对话框 VM 必须分基类 / Page VMs and Dialog VMs Must Have Separate Base Classes

- Prism 官方对 `DialogService` 的说明明确指出，对话框并不使用导航生命周期接口。 Prism’s official dialog service guidance explicitly indicates that dialogs do not use navigation lifecycle interfaces.
- 所以不要让对话框 ViewModel 继承页面导航基类。 Therefore, dialog ViewModels should not inherit the page navigation base class.

### 6.6 推荐的对话框基类 / Recommended Base for Dialog ViewModels

```csharp
using System;
using System.Reactive.Disposables;
using Prism.Services.Dialogs;
using ReactiveUI;

namespace Demo.Presentation.Core;

/// <summary>
/// 对话框 ViewModel 基类。
/// Dialog ViewModel base class.
/// </summary>
public abstract class ReactiveDialogViewModelBase : ReactiveObject, IDialogAware, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    protected CompositeDisposable LifetimeDisposables { get; } = new();

    public abstract string Title { get; }

    public event Action<IDialogResult>? RequestClose;

    protected ReactiveDialogViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            OnActivated(disposables);
        });
    }

    protected virtual void OnActivated(CompositeDisposable disposables)
    {
    }

    public virtual bool CanCloseDialog() => true;

    public virtual void OnDialogOpened(IDialogParameters parameters)
    {
    }

    public virtual void OnDialogClosed()
    {
        LifetimeDisposables.Dispose();
    }

    protected void Close(IDialogResult dialogResult)
    {
        RequestClose?.Invoke(dialogResult);
    }
}
```

### 6.7 对话服务的响应式包装 / Reactive Wrapper for Dialog Service

- `IDialogService.ShowDialog` 是回调式 API，若直接在 ViewModel 中串联业务，会打断 ReactiveUI 的方法链。 `IDialogService.ShowDialog` is a callback-style API, and chaining business logic directly from it breaks the ReactiveUI pipeline.
- 建议在 `Presentation.Core` 中提供一个 `ShowDialogAsObservable` 扩展，把结果重新收敛到 Rx 管道。 It is recommended to provide a `ShowDialogAsObservable` extension in `Presentation.Core` so the result can re-enter the Rx pipeline.

```csharp
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Prism.Services.Dialogs;

namespace Demo.Presentation.Core;

/// <summary>
/// Prism 对话服务的响应式扩展。
/// Reactive extensions for Prism dialog service.
/// </summary>
public static class DialogServiceExtensions
{
    public static IObservable<IDialogResult> ShowDialogAsObservable(
        this IDialogService dialogService,
        string name,
        IDialogParameters? parameters = null)
    {
        return Observable.Create<IDialogResult>(observer =>
        {
            dialogService.ShowDialog(name, parameters, result =>
            {
                observer.OnNext(result);
                observer.OnCompleted();
            });

            return Disposable.Empty;
        });
    }
}
```

- 如果交互仅仅是本地文件选择、控件焦点切换或局部视觉反馈，也可以继续使用 `Interaction` 或独立服务抽象，而不是强行一律走 `IDialogService`。 If the interaction is only a local file picker, control-focus change, or local visual feedback, it can still go through `Interaction` or a dedicated service abstraction instead of forcing everything through `IDialogService`.

## 7. 事件总线：不是只选一个，而是分层使用 / Event Bus: Do Not Pick Just One, Use Them by Layer

### 7.1 推荐结论 / Recommended Conclusion

- **跨模块、跨区域、业务级通知**：使用 `Prism IEventAggregator`。 **Cross-module, cross-region, business-level notifications**: use `Prism IEventAggregator`.
- **视图局部、瞬时、一次性 UI 信号**：优先 `Interaction`，其次局部 Observable。 **View-local, transient, one-shot UI signals**: prefer `Interaction`, then local observables.
- **`ReactiveUI MessageBus`**：只在非常局部、明确受控的场景使用；默认不要把它升级成全局事件系统。 **`ReactiveUI MessageBus`**: use it only in very local, explicitly controlled scenarios; do not promote it into the global event system by default.

### 7.2 为什么 `IEventAggregator` 更适合跨模块 / Why `IEventAggregator` Fits Cross-Module Communication Better

- 它天然属于 Prism 模块化体系。 It naturally belongs to Prism’s modular architecture.
- 它对“谁是应用级事件”有更清晰的建模方式。 It offers clearer modeling for what counts as an application-level event.
- 团队协作时更容易追踪事件定义。 It is easier to trace event definitions in team development.

### 7.3 为什么 `MessageBus` 不适合做全局主总线 / Why `MessageBus` Is a Poor Global Primary Bus

- 它太灵活，灵活到容易失控。 It is so flexible that it is easy to lose control.
- 事件名字与通道语义不稳定时，可维护性会迅速下降。 When event naming and channel semantics become unstable, maintainability drops quickly.
- 它更适合 ReactiveUI 生态内部的小范围信号，而不是大型 WPF 模块化应用的公共语言。 It is better suited to small-scope signaling within the ReactiveUI ecosystem than to serving as the common language of a large modular WPF application.

### 7.4 最佳融合方式：把 Prism 事件桥接成 Observable / Best Integration Pattern: Bridge Prism Events into Observables

```csharp
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Prism.Events;

namespace Demo.Presentation.Core;

/// <summary>
/// 将 Prism 事件桥接为 Rx 流。
/// Bridges Prism events into Rx streams.
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
```

- 这样做的价值，是让 Prism 负责“事件边界”，让 ReactiveUI 负责“事件管道编排”。 The value of this pattern is that Prism owns the **event boundary**, while ReactiveUI owns the **event pipeline orchestration**.

### 7.5 线程与订阅释放规则 / Threading and Subscription Disposal Rules

- 事件桥接扩展本身保持**线程中立**，不要默认把所有订阅都强行切回 UI 线程。 The event-bridge extension itself should stay **thread-neutral** and must not force every subscription back onto the UI thread by default.
- 当订阅结果要更新 ViewModel 的 UI 绑定属性时，由调用方显式追加 `.ObserveOn(RxApp.MainThreadScheduler)`。 When a subscription updates UI-bound ViewModel properties, the caller should explicitly append `.ObserveOn(RxApp.MainThreadScheduler)`.
- 长生命周期对象中的所有 `Subscribe(...)` 都必须进入当前实例的 `CompositeDisposable`。 Every `Subscribe(...)` call inside a long-lived object must be stored in the current instance's `CompositeDisposable`.
- `IEventAggregator`、服务事件、`Subject`、`Observable.Timer` 这类源头最容易产生事件逃逸，必须优先审查。 Sources such as `IEventAggregator`, service events, `Subject`s, and `Observable.Timer` are the most likely to leak events and must be reviewed first.

## 8. 通用基类与复用：应该怎么搭 / Common Base Classes and Reuse: How to Build Them

### 8.1 建议的基类族谱 / Recommended Base Class Family

- `ReactiveNavigationViewModelBase`：页面 ViewModel 的共同基类。 `ReactiveNavigationViewModelBase`: the shared base for page ViewModels.
- `ReactiveDialogViewModelBase`：对话框 ViewModel 的共同基类。 `ReactiveDialogViewModelBase`: the shared base for dialog ViewModels.
- `PrismReactiveView<TViewModel>`：区域页面与普通 UserControl 的共同视图基类。 `PrismReactiveView<TViewModel>`: the shared view base for region pages and regular user controls.
- `PrismReactiveWindow<TViewModel>`：Shell 或独立窗口基类。 `PrismReactiveWindow<TViewModel>`: the base for Shell or standalone windows.

### 8.2 不建议的“大一统基类” / The All-in-One Base Class Anti-Pattern

- 不要把导航、对话、事件发布、日志、权限、校验、忙碌态、关闭确认全部堆进一个基类。 Do not pile navigation, dialogs, event publishing, logging, authorization, validation, busy state, and close confirmation into a single monolithic base class.
- 基类应该只沉淀“高度稳定、跨模块共用、与框架契约强相关”的能力。 A base class should only contain capabilities that are highly stable, shared across modules, and strongly tied to framework contracts.

### 8.3 建议放在基类中的能力 / Capabilities That Belong in Base Classes

- `Activator`。 `Activator`.
- `LifetimeDisposables`。 `LifetimeDisposables`.
- 通用导航扩展点。 Common navigation extension points.
- 通用异常出口，例如统一处理 `ReactiveCommand.ThrownExceptions` 的帮助方法。 Common exception exits, such as helper methods for unified handling of `ReactiveCommand.ThrownExceptions`.

### 8.4 不建议放在基类中的能力 / Capabilities That Should Not Go into Base Classes

- 具体的 `RegionName` 常量。 Specific `RegionName` constants.
- 某个模块特有的加载流程。 Module-specific loading flows.
- 某个页面特有的“保存前确认”逻辑。 Page-specific “confirm before save” logic.
- 面向某个业务域的事件类型。 Domain-specific event types.

### 8.5 建议抽出的高频基础组件 / High-Frequency Reusable Utilities

- `DialogServiceExtensions`：将 `IDialogService` 包装为 Rx 可组合接口。 `DialogServiceExtensions`: wraps `IDialogService` into an Rx-composable interface.
- `NavigationContextExtensions`：提供强类型参数读取，减少字符串键与手工类型转换。 `NavigationContextExtensions`: provides strongly typed parameter access, reducing string keys and manual casting.
- `BusyMonitor`：聚合多个 `ReactiveCommand.IsExecuting`，为页面或局部区域提供单一忙碌态。 `BusyMonitor`: aggregates multiple `ReactiveCommand.IsExecuting` streams into a single busy state for a page or local region.
- `RxExceptionDispatcher` 或同类帮助器：统一将 `ThrownExceptions` 与全局异常出口收口。 `RxExceptionDispatcher` or a similar helper: centralizes `ThrownExceptions` and the global exception exit.

## 9. 一些容易不太懂的方法与接口 / Some Commonly Confusing Methods and Interfaces

### 9.1 `WhenActivated`

- 中文理解：当 View 进入可视树时建立绑定与订阅，离开时自动释放。 Chinese understanding: establish bindings and subscriptions when the View enters the visual tree, and automatically dispose them when it leaves.
- English understanding: create bindings and hot subscriptions only while the View is active in the visual tree.

### 9.2 `IActivatableViewModel`

- 这不是 Prism 生命周期的一部分，而是 ReactiveUI 提供给 ViewModel 的激活钩子。 This is not part of Prism’s lifecycle; it is the activation hook ReactiveUI provides for ViewModels.
- 它非常适合管理“页面可见时才应该活跃的观察流”。 It is ideal for managing observable streams that should exist only while the page is visible.

### 9.3 `INavigationAware`

- 它解决的是导航进入、离开、复用判断。 It solves navigation enter, leave, and reuse decisions.
- 你可以在这里解析参数，但不要把所有绑定和订阅都放进来。 You can parse parameters here, but do not place all bindings and subscriptions here.

### 9.4 `IDestructible`

- 它是实例级释放兜底。 It is the instance-level disposal safety net.
- 你可以把 `CompositeDisposable`、桥接注册、取消令牌等放到这里收尾。 You can finalize `CompositeDisposable`s, bridge registrations, and cancellation tokens here.

### 9.5 `IRegionMemberLifetime`

- 它通过 `KeepAlive` 决定页面离开区域后是否保留实例。 It uses `KeepAlive` to decide whether a page instance remains after leaving a region.
- `KeepAlive = false` 时，Prism 更容易触发实例销毁，适合编辑页、详情页等瞬态页面。 With `KeepAlive = false`, Prism is more likely to destroy the instance, which suits transient pages such as editors and detail pages.

### 9.6 `IConfirmNavigationRequest`

- 它用于“能不能离开当前页”的协商。 It is used to negotiate whether the current page is allowed to be left.
- 例如表单未保存时，你可以结合 `Interaction` 或 `IDialogService` 发起确认。 For example, when a form is unsaved, you can combine it with `Interaction` or `IDialogService` to request confirmation.

### 9.7 `Interaction<TInput, TOutput>`

- 它不是全局弹窗系统，而是 ViewModel 对当前 View 的交互请求。 It is not a global dialog system; it is a ViewModel’s interaction request toward the current View.
- 用于确认、选择、错误恢复、复制提示等都很合适。 It works well for confirmation, selection, error recovery, copy notifications, and similar flows.

### 9.8 `ReactiveCommand`

- 它比普通 `ICommand` 多了 `IsExecuting`、结果流、错误流。 It provides `IsExecuting`, result streams, and error streams beyond a normal `ICommand`.
- 在 Prism + ReactiveUI 组合里，命令统一用 `ReactiveCommand`，不要再混 `DelegateCommand`。 In a Prism + ReactiveUI combination, commands should consistently use `ReactiveCommand`; do not mix in `DelegateCommand`.

### 9.9 `ObservableAsPropertyHelper<T>`

- 它适合承接“由流推导出来的只读属性”。 It is suitable for read-only properties derived from streams.
- 比起手动在多个地方 `RaisePropertyChanged`，它更稳定、更声明式。 Compared with manually raising property changes in multiple places, it is more stable and declarative.

### 9.10 `IActiveAware`

- 它表达的是 Prism 语义下的“当前页面是否处于业务上的活跃态”，常见于 `TabControl`、多文档区域或激活页切换。 It expresses whether a page is in the Prism sense of “business-active”, which commonly appears in `TabControl`, multi-document regions, or active-page switching.
- 它与 `IActivatableViewModel` 不是一回事：前者偏业务前台态，后者偏可视树激活态。 It is not the same as `IActivatableViewModel`: the former is about business foreground state, while the latter is about visual-tree activation.

### 9.11 `RxApp.DefaultExceptionHandler`

- 它是 ReactiveUI 未处理异常的最后出口，不应留空。 It is the final exit for unhandled ReactiveUI exceptions and must not be left unconfigured.
- 即使每个命令都订阅了 `ThrownExceptions`，应用级仍应配置 `RxApp.DefaultExceptionHandler` 作为兜底。 Even if every command subscribes to `ThrownExceptions`, the application should still configure `RxApp.DefaultExceptionHandler` as a safety net.

## 10. 推荐的项目目录 / Recommended Project Layout

```text
ProjectName/
├── App.xaml
├── App.xaml.cs
├── Program.cs
├── Core/
│   ├── Constants/
│   │   └── RegionNames.cs
│   ├── Events/
│   │   └── AppEvents.cs
│   ├── Interfaces/
│   └── Navigation/
│       └── NavigationContextExtensions.cs
├── Infrastructure/
│   └── Services/
├── Modules/
│   ├── Shell/
│   │   ├── ShellModule.cs
│   │   ├── Views/
│   │   └── ViewModels/
│   └── Dashboard/
│       ├── DashboardModule.cs
│       ├── Views/
│       └── ViewModels/
└── Presentation/
    └── Core/
        ├── BusyMonitor.cs
        ├── DialogServiceExtensions.cs
        ├── RxExceptionDispatcher.cs
        ├── PrismReactiveView.cs
        ├── PrismReactiveWindow.cs
        ├── ReactiveNavigationViewModelBase.cs
        ├── ReactiveDialogViewModelBase.cs
        └── EventAggregatorExtensions.cs
```

- 这里的重点不是目录本身，而是让“基础设施、模块、表现层基类”边界清晰。 The point here is not the folder names themselves, but the clear boundaries between infrastructure, modules, and presentation-layer base types.

## 11. 推荐的设计准则 / Recommended Design Rules

- 应用组合根只保留 `PrismApplication`。 Keep `PrismApplication` as the only application composition root.
- 业务依赖注入只走 `Prism` 容器，业务代码不使用 `Locator.Current`。 Business dependency injection must go only through the `Prism` container, and business code must not use `Locator.Current`.
- 页面导航只走 Prism。 Page navigation must go through Prism only.
- 视图绑定只走 ReactiveUI。 View bindings must go through ReactiveUI only.
- 复杂双向绑定、命令与交互优先使用 `this.Bind` / `this.BindCommand`；纯展示型视觉占位允许保留 XAML `Binding` 与 `d:DataContext`。 Prefer `this.Bind` / `this.BindCommand` for complex two-way bindings, commands, and interactions; simple display-only placeholders may keep XAML `Binding` and `d:DataContext`.
- 页面生命周期分成“导航实例期”和“可视激活期”。 Page lifecycle must be split into a “navigation instance phase” and a “visual activation phase”.
- 模块级事件只走 `IEventAggregator`。 Module-level events must go through `IEventAggregator` only.
- 局部交互优先 `Interaction`。 Local interactions should prefer `Interaction`.
- 页面 VM 与对话框 VM 分基类。 Page ViewModels and dialog ViewModels must use separate base classes.
- 每个 `ReactiveCommand.ThrownExceptions` 都必须有出口，并在应用级配置 `RxApp.DefaultExceptionHandler` 兜底。 Every `ReactiveCommand.ThrownExceptions` stream must have an exit path, and `RxApp.DefaultExceptionHandler` must be configured as an application-level safety net.
- 长生命周期对象中的订阅必须统一纳入 `CompositeDisposable` 或等价释放机制。 Subscriptions inside long-lived objects must be consistently tracked in `CompositeDisposable` or an equivalent disposal mechanism.

## 12. 官方参考 / Official References

- Prism ViewModelLocator: `https://prismlibrary.github.io/docs/viewmodel-locator.html`
- Prism 导航 / Prism navigation: `https://prismlibrary.github.io/docs/wpf/legacy/Navigation.html`
- Prism 控制视图生命周期 / Prism controlling view lifetime: `https://prismlibrary.github.io/docs/wpf/region-navigation/controlling-view-lifetime.html`
- Prism 确认导航 / Prism confirming navigation: `https://prismlibrary.github.io/docs/wpf/region-navigation/confirming-navigation.html`
- Prism 事件聚合器 / Prism event aggregator: `https://prismlibrary.github.io/docs/event-aggregator.html`
- Prism WPF 对话服务 / Prism WPF dialog service: `https://prismlibrary.github.io/docs/wpf/dialog-service.html`
- ReactiveUI WhenActivated: `https://www.reactiveui.net/docs/handbook/when-activated`
- ReactiveUI Commands: `https://www.reactiveui.net/docs/handbook/commands/`
- ReactiveUI Interactions: `https://www.reactiveui.net/docs/handbook/interactions/`
- ReactiveUI View activation and `IViewFor`: `https://www.reactiveui.net/docs/handbook/view-location/index.html`
