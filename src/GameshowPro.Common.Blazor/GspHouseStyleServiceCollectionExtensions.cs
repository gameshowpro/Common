using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;

namespace GameshowPro.Common.Blazor;

/// <summary>Registers the GameshowPro house style.</summary>
public static class GspHouseStyleServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything the house components read: the app's <see cref="GspHouseOptions"/> and the derived
    /// <see cref="GspAppInfo"/>, a <see cref="GspRuntimeState"/> and MudBlazor's services (so the app does not call
    /// <c>AddMudServices</c> separately). Call once in <c>Program.cs</c>:
    /// <code>builder.Services.AddGspHouseStyle(o => o.Accent = MyApp.Accent);</code>
    /// </summary>
    public static IServiceCollection AddGspHouseStyle(this IServiceCollection services, Action<GspHouseOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        GspHouseOptions options = new();
        configure?.Invoke(options);
        services.AddMudServices();
        services.TryAddSingleton(options);
        services.TryAddSingleton(sp => GspAppInfo.Resolve(sp.GetRequiredService<GspHouseOptions>(), System.Reflection.Assembly.GetEntryAssembly()));
        services.TryAddScoped<GspRuntimeState>();
        return services;
    }
}
