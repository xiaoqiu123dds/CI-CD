using Prism.Navigation;
using Prism.Navigation.Regions;
using PrismReactiveDemo.Core.Navigation;

namespace PrismReactiveDemo.Tests;

public sealed class NavigationContextExtensionsTests
{
    [Fact]
    public void GetRequired_ShouldThrowWhenKeyIsMissing()
    {
        var navigationContext = CreateContext();

        void Action()
        {
            _ = navigationContext.GetRequired<Guid>("DocumentId");
        }

        var exception = Assert.Throws<InvalidOperationException>(Action);
        Assert.Contains("DocumentId", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetValueOrDefault_ShouldReturnDefaultWhenKeyIsMissing()
    {
        var navigationContext = CreateContext();

        var actual = navigationContext.GetValueOrDefault<int>("VisitCount");

        Assert.Equal(default, actual);
    }

    [Fact]
    public void CreateParameter_ShouldCreateNavigationParameters()
    {
        var parameters = PrismReactiveDemo.Core.Navigation.NavigationContextExtensions.CreateParameter(
            "DocumentTitle",
            "Dashboard");

        Assert.True(parameters.ContainsKey("DocumentTitle"));
        Assert.Equal("Dashboard", parameters.GetValue<string>("DocumentTitle"));
    }

    private static NavigationContext CreateContext(Uri? uri = null, INavigationParameters? parameters = null)
    {
        return new NavigationContext(
            null!,
            uri ?? new Uri("https://localhost/editor", UriKind.Absolute),
            parameters ?? new NavigationParameters());
    }
}
