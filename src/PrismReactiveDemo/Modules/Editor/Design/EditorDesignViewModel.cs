namespace PrismReactiveDemo.Modules.Editor.Design;

/// <summary>
/// Editor 设计时数据。
/// Design-time data for Editor.
/// </summary>
public sealed class EditorDesignViewModel
{
    public string DocumentTitle { get; set; } = "设计时文档";

    public string InstanceId { get; set; } = "EDESIGN1";

    public string FilePath { get; set; } = "C:\\Temp\\design.txt";

    public string EditableText { get; set; } = "设计器预览：核心双向编辑绑定在 code-behind 使用 this.Bind。";

    public bool IsDirty { get; set; } = true;

    public bool IsBusy { get; set; } = false;

    public string ActivityState { get; set; } = "设计器状态：未接入 IActiveAware。";

    public string HeartbeatText { get; set; } = "--:--:--";
}
