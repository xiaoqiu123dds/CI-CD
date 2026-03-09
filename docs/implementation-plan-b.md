# B 方案项目计划 / Option B Project Plan

## 1. 计划目标 / Plan Goal

- 本计划用于将 `Prism + ReactiveUI` 的主文档落地为一个**可演进、可验证、可复用**的单项目模块化 WPF 工程。 This plan turns the `Prism + ReactiveUI` main guide into a **evolvable, verifiable, and reusable** single-project modular WPF application.
- 本计划以 `PrismApplication` 为唯一组合根，以 `Prism` 管组合、`ReactiveUI` 管状态为根本原则。 This plan keeps `PrismApplication` as the single composition root and follows the fundamental rule that `Prism` manages composition while `ReactiveUI` manages state.
- 本计划当前不包含数据库、权限、日志框架、图表组件等额外扩展，只聚焦在深度集成主线。 This plan currently excludes databases, authorization, logging frameworks, and charting libraries, focusing only on the deep-integration main line.

## 2. 计划原则 / Planning Principles

- 业务依赖注入统一走 `Prism` 容器。 Business dependency injection must go through the `Prism` container.
- 业务代码不使用 `Locator.Current`。 Business code must not use `Locator.Current`.
- 页面导航统一走 `Region Navigation`。 Page navigation must go through `Region Navigation`.
- 响应式状态、命令、交互、绑定统一走 `ReactiveUI`。 Reactive state, commands, interactions, and bindings must go through `ReactiveUI`.
- 生命周期分为应用期、实例期、激活期三个重点观察层。 Lifetime must be observed through three key layers: application scope, instance scope, and activation scope.
- 所有高频可复用能力优先沉淀为小型基础组件，而不是堆进单一基类。 High-frequency reusable capabilities should first be extracted into small infrastructure components rather than pushed into one monolithic base class.

## 3. 目标交付物 / Target Deliverables

- `PrismApplication` 启动入口。 A `PrismApplication` startup entry.
- `Shell` 窗口与区域定义。 A `Shell` window with region definitions.
- `PrismReactiveView<TViewModel>` 与 `PrismReactiveWindow<TViewModel>`。 `PrismReactiveView<TViewModel>` and `PrismReactiveWindow<TViewModel>`.
- `ReactiveNavigationViewModelBase` 与 `ReactiveDialogViewModelBase`。 `ReactiveNavigationViewModelBase` and `ReactiveDialogViewModelBase`.
- `DialogServiceExtensions`、`NavigationContextExtensions`、`EventAggregatorExtensions`、`BusyMonitor`、`RxExceptionDispatcher`。 `DialogServiceExtensions`, `NavigationContextExtensions`, `EventAggregatorExtensions`, `BusyMonitor`, and `RxExceptionDispatcher`.
- 至少两个示例模块：`Dashboard` 与 `Editor`。 At least two sample modules: `Dashboard` and `Editor`.

## 4. 分阶段实施 / Phased Implementation

### Phase 1：应用骨架与组合根 / Application Skeleton and Composition Root

- 目标 / Goal：建立 `App.xaml`、`App.xaml.cs`、`Program.cs`、`PrismApplication` 入口和基础模块注册链。 Establish `App.xaml`, `App.xaml.cs`, `Program.cs`, the `PrismApplication` entry, and the base module registration chain.
- 产物 / Deliverables：`App.xaml`、`App.xaml.cs`、`Program.cs`、基础 NuGet 引用、`Shell` 模块注册。 `App.xaml`, `App.xaml.cs`, `Program.cs`, base NuGet references, and `Shell` module registration.
- 核心约束 / Constraint：此阶段只确定唯一组合根，不引入第二套容器或第二套导航模型。 This phase determines the single composition root only and must not introduce a second container or navigation model.
- 验收 / Acceptance：应用可启动到 `Shell`，容器可解析 `ShellViewModel`。 The application boots into `Shell`, and the container resolves `ShellViewModel`.

### Phase 2：表现层基类与生命周期 / Presentation Base Classes and Lifecycle

- 目标 / Goal：落地 `PrismReactiveView<TViewModel>`、`PrismReactiveWindow<TViewModel>`、`ReactiveNavigationViewModelBase`、`ReactiveDialogViewModelBase`。 Implement `PrismReactiveView<TViewModel>`, `PrismReactiveWindow<TViewModel>`, `ReactiveNavigationViewModelBase`, and `ReactiveDialogViewModelBase`.
- 产物 / Deliverables：视图强类型同步、`WhenActivated` 激活钩子、`DestroyToken`、实例级 `CompositeDisposable`。 Typed view synchronization, `WhenActivated` hooks, `DestroyToken`, and instance-level `CompositeDisposable`.
- 核心约束 / Constraint：`Unloaded` 不等于实例死亡，实例级释放只能在 `Destroy()` 或等价时机完成。 `Unloaded` must not be treated as instance death, and instance-level disposal must only happen in `Destroy()` or an equivalent lifecycle point.
- 验收 / Acceptance：切换视图时绑定自动释放，移除实例时 `Destroy()` 被触发，长任务可通过 `DestroyToken` 取消。 Bindings dispose automatically when views deactivate, `Destroy()` fires when instances are removed, and long tasks can be canceled through `DestroyToken`.

