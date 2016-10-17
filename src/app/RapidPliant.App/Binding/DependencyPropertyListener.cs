using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using RapidPliant.App.Controls;

namespace RapidPliant.App.Binding
{
    public class DependencyPropertyListener : FrameworkElement
    {
        public Action<RapidDependencyPropertyChangedEventArgs> Callback { get; set; }
        public BindingExpressionBase BindingExpression { get; set; }
        public RapidView View { get; set; }
        public DependencyObject Source { get; set; }
        public DependencyProperty Property { get; set; }

        public DependencyPropertyListener(RapidView view, DependencyObject source, DependencyProperty depProperty, Action<RapidDependencyPropertyChangedEventArgs> callback)
        {
            View = view;
            if (View != null)
            {
                View.AddRelatedObject(this);
            }
            
            Source = source;
            Property = depProperty;

            System.Windows.Data.Binding b = new System.Windows.Data.Binding(depProperty.Name);
            b.Source = source;
            
            var prop = DependencyProperty.RegisterAttached(
                "ListenAttached" + depProperty.Name,
                typeof(object),
                GetType(),
                new PropertyMetadata(OnPropertyChanged)
            );

            DataContext = view.ViewModel;
            
            BindingExpression = BindingOperations.SetBinding(this, prop, b);

            Callback = callback;
        }

        private void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Callback?.Invoke(new RapidDependencyPropertyChangedEventArgs() {
                Source = Source,
                Property = e.Property,
                View = View,
                BindingExpression = BindingExpression,
                OldVal = e.OldValue,
                NewVal = e.NewValue
            });
        }
    }

    public static class DependencyPropertyListenerExtensions
    {
        public static DependencyPropertyListener OnPropertyChange(this DependencyObject source, DependencyProperty depProperty, Action<RapidDependencyPropertyChangedEventArgs> callback)
        {
            var view = source.FindParent<RapidView>();
            return new DependencyPropertyListener(view, source, depProperty, callback);
        }
    }

    public class RapidDependencyPropertyChangedEventArgs
    {
        public RapidView View { get; set; }
        public BindingExpressionBase BindingExpression { get; set; }
        public DependencyProperty Property { get; set; }
        public DependencyObject Source { get; set; }

        public object OldVal { get; set; }
        public object NewVal { get; set; }
        
    }
}
