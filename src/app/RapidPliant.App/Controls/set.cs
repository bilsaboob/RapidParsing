using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RapidPliant.App.Controls
{
    public class set : DependencyObject
    {
        public static GridLength unsetValue = new GridLength(0, GridUnitType.Pixel);

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width", typeof(GridLength), typeof(RapidGrid), new FrameworkPropertyMetadata(unsetValue, OnElemWidthPropertyChanged));
        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached("Height", typeof(GridLength), typeof(RapidGrid), new FrameworkPropertyMetadata(unsetValue, OnElemHeighPropertyChanged));

        private static void OnElemWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var elem = d as UIElement;
            if (elem == null)
                return;

            var grid = elem.FindParent<RapidGrid>();
            if (grid != null)
            {
                grid.UpdateLayoutByElement(elem);
            }
        }

        private static void OnElemHeighPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var elem = d as UIElement;
            if (elem == null)
                return;

            var grid = elem.FindParent<RapidGrid>();
            if (grid != null)
            {
                grid.UpdateLayoutByElement(elem);
            }
        }
        
        public static void SetWidth(UIElement elem, GridLength value)
        {
            if (elem == null)
                return;

            elem.SetValue(WidthProperty, value);

            var grid = elem.FindParent<RapidGrid>();
            if (grid != null)
            {
                grid.UpdateLayoutByElement(elem);
            }
        }

        public static void SetHeight(UIElement elem, GridLength value)
        {
            if (elem == null)
                return;

            elem.SetValue(HeightProperty, value);

            var grid = elem.FindParent<RapidGrid>();
            if (grid != null)
            {
                grid.UpdateLayoutByElement(elem);
            }
        }
    }
}
