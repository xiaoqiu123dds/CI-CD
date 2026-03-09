namespace PrismReactiveDemo;

/// <summary>
/// 自定义 WPF 入口点。
/// Custom WPF entry point.
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main()
    {
        var application = new App();
        application.InitializeComponent();
        application.Run();
    }
}
