using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace PrismReactiveDemo.Infrastructure.Diagnostics;

/// <summary>
/// 应用级未处理异常捕获器。
/// Application-level handler for unhandled exceptions.
/// </summary>
public static class GlobalExceptionHandler
{
    private static bool _isInitialized;
    private static Action<string, Exception>? _handleException;

    public static void Initialize(Application application, Action<string, Exception> handleException)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _handleException = handleException;

        application.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _handleException?.Invoke(nameof(Application.DispatcherUnhandledException), e.Exception);
        e.Handled = true;
    }

    private static void OnAppDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _handleException?.Invoke(nameof(AppDomain.CurrentDomain.UnhandledException), exception);
            return;
        }

        Debug.WriteLine(e.ExceptionObject?.ToString());
    }

    private static void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _handleException?.Invoke(nameof(TaskScheduler.UnobservedTaskException), e.Exception);
        e.SetObserved();
    }
}
