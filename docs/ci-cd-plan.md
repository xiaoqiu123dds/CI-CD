# CI/CD 实施计划 / CI/CD Implementation Plan

## 目标边界 / Target Scope

- 当前仓库是 `WPF + .NET 8` 桌面应用，因此第一版 `CD` 以“生成可下载发布包并自动挂载到 GitHub Release”为主，而不是直接部署到服务器。 This repository is a `WPF + .NET 8` desktop application, so the first CD iteration focuses on producing downloadable release bundles and attaching them to GitHub Releases rather than deploying to a server.
- 第一版流水线默认采用 `GitHub Actions`，因为仓库当前尚未接入其它托管平台配置。 The initial pipeline uses `GitHub Actions` by default because the repository does not yet contain configuration for other hosting platforms.
- 当前已存在编码门禁脚本 `scripts/check_encoding.ps1`，应将其纳入每次构建和发布的最前置步骤。 The existing encoding gate script `scripts/check_encoding.ps1` should run as the earliest step in every build and release.

## 当前现状 / Current State

- 已验证 `powershell.exe -ExecutionPolicy Bypass -File scripts/check_encoding.ps1 -Root .` 通过。 The encoding gate has already been verified locally.
- 已验证 `dotnet build PrismReactiveDemo.Sample.sln -c Release` 通过。 A local `Release` build has already been verified.
- 仓库此前缺少自动化测试项目、CI 工作流、发布打包脚本和发布工作流。 The repository previously lacked automated tests, CI workflows, packaging scripts, and release workflows.

## 目标流水线 / Target Pipeline

### CI（每次 Push / PR）/ CI (Every Push / PR)

1. 拉取代码。 Checkout source code.
2. 安装 `.NET 8 SDK`。 Install the `.NET 8 SDK`.
3. 执行编码检查。 Run the encoding gate.
4. 还原解决方案依赖。 Restore solution dependencies.
5. 以 `Release` 构建解决方案。 Build the solution in `Release` mode.
6. 执行单元测试。 Run unit tests.
7. 产出桌面发布包并上传为工作流制品。 Produce a desktop release bundle and upload it as a workflow artifact.

### CD（Tag / 手动发布）/ CD (Tag / Manual Release)

1. 以 `v*` Tag 或手动输入版本号触发。 Trigger from a `v*` tag or a manually supplied version.
2. 重复执行编码检查、构建和测试，确保发布链不绕过质量门禁。 Repeat encoding, build, and test steps so the release path cannot bypass quality gates.
3. 执行 `dotnet publish` 生成 `win-x64` 发布目录。 Run `dotnet publish` to generate the `win-x64` publish directory.
4. 压缩为 `.zip` 发布包，并输出 `.sha256` 校验文件。 Compress the output into a `.zip` bundle and produce a `.sha256` checksum file.
5. 自动创建或更新 GitHub Release，并挂载打包产物。 Automatically create or update a GitHub Release and attach the packaged artifacts.

## 分步实施顺序 / Step-by-Step Execution Order

### Step 1 - 建立质量门禁 / Establish Quality Gates

- 新增测试项目并补充基础单元测试。 Add a test project and cover core extension points with unit tests.
- 把编码检查、构建、测试串到同一条 CI 工作流。 Wire encoding, build, and test into one CI workflow.
- 验收标准：PR 上能看到红绿灯，失败时可定位到编码、编译或测试阶段。 Acceptance: pull requests show clear pass/fail gates, and failures are attributable to encoding, build, or tests.

### Step 2 - 建立标准发布包 / Standardize Release Bundles

- 通过统一脚本打出 `win-x64` 包，避免本地手工发布命令漂移。 Produce the `win-x64` package via a single script to avoid drift across manual local commands.
- 输出压缩包与校验文件，形成可重复下载的交付物。 Output both the archive and checksum to create repeatable deliverables.
- 验收标准：本地与 CI 使用同一脚本得到相同目录结构。 Acceptance: local runs and CI runs produce the same bundle structure through the same script.

### Step 3 - 打通 Release 发布 / Complete Release Publishing

- 使用 Tag 自动触发发布工作流。 Use tags to trigger release publishing automatically.
- 把发布包挂到 GitHub Release，便于测试与分发。 Attach release bundles to GitHub Releases for testing and distribution.
- 验收标准：创建 `v1.0.0` Tag 后，Release 页面自动出现 zip 与 sha256 文件。 Acceptance: creating a `v1.0.0` tag automatically produces a Release page with zip and sha256 files.

### Step 4 - 扩展更完整的桌面分发 / Extend to Richer Desktop Distribution

- 可选接入 `MSIX`、`ClickOnce`、企业共享目录、`WinGet` 等分发方式。 Optionally add `MSIX`, `ClickOnce`, enterprise file shares, or `WinGet` distribution.
- 若要进入签名发布，需要额外准备代码签名证书和机密配置。 Code-signing releases require additional signing certificates and secret management.
- 验收标准：确定目标分发渠道后，再补对应工作流和凭据管理。 Acceptance: once the target distribution channel is chosen, add the matching workflow and credential management.

## 本地预演命令 / Local Rehearsal Commands

- 编码检查：`powershell.exe -ExecutionPolicy Bypass -File scripts/check_encoding.ps1 -Root .`。 Encoding gate: `powershell.exe -ExecutionPolicy Bypass -File scripts/check_encoding.ps1 -Root .`.
- 构建：`dotnet build PrismReactiveDemo.Sample.sln -c Release`。 Build: `dotnet build PrismReactiveDemo.Sample.sln -c Release`.
- 测试：`dotnet test PrismReactiveDemo.Sample.sln -c Release`。 Test: `dotnet test PrismReactiveDemo.Sample.sln -c Release`.
- 打包：`powershell.exe -ExecutionPolicy Bypass -File scripts/package_release.ps1 -Version v0.1.0-local`。 Package: `powershell.exe -ExecutionPolicy Bypass -File scripts/package_release.ps1 -Version v0.1.0-local`.

## 后续增强建议 / Follow-Up Enhancements

- 引入覆盖率门槛并把结果上传到 `Codecov` 或 `Coveralls`。 Add coverage thresholds and upload reports to `Codecov` or `Coveralls`.
- 给发布流程补充变更日志模板和版本号策略。 Add a release note template and a versioning policy.
- 如果后续要发布给最终用户，优先考虑 `MSIX + 签名` 或 `WinGet`。 If the application will be distributed to end users, prioritize `MSIX + signing` or `WinGet`.
