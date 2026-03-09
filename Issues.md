# Issues

## 2026-03-08 - 建立首版 GitHub Actions CI/CD 流水线 / Established the Initial GitHub Actions CI/CD Pipeline

- **问题 / Issue**：仓库已有编码门禁与可编译的 `WPF` 示例，但此前缺少自动化测试、持续集成工作流、统一打包脚本和可重复的发布链路。 The repository already had an encoding gate and a compilable `WPF` sample, but it previously lacked automated tests, CI workflows, a unified packaging script, and a repeatable release path.
- **处理 / Resolution**：
  - 新增 `tests/PrismReactiveDemo.Tests`，为导航参数扩展、对话服务扩展、事件总线桥接和异常分发补充基础单元测试。
  - 新增 `.github/workflows/ci.yml`，在 `Push / Pull Request / workflow_dispatch` 时执行编码检查、依赖还原、`Release` 构建、单元测试和桌面发布包制品上传。
  - 新增 `.github/workflows/release.yml` 与 `scripts/package_release.ps1`，支持基于 `v*` Tag 或手动版本号生成 `win-x64` 压缩包与 `SHA256` 文件，并自动发布到 GitHub Release。
  - 新增 `docs/ci-cd-plan.md` 并更新 `README.md`，补齐 CI/CD 边界、执行顺序和本地预演命令。
- **结果 / Result**：仓库现已具备“编码门禁 → 构建 → 测试 → 打包 → Release 发布”的首版 CI/CD 闭环，后续可继续扩展 `MSIX`、签名发布或 `WinGet` 分发。 The repository now has an initial CI/CD loop covering “encoding gate → build → test → package → release publishing”, and can later be extended with `MSIX`, signed releases, or `WinGet` distribution.
## 2026-03-07 - 前端全局重构：引入 Stitch 辅助与 IndusTech 科技感规范 / Global UI Refactoring: Stitch-Assisted IndusTech Modernization

- **问题 / Issue**：用户提出现有界面过于简陋，需要配合前端技能 (`wpf-ui-designer`) 进行现代风格改造，并利用 Stitch 创建高质量初盘设计。 The user requested a modern UI overhaul utilizing the frontend skill (`wpf-ui-designer`) and high-quality initial layout generation using Stitch.
- **处理 / Resolution**：
  - 调用 Stitch MCP 生成了一个具有科技工业感（Deep Slate 岩板黑 + Electric Cyan 赛博青）的现代 Dashboard Web 结构图。
  - 将生成的结构人工转换为 WPF XAML，并引入 `MahApps.Metro` 框架和项目中的 `IndusTechTemplates` 资产文件。
  - 重写 `ShellWindow.xaml` 为“无边框自定义窗口 (Borderless Custom Titlebar)”并接入拖动/最小化控制。
  - 重写 `DashboardView.xaml` 布局：增加了欢迎标题头、三个并排的数据指示卡片（CardStyle）、活动日志悬浮面板，以及底部右对齐的按钮组。
  - 同步修改绑定逻辑代码，处理了侧边栏菜单切换引起的路由命令跳转。
- **结果 / Result**：`dotnet build` 完全通过，`PrismReactiveDemo` 的前端完成全面替换。项目不仅在底层具备深度的 Prism + ReactiveUI 工程标准，在表层也达到了工业高定科技感 UI 的像素级要求。
## 2026-03-07 - 深度代码重构：Bug修复 + 架构反模式清除 + C#12现代化 / Deep Refactoring: Bug Fixes, Architecture Anti-pattern Removal, C#12 Modernization

- **问题 / Issue**：全量代码审查发现 2 个真实 Bug、3 个架构反模式和若干 C# 语法可精简之处。 A full codebase review found 2 real bugs, 3 architecture anti-patterns, and several C# syntax simplification opportunities.
- **Bug 修复 / Bug Fixes**：
  - `EditorView.SetupInteractions`：`ViewModel?.` 静默跳过改为 `WhenAnyValue(x => x.ViewModel).WhereNotNull()` 响应式注册，彻底消除 Interaction 静默失效漏洞。
  - `ShellViewModel._disposables` 永不释放：实现 `IDisposable`，在 `ShellWindow.OnClosed` 中触发释放，防止 EventAggregator 订阅全程泄漏。
  - `ConfirmDialogViewModel.Title => DialogTitle`：计算属性不触发通知，改为基类 `[Reactive] public virtual string Title`，子类 `OnDialogOpened` 直接赋值，对话框标题栏可正确刷新。
- **架构反模式 / Anti-patterns Removed**：
  - `PrismReactiveView.DataContextChanged` 冗余订阅（基类已实现）已删除，防止双重变更通知。
  - `BusyMonitor` 类（带状态变量和线程竞态）删除，改为 `Observable.CombineLatest` 纯 Rx 聚合（3行替代50行）。
  - `ReactiveNavigationViewModelBase` 加入 `IRegionMemberLifetime` 接口和 `virtual bool KeepAlive => true`，消灭子类重复声明。
