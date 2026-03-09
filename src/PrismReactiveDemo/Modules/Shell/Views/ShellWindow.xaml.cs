using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using PrismReactiveDemo.Modules.Shell.ViewModels;
using PrismReactiveDemo.Presentation.Core;
using ReactiveUI;

namespace PrismReactiveDemo.Modules.Shell.Views;

/// <summary>
/// Shell 窗口示例。
/// Example Shell window.
/// </summary>
public partial class ShellWindow : PrismReactiveWindow<ShellViewModel>
{
    public ShellWindow()
    {
        InitializeComponent();
        this.StateChanged += OnWindowStateChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        // 应用退出时释放 ShellViewModel 持有的全部订阅。
        // Dispose all subscriptions held by ShellViewModel when the application exits.
        ViewModel?.Dispose();
        base.OnClosed(e);
    }

    protected override void SetupBindings(CompositeDisposable disposables)
    {
        this.BindCommand(ViewModel, vm => vm.NavigateDashboardCommand, view => view.DashboardButton)
            .DisposeWith(disposables);

        this.BindCommand(ViewModel, vm => vm.NavigateEditorCommand, view => view.EditorButton)
            .DisposeWith(disposables);

        // Sidebar Navigation RadioButtons routing
        Observable
            .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                h => DashboardNav.Checked += h,
                h => DashboardNav.Checked -= h
            )
            .Subscribe(_ => DashboardButton.Command?.Execute(null))
            .DisposeWith(disposables);

        Observable
            .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                h => EditorNav.Checked += h,
                h => EditorNav.Checked -= h
            )
            .Subscribe(_ => EditorButton.Command?.Execute(null))
            .DisposeWith(disposables);
    }

    #region Window Control Methods (Borderless)

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
        }
        else
        {
            this.WindowState = WindowState.Maximized;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (MaximizeIcon != null)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeIcon.Data = (System.Windows.Media.Geometry)FindResource("RestoreIcon");
                MaximizeButton.ToolTip = "还原 / Restore";
            }
            else
            {
                MaximizeIcon.Data = (System.Windows.Media.Geometry)FindResource("MaximizeIcon");
                MaximizeButton.ToolTip = "最大化 / Maximize";
            }
        }
    }

    protected void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            MaximizeButton_Click(sender, e);
        }
        else
        {
            if (this.WindowState == WindowState.Maximized)
            {
                var point = e.GetPosition(this);
                this.WindowState = WindowState.Normal;
                this.Left = point.X - (this.ActualWidth / 2);
                this.Top = 0;
            }
            this.DragMove();
        }
    }

    #endregion
}
