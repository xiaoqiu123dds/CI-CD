using System.Reactive.Disposables;
using System.Threading;
using Prism.Navigation.Regions;
using ReactiveUI;

namespace PrismReactiveDemo.Presentation.Core;

/// <summary>
/// 页面级导航 ViewModel 基类。
/// Base navigation ViewModel for pages.
/// </summary>
public abstract class ReactiveNavigationViewModelBase
    : ReactiveObject,
        INavigationAware,
        IDestructible,
        IActivatableViewModel,
        IRegionMemberLifetime
{
    private bool _isDestroyed;
    private readonly CancellationTokenSource _destroyCts = new();

    protected CompositeDisposable LifetimeDisposables { get; } = new();

    protected CancellationToken DestroyToken => _destroyCts.Token;

    // 默认保留页面实例。将页面设为瞬态时，子类 override 返回 false 即可。
    // Page instances are kept alive by default. Subclasses override to return false for transient pages.
    public virtual bool KeepAlive => true;

    public ViewModelActivator Activator { get; } = new();

    protected ReactiveNavigationViewModelBase()
    {
        this.WhenActivated(disposables =>
        {
            OnActivated(disposables);
        });
    }

    protected virtual void OnActivated(CompositeDisposable disposables) { }

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

    protected virtual void OnNavigatedToCore(NavigationContext navigationContext) { }

    protected virtual void OnNavigatedFromCore(NavigationContext navigationContext) { }

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

    protected virtual void OnDestroyedCore() { }
}