- **C# 现代化 / C#12 Modernization**：
  - `DashboardViewModel` 和 `EditorViewModel` 的 `ObservableAsPropertyHelper` 三件套改为 `[ObservableAsProperty] + ToPropertyEx`（Fody 织入）。
  - 删除 `ToPropertyEx` 前多余的 `.ObserveOn(RxApp.MainThreadScheduler)`。
- **结果 / Result**：`dotnet build` 编译通过，0 错误。所有修改经过手动验证，项目代码与 B 方案架构文档对齐度显著提升。

## 2026-03-07 - 修正 Prism + ReactiveUI 示例中的生命周期与响应式反模式 / Fixed Lifecycle and Reactive Anti-Patterns in the Prism + ReactiveUI Sample

- 问题 / Issue：示例项目中的 `EditorView` 交互注册依赖激活时机，可能在 `ViewModelLocator` 尚未完成注入时静默跳过；同时 `PrismReactiveView` / `PrismReactiveWindow` 手工同步 `DataContext`，以及 `BusyMonitor` + 传统 `ObservableAsPropertyHelper` 写法，也让示例偏离了更自然的 ReactiveUI 风格。 The sample project's `EditorView` interaction registration depended on activation timing and could silently skip registration before `ViewModelLocator` finished injection; meanwhile, `PrismReactiveView` / `PrismReactiveWindow` manually synchronized `DataContext`, and the `BusyMonitor` + traditional `ObservableAsPropertyHelper` pattern also pushed the sample away from a more natural ReactiveUI style.
- 影响 / Impact：运行时可能出现文件选择交互无响应却无异常提示，视图与 `DataContext` 的双重桥接增加了重复通知风险，而命令忙碌态的命令式封装则削弱了“状态即流”的示例表达。 At runtime, file-picking interactions could fail silently without any error, the double bridge between the view and `DataContext` increased the risk of duplicate notifications, and the imperative busy-state wrapper weakened the sample's “state as a stream” expression.
- 处理 / Resolution：改为在 `EditorView.xaml.cs` 中通过 `WhenAnyValue(ViewModel)` 动态注册 `Interaction` 处理器；移除了 `PrismReactiveView.cs` 与 `PrismReactiveWindow.cs` 的冗余 `DataContextChanged` 桥接；删除 `BusyMonitor.cs`，改由 `DashboardViewModel.cs` 与 `EditorViewModel.cs` 直接组合 `ReactiveCommand.IsExecuting` 并通过 `ToPropertyEx` 生成 `IsBusy`。 Updated `EditorView.xaml.cs` to dynamically register `Interaction` handlers through `WhenAnyValue(ViewModel)`; removed the redundant `DataContextChanged` bridge from `PrismReactiveView.cs` and `PrismReactiveWindow.cs`; deleted `BusyMonitor.cs`, and now both `DashboardViewModel.cs` and `EditorViewModel.cs` compose `ReactiveCommand.IsExecuting` directly and expose `IsBusy` through `ToPropertyEx`.
- 结果 / Result：示例项目现在更贴近“Prism 管组合、ReactiveUI 管状态”的目标模型，交互注册不再依赖脆弱时序，忙碌态示例也改成了更直观的响应式表达。 The sample project now aligns more closely with the target model of “Prism manages composition, ReactiveUI manages state”, interaction registration no longer depends on fragile timing, and the busy-state demo now uses a more direct reactive expression.

## 2026-03-07 - 生成 Prism + ReactiveUI 示例项目 / Generated Prism + ReactiveUI Sample Project

- 问题 / Issue：主文档和项目计划已经形成，但仓库中还没有一个能直接运行并覆盖关键条目的示例工程。 The main guide and project plan were already in place, but the repository still lacked a runnable sample project that covered the key items.
- 影响 / Impact：如果没有可执行示例，生命周期、导航、对话服务、事件桥接、异常出口和活动态差异只能停留在文档层面，不利于验证和复用。 Without an executable sample, lifecycle, navigation, dialog service, event bridging, exception exits, and active-state differences would remain only at the documentation level, making validation and reuse harder.
- 处理 / Resolution：新增 `PrismReactiveDemo.Sample.sln` 与 `src/PrismReactiveDemo/PrismReactiveDemo.csproj`，并补充 `docs/example-project-coverage.md` 用于逐项映射主文档条目。 Added `PrismReactiveDemo.Sample.sln` and `src/PrismReactiveDemo/PrismReactiveDemo.csproj`, and added `docs/example-project-coverage.md` to map the main guide items one by one.
- 结果 / Result：当前仓库已经同时具备“原则文档 + 项目计划 + 示例工程 + 覆盖映射”的完整交付链。 The repository now contains a complete delivery chain of “principle guide + project plan + sample project + coverage map”.

