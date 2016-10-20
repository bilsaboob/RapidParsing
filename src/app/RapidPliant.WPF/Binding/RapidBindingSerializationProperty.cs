using System.Windows;

namespace RapidPliant.WPF.Binding
{
    public class RapidBindingSerializationProperty : DependencyObject
    {
        public static readonly DependencyProperty RapidBindingProperty = DependencyProperty.Register(
            "RapidBinding", 
            typeof(string),
            typeof(RapidBindingSerializationProperty), 
            new FrameworkPropertyMetadata("", OnRapidBindingPropertyChanged)
        );

        private static void OnRapidBindingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        
        public static void SetRapidBinding(FrameworkElement elem, string value)
        {
            if (elem == null)
                return;

            elem.SetValue(RapidBindingProperty, value);
        }

        public static string GetRapidBinding(FrameworkElement elem)
        {
            return (string)elem.GetValue(RapidBindingProperty);
        }

        public static string UnwrapXaml(string xamlStr)
        {
            return xamlStr.Replace("RapidBindingSerializationProperty.RapidBinding=\"", "");
        }
    }
}
