namespace GameshowPro.Common.View;

/// <summary>
/// A version subclass of <see cref="ComboBox"/> adds a <see cref="SelectedIndexCustom"/> property which can be databound.
/// Changes are relayed in both directions while counteracting the default behavior of permanently clearing the SelectedIndex whenever the ItemsSource changes.
/// </summary>
public class ComboBoxCustom : ComboBox
{
    private bool _settingSelectedIndex;
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ItemsSourceProperty)
        {
            SelectedIndex = SelectedIndexCustom ?? -1;
        }
        else if (e.Property == SelectedItemProperty && !_settingSelectedIndex)
        {
            int index = SelectedIndex;
            if (index >= 0)
            {
                _settingSelectedIndex = true;
                SelectedIndexCustom = index;
                _settingSelectedIndex = false;
            }
        }
    }

    public int? SelectedIndexCustom
    {
        get { return (int?)GetValue(s_selectedIndexCustomProperty); }
        set { SetValue(s_selectedIndexCustomProperty, value); }
    }

    public static readonly DependencyProperty s_selectedIndexCustomProperty =
        DependencyProperty.Register("SelectedIndexCustom", typeof(int?), typeof(ComboBoxCustom), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, SelectedIndexCustomPropertyChanged));

    private static void SelectedIndexCustomPropertyChanged(DependencyObject element,
            DependencyPropertyChangedEventArgs e)
    {
        if (element is ComboBoxCustom comboBox && !comboBox._settingSelectedIndex)
        {
            comboBox._settingSelectedIndex = true;
            comboBox.SelectedIndex = ((int?)e.NewValue) ?? -1;
            comboBox._settingSelectedIndex = false;
        }
    }
}
