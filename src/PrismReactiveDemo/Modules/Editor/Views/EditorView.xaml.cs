using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Win32;
using PrismReactiveDemo.Modules.Editor.ViewModels;
using PrismReactiveDemo.Presentation.Core;
using ReactiveUI;

namespace PrismReactiveDemo.Modules.Editor.Views;

/// <summary>
/// Editor 页面。
/// Editor page.
/// </summary>
public partial class EditorView : PrismReactiveView<EditorViewModel>
{
    public EditorView()
    {
        InitializeComponent();
    }

    protected override void SetupBindings(CompositeDisposable disposables)
    {
        this.Bind(ViewModel, vm => vm.EditableText, view => view.EditorTextBox.Text)
            .DisposeWith(disposables);

        this.BindCommand(ViewModel, vm => vm.SaveCommand, view => view.SaveButton)
            .DisposeWith(disposables);

        this.BindCommand(ViewModel, vm => vm.PickFileCommand, view => view.PickFileButton)
            .DisposeWith(disposables);
    }

    protected override void SetupInteractions(CompositeDisposable disposables)
    {
        var interactionRegistration = new SerialDisposable();
        interactionRegistration.DisposeWith(disposables);

        // 响应式监听 ViewModel 的装配时机，防止 ViewModelLocator 注入滞后时静默跳过注册。
        // Reactively wait for ViewModel injection to avoid silent null-skip when ViewModelLocator is late.
        this.WhenAnyValue(x => x.ViewModel)
            .WhereNotNull()
            .Subscribe(vm =>
                vm.PickFileInteraction.RegisterHandler(context =>
                    {
                        var dialog = new OpenFileDialog
                        {
                            Title = "选择任意示例文件 / Pick Any Example File",
                            Filter = "Text Files|*.txt|All Files|*.*",
                        };

                        context.SetOutput(dialog.ShowDialog() == true ? dialog.FileName : null);
                    })
                    .DisposeWith(disposables)
            )
            .DisposeWith(disposables);
    }
}
