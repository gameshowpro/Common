namespace GameshowPro.Common.Model;

/// <summary>
/// Manages a WPF window in kiosk or normal mode and tracks persisted settings.
/// </summary>
/// <remarks>Docs added by AI.</remarks>
public class KioskWindowHandler : ObservableClass
{
    /// <summary>
    /// Persisted window settings for kiosk behavior and screen selection.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public class Settings : ObservableClass
    {
        /// <summary>
        /// The target display index for the window.
        /// </summary>
        /// <remarks>Docs added by AI.</remarks>
        [DataMember, DefaultValue(0)]
        public int DisplayIndex
        {
            get;
            set { SetProperty(ref field, value); }
        } = 0;

        /// <summary>
        /// Whether the window is presented in kiosk mode.
        /// </summary>
        /// <remarks>Docs added by AI.</remarks>
        [DataMember, DefaultValue(true)]
        public bool IsKiosk
        {
            get;
            set { SetProperty(ref field, value); }
        } = false;

        /// <summary>
        /// Whether the window is currently shown.
        /// </summary>
        /// <remarks>Docs added by AI.</remarks>
        [DataMember, DefaultValue(true)]
        public bool IsVisible
        {
            get;
            set { SetProperty(ref field, value); }
        } = true;
    }

    private readonly double _originalWidth;
    private readonly double _originalHeight;

    /// <summary>
    /// Creates a handler for the specified window and settings.
    /// </summary>
    /// <param name="window">The WPF window to manage.</param>
    /// <param name="settings">The persisted settings to apply.</param>
    /// <remarks>Docs added by AI.</remarks>
    public KioskWindowHandler(Window window, Settings settings)
    {
        Window = window;
        _originalWidth = window.Width;
        _originalHeight = window.Height;
        window.Closing += Window_Closing;
        CurrentSettings = settings;
        CurrentSettings.PropertyChanged += _settings_PropertyChanged;
        ApplySettings();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        CurrentSettings.IsVisible = false;
    }

    /// <summary>
    /// The managed window.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public Window Window { get; }
    /// <summary>
    /// The current settings backing this handler.
    /// </summary>
    /// <remarks>Docs added by AI.</remarks>
    public Settings CurrentSettings { get; }

    private void _settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Settings.IsKiosk):
            case nameof(Settings.DisplayIndex):
            case nameof(Settings.IsVisible):
                ApplySettings();
                break;
        }
    }

    private void ApplySettings()
    {
        if (CurrentSettings.IsVisible)
        {
            bool kiosk = CurrentSettings.IsKiosk && Screen.SizeWindowToScreen(Window, CurrentSettings.DisplayIndex);
            Window.Show();
            Window.ShowInTaskbar = true;
            if (kiosk)
            {
                Window.WindowStyle = WindowStyle.None;
                Window.ResizeMode = ResizeMode.NoResize;
                Window.Cursor = Cursors.None;
                Window.Topmost = true;
            }
            else
            {
                Window.Width = _originalWidth;
                Window.Height = _originalHeight;
                Window.WindowStyle = WindowStyle.ToolWindow;
            }
        }
        else
        {
            Window.Hide();
        }
    }
}