## 2026-03-07 - 并入深度集成优化项并重写项目计划 / Merged Deep-Integration Optimizations and Rewrote Project Plan

- 问题 / Issue：主文档此前已建立 `Prism + ReactiveUI` 的深度集成主线，但仍缺少若干高价值的工程化补充，例如容器边界约束、`DestroyToken`、响应式对话扩展、忙碌态聚合、异常出口与活动态区分。 The main guide had already established the deep-integration backbone of `Prism + ReactiveUI`, but it still lacked several high-value engineering supplements such as container-boundary rules, `DestroyToken`, reactive dialog extensions, busy-state aggregation, exception exits, and active-state differentiation.
- 影响 / Impact：如果这些约束不进入主文档与计划，后续落地项目时仍可能出现容器混用、异常静默、长任务泄漏、忙碌态分散和活动态语义混淆。 If these constraints were not merged into the main guide and plan, implementation could still suffer from mixed containers, silent exceptions, long-task leaks, fragmented busy states, and confused active-state semantics.
- 处理 / Resolution：已将认可的补充项并入 `docs/prism-reactiveui-deep-integration.md`，并基于新的主文档重写 `docs/implementation-plan-b.md`。 The accepted supplements have been merged into `docs/prism-reactiveui-deep-integration.md`, and `docs/implementation-plan-b.md` has been rewritten based on the updated main guide.
- 结果 / Result：当前文档体系已收敛为“主文档定义原则，项目计划定义实施顺序与验收标准”的一致结构。 The current documentation set now converges on a consistent structure: the main guide defines principles, while the project plan defines execution order and acceptance criteria.

## 2026-03-07 - 移除 Generic Host 设计定位 / Removed Generic Host Positioning

- 问题 / Issue：当前 `B` 方案文档仍保留了少量 `Generic Host` 表述，容易让项目在 `PrismApplication` 之外再形成第二套应用组合根。 The current option `B` documents still retained a small amount of `Generic Host` wording, which could lead the project to form a second application composition root outside `PrismApplication`.
- 影响 / Impact：会让容器职责、启动链路和生命周期边界变得模糊，削弱 `Prism + ReactiveUI` 深度集成的主线。 This would blur container responsibilities, startup flow, and lifecycle boundaries, weakening the main line of the `Prism + ReactiveUI` deep-integration design.
- 处理 / Resolution：已从 `README.md`、`docs/prism-reactiveui-deep-integration.md`、`docs/implementation-plan-b.md` 中移除 `Generic Host` 定位，并明确 `PrismApplication` 是唯一组合根。 Removed the `Generic Host` positioning from `README.md`, `docs/prism-reactiveui-deep-integration.md`, and `docs/implementation-plan-b.md`, and explicitly defined `PrismApplication` as the single composition root.
- 结果 / Result：当前 `B` 方案收敛为“`Prism` 管组合、`ReactiveUI` 管状态、`PrismApplication` 为唯一入口”的一致模型。 The current option `B` now converges on a consistent model: “`Prism` manages composition, `ReactiveUI` manages state, and `PrismApplication` is the only entry root”.

## 2026-03-07 - Prism + ReactiveUI 深度集成方案补充 / Prism + ReactiveUI Deep Integration Plan Added

- 问题 / Issue：技能初版已具备 `Prism` 与 `ReactiveUI` 的基础拼接，但生命周期、资源释放、区域导航、对话服务、事件总线和通用基类的职责边界仍然偏表层。 The initial skill draft already stitches together `Prism` and `ReactiveUI`, but the responsibility boundaries around lifecycle, disposal, region navigation, dialog service, event bus, and common base classes remain superficial.
- 影响 / Impact：如果直接在此基础上扩展项目，容易产生双重生命周期、重复释放、导航与对话模型混杂、以及事件通道失控的问题。 If a project expands directly on top of this draft, it can easily introduce dual lifecycles, double-disposal, mixed navigation/dialog models, and uncontrolled event channels.
- 处理 / Resolution：新增深度说明文档 `docs/prism-reactiveui-deep-integration.md` 与构建计划 `docs/implementation-plan-b.md`，并补充仓库入口说明与编码检查脚本。 Added the deep integration guide `docs/prism-reactiveui-deep-integration.md`, the build plan `docs/implementation-plan-b.md`, plus repository entry documentation and an encoding check script.
- 结果 / Result：后续 `B` 方案项目应以“Prism 管组合、ReactiveUI 管状态”的单一权威模型推进。 Future option `B` projects should proceed with the single-authority model of “Prism manages composition, ReactiveUI manages state”.