### Phase 3：Shell、Region 与导航主线 / Shell, Regions, and Navigation Backbone

- 目标 / Goal：建立 `RegionNames`、页面注册、导航门面与导航参数约定。 Establish `RegionNames`, page registration, a navigation facade, and a navigation-parameter convention.
- 产物 / Deliverables：`RegionNames.cs`、导航注册、`NavigationContextExtensions.cs`。 `RegionNames.cs`, navigation registration, and `NavigationContextExtensions.cs`.
- 核心约束 / Constraint：页面参数通过 `NavigationContext` 进入，不引入 `RoutingState`。 Page parameters enter through `NavigationContext`, and `RoutingState` must not be introduced.
- 验收 / Acceptance：可从 `Shell` 导航到 `Dashboard` 和 `Editor`，且页面可强类型读取导航参数。 Navigation works from `Shell` to `Dashboard` and `Editor`, and pages can read typed navigation parameters.

### Phase 4：对话、交互与设计时支持 / Dialogs, Interactions, and Design-Time Support

- 目标 / Goal：完成 `IDialogService`、`Interaction` 和设计时数据的职责划分。 Complete the responsibility split between `IDialogService`, `Interaction`, and design-time data.
- 产物 / Deliverables：`DialogServiceExtensions.cs`、至少一个业务对话框、至少一个 `Interaction` 示例。 `DialogServiceExtensions.cs`, at least one business dialog, and at least one `Interaction` example.
- 核心约束 / Constraint：业务弹窗走 `IDialogService`，局部交互走 `Interaction`，纯展示型 XAML 允许保留普通 `Binding` 与 `d:DataContext`。 Business dialogs go through `IDialogService`, local interactions go through `Interaction`, and display-only XAML may keep standard `Binding` and `d:DataContext`.
- 验收 / Acceptance：一个回调式对话框可被 `ShowDialogAsObservable` 串入 Rx 管道，一个设计时页面在设计器中可见占位数据。 A callback dialog can be chained through `ShowDialogAsObservable` into an Rx pipeline, and one design-time page displays placeholder data in the designer.

### Phase 5：事件桥接、线程规则与异常出口 / Event Bridging, Threading Rules, and Exception Exits

- 目标 / Goal：建立 `IEventAggregator` 的 Rx 桥接、全局 Rx 异常出口与订阅释放规范。 Establish the Rx bridge for `IEventAggregator`, the global Rx exception exit, and subscription-disposal rules.
- 产物 / Deliverables：`EventAggregatorExtensions.cs`、`RxExceptionDispatcher.cs`、`RxApp.DefaultExceptionHandler` 初始化。 `EventAggregatorExtensions.cs`, `RxExceptionDispatcher.cs`, and `RxApp.DefaultExceptionHandler` initialization.
- 核心约束 / Constraint：桥接扩展本身保持线程中立；UI 绑定更新由调用方显式 `.ObserveOn(RxApp.MainThreadScheduler)`。 The bridge extension itself stays thread-neutral; UI-bound updates explicitly `.ObserveOn(RxApp.MainThreadScheduler)` at the call site.
- 验收 / Acceptance：跨模块事件可桥接到 Rx 流，异常既能进入 `ThrownExceptions` 处理链，也有应用级兜底出口。 Cross-module events bridge into Rx streams, and exceptions flow through both `ThrownExceptions` handling and an application-level safety net.

### Phase 6：忙碌态聚合与页面行为稳定化 / Busy State Aggregation and Page Behavior Stabilization

- 目标 / Goal：建立 `BusyMonitor`，并验证 `IActiveAware`、`IActivatableViewModel`、`IRegionMemberLifetime` 的协同行为。 Build `BusyMonitor` and validate the coordinated behavior of `IActiveAware`, `IActivatableViewModel`, and `IRegionMemberLifetime`.
- 产物 / Deliverables：`BusyMonitor.cs`、页面级 Loading 示例、活动页状态切换示例。 `BusyMonitor.cs`, a page-level loading example, and an active-page state-switching example.
- 核心约束 / Constraint：业务激活和可视激活必须分离建模；`KeepAlive` 策略由页面按语义显式决定。 Business activation and visual activation must be modeled separately, and the `KeepAlive` strategy must be chosen explicitly per page semantics.
- 验收 / Acceptance：页面能区分“当前被选中”和“当前在可视树激活”，多个命令可汇聚为单一忙碌态。 A page can distinguish “currently selected” from “currently activated in the visual tree”, and multiple commands can aggregate into one busy state.

### Phase 7：示例模块与规范验证 / Sample Modules and Rule Validation

- 目标 / Goal：用 `Dashboard` 与 `Editor` 模块验证整套规范。 Use `Dashboard` and `Editor` modules to validate the full set of rules.
- 产物 / Deliverables：`DashboardModule`、`EditorModule`、对应 View / ViewModel / Dialog / Event 示例。 `DashboardModule`, `EditorModule`, and the corresponding View / ViewModel / Dialog / Event examples.
- 核心约束 / Constraint：示例必须覆盖页面复用、离开确认、局部交互、跨模块事件、忙碌态聚合。 The samples must cover page reuse, leave confirmation, local interactions, cross-module events, and busy-state aggregation.
- 验收 / Acceptance：至少完成一次完整链路：导航进入 → 异步执行 → Busy 显示 → 异常出口 → 事件发布 → 导航离开 / 销毁。 At least one full chain must work: navigation enter → async execution → busy indication → exception exit → event publication → navigation leave / destroy.

### Phase 8：收尾、文档与回归验证 / Finalization, Documentation, and Regression Validation

- 目标 / Goal：收束文档、补足约束说明并执行编码检查。 Consolidate documents, complete the rule descriptions, and run encoding validation.
- 产物 / Deliverables：更新后的 `README.md`、`Issues.md`、主文档、项目计划。 Updated `README.md`, `Issues.md`, the main guide, and the project plan.
- 核心约束 / Constraint：不得在未更新 `Issues.md` 的情况下报告完成。 Completion must not be reported before `Issues.md` has been updated.
- 验收 / Acceptance：编码检查通过，文档与工程骨架保持一致。 The encoding check passes, and the documents remain aligned with the project skeleton.

## 5. 实施顺序建议 / Recommended Execution Order

- 第一优先级 / Priority 1：`PrismApplication`、`Shell`、Region、基类体系。 `PrismApplication`, `Shell`, Regions, and base classes.
- 第二优先级 / Priority 2：导航参数、对话/交互边界、异常出口。 Navigation parameters, dialog/interaction boundaries, and exception exits.
- 第三优先级 / Priority 3：事件桥接、忙碌态聚合、活动态区分。 Event bridging, busy-state aggregation, and active-state differentiation.
- 第四优先级 / Priority 4：示例模块、回归验证、文档收尾。 Sample modules, regression validation, and documentation finalization.

## 6. 关键验收标准 / Key Acceptance Criteria

- 所有页面 View 均通过 `WhenActivated` 注册释放敏感绑定。 All page Views register disposal-sensitive bindings through `WhenActivated`.
- 所有页面 ViewModel 均具备实例级释放能力，并暴露 `DestroyToken` 给长任务使用。 All page ViewModels support instance-level disposal and expose `DestroyToken` for long-running tasks.
- 所有业务服务与 ViewModel 均由 `Prism` 容器解析。 All business services and ViewModels are resolved by the `Prism` container.
- 所有复杂命令均使用 `ReactiveCommand`，且 `ThrownExceptions` 有显式出口。 All complex commands use `ReactiveCommand`, and `ThrownExceptions` has an explicit exit path.
- 对话服务、交互、导航、事件总线的职责边界可用示例清晰证明。 The responsibilities of dialog service, interaction, navigation, and event bus are clearly demonstrated with examples.

## 7. 风险与控制 / Risks and Controls

- 风险 / Risk：把 `Unloaded` 或 `DataContext = null` 误判为实例销毁。 Mistaking `Unloaded` or `DataContext = null` for instance destruction.
  - 控制 / Control：视图层只处理激活与绑定释放，实例销毁只由 `Destroy()` 负责。 The view layer handles only activation and binding disposal, while instance destruction is owned only by `Destroy()`.

- 风险 / Risk：`ThrownExceptions` 未订阅导致异常静默。 `ThrownExceptions` remains unsubscribed and exceptions go silent.
  - 控制 / Control：页面级命令统一接入异常帮助器，应用级配置 `RxApp.DefaultExceptionHandler`。 Page-level commands must use a shared exception helper, and the application configures `RxApp.DefaultExceptionHandler`.

- 风险 / Risk：跨模块通信退化为 `MessageBus` 或静态事件。 Cross-module communication degrades into `MessageBus` or static events.
  - 控制 / Control：跨模块事件一律进入 `IEventAggregator`，内部再桥接成 Rx。 Cross-module events must first enter `IEventAggregator` and only then bridge into Rx.

- 风险 / Risk：把所有能力塞进同一个基类。 Shoving every capability into one base class.
  - 控制 / Control：优先抽出 `BusyMonitor`、参数扩展、对话扩展、异常分发器等小型基础组件。 Prefer extracting small foundation components such as `BusyMonitor`, parameter extensions, dialog extensions, and exception dispatchers.

## 8. 完成定义 / Definition of Done

- 主文档中的规则已在工程结构或示例模块中得到落实。 The rules in the main guide have been implemented in the project structure or sample modules.
- 项目计划、README、Issues 三者内容保持一致。 The project plan, README, and Issues remain aligned.
- 编码检查通过：`powershell.exe -ExecutionPolicy Bypass -File scripts/check_encoding.ps1 -Root .`。 The encoding check passes: `powershell.exe -ExecutionPolicy Bypass -File scripts/check_encoding.ps1 -Root .`.
- 下一阶段可直接进入代码骨架生成，不再重新争论容器、导航和生命周期边界。 The next phase can move directly into code skeleton generation without reopening debates about containers, navigation, or lifecycle boundaries.
