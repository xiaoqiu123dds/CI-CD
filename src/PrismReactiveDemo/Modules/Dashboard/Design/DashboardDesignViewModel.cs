namespace PrismReactiveDemo.Modules.Dashboard.Design;

/// <summary>
/// Dashboard 设计时数据。
/// Design-time data for Dashboard.
/// </summary>
public sealed class DashboardDesignViewModel
{
    public string WelcomeMessage { get; set; } = "设计时示例：Dashboard 使用 XAML Binding 展示静态文本与占位内容。";

    public string LastEditorMessage { get; set; } = "设计器预览：最近一次保存来自 Editor-AB12CD34。";

    public string LastRefreshAt { get; set; } = "2026-03-07 10:30:00";

    public string InstanceId { get; set; } = "DESIGN01";

    public int VisitCount { get; set; } = 2;

    public bool IsBusy { get; set; } = false;
}
