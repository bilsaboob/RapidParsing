using System.Windows;
using RapidPliant.WPF.Controls;
using RapidPliant.WPF.Mvx;

namespace RapidPliant.WPF.Binding
{
    public static class RapidBindingHelpers
    {
        public static object FindDataContexts(FrameworkElement frameworkElem, PathIterator path, out object rootDataContext, out object thisDataContext)
        {
            rootDataContext = null;
            thisDataContext = null;
            object target = null;

            var parentWithDataContext = frameworkElem.FindParentWithDataContext();
            if (parentWithDataContext != null && parentWithDataContext.DataContext != null)
            {
                thisDataContext = parentWithDataContext.DataContext;
                rootDataContext = thisDataContext;
                target = thisDataContext;
            }

            if (path.MoveNext())
            {
                if (path.Current.Name == "root")
                {
                    var view = frameworkElem.FindParent<RapidView>();
                    rootDataContext = view.ViewModel;
                    target = rootDataContext;
                }
            }

            return target;
        }
    }
}