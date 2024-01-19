namespace GameshowPro.Common.Model;

public class KioskWindowHandler : NotifyingClass
{
    public class Settings : NotifyingClass
    {
        private int _displayIndex = 0;
        [JsonProperty, DefaultValue(0)]
        public int DisplayIndex
        {
            get { return _displayIndex; }
            set { SetProperty(ref _displayIndex, value); }
        }

        private bool _isKiosk = false;
        [JsonProperty, DefaultValue(true)]
        public bool IsKiosk
        {
            get { return _isKiosk; }
            set { SetProperty(ref _isKiosk, value); }
        }

        private bool _isVisible = true;
        [JsonProperty, DefaultValue(true)]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
    }

    private readonly double _originalWidth;
    private readonly double _originalHeight;

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

    public Window Window { get; }
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
