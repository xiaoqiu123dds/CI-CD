using System.Reactive.Disposables;
using System.Reactive.Linq;
using Prism.Dialogs;

namespace PrismReactiveDemo.Presentation.Core;

/// <summary>
/// Prism 对话服务响应式扩展。
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
