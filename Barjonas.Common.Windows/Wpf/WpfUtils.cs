using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Barjonas.Common.Wpf
{
    public interface IInitializableApp
    {
        void InitializeComponent();
    }
    public static class Utils
    {

        public static void SetWpfCulture()
        {
            var lang = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(TextElement), new FrameworkPropertyMetadata(lang));
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(FixedDocument), new FrameworkPropertyMetadata(lang));
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(FixedDocumentSequence), new FrameworkPropertyMetadata(lang));
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(FlowDocument), new FrameworkPropertyMetadata(lang));
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(TableColumn), new FrameworkPropertyMetadata(lang));
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(lang));
        }
    }
}
