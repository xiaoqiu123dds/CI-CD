using System.Reactive.Linq;
using Prism.Dialogs;
using PrismReactiveDemo.Presentation.Core;

namespace PrismReactiveDemo.Tests;

public sealed class DialogServiceExtensionsTests
{
    [Fact]
    public async Task ShowDialogAsObservable_ShouldEmitDialogResult()
    {
        var expectedResult = new DialogResult(ButtonResult.OK);
        var dialogService = new FakeDialogService(expectedResult);
        var parameters = new DialogParameters
        {
            { "Title", "测试标题" }
        };

        var results = await dialogService
            .ShowDialogAsObservable("Info", parameters)
            .ToList();

        var actual = Assert.Single(results);
        Assert.Same(expectedResult, actual);
        Assert.Equal("Info", dialogService.LastDialogName);
        Assert.Same(parameters, dialogService.LastParameters);
    }

    private sealed class FakeDialogService : IDialogService
    {
        private readonly IDialogResult _result;

        public FakeDialogService(IDialogResult result)
        {
            _result = result;
        }

        public string? LastDialogName { get; private set; }

        public IDialogParameters? LastParameters { get; private set; }

        public void ShowDialog(string name, IDialogParameters parameters, DialogCallback callback)
        {
            LastDialogName = name;
            LastParameters = parameters;
            callback.Invoke(_result);
        }
    }
}
