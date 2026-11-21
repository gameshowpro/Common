namespace GameshowPro.Common.Blazor;

/// <summary>
/// Copies text to the clipboard from the browser, also when the page is not in a secure context (plain HTTP on a LAN), where
/// <c>navigator.clipboard</c> is missing. Registered by <c>AddGspHouseStyle</c>; an app that does not call it can register it
/// itself (<c>services.AddScoped&lt;GspClipboard&gt;()</c>) because it needs only an <see cref="IJSRuntime"/>.
/// </summary>
public sealed class GspClipboard(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    /// <summary>Copies <paramref name="text"/>. Returns false, and never throws, when the browser refuses or interop is unavailable.</summary>
    public async ValueTask<bool> CopyAsync(string text)
    {
        try
        {
            _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./_content/GameshowPro.Common.Blazor/gsp-clipboard.js");
            return await _module.InvokeAsync<bool>("copy", text);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (Exception)
            {
                // The circuit or page has gone; nothing to release.
            }
            _module = null;
        }
    }
}
