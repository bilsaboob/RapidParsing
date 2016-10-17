﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RapidPliant.App.Controls
{
    /// <summary>
    /// Class that helps the creation of control and data templates by using delegates.
    /// </summary>
    public static class TemplateGenerator
    {
        private sealed class _TemplateGeneratorControl : ContentControl
        {
            internal static readonly DependencyProperty FactoryProperty = DependencyProperty.Register("Factory", typeof(Func<object>), typeof(_TemplateGeneratorControl), new PropertyMetadata(null, _FactoryChanged));

            private static void _FactoryChanged(DependencyObject instance, DependencyPropertyChangedEventArgs args)
            {
                var control = (_TemplateGeneratorControl)instance;
                var factory = (Func<object>)args.NewValue;
                var contentControl = factory();
                var bindableContentControl = contentControl as FrameworkElement;
                if (bindableContentControl != null)
                {
                    bindableContentControl.DataContext = control.DataContext;
                }
                control.Content = contentControl;
            }
        }

        /// <summary>
        /// Creates a data-template that uses the given delegate to create new instances.
        /// </summary>
        public static DataTemplate CreateDataTemplate(Func<object> factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var frameworkElementFactory = new FrameworkElementFactory(typeof(_TemplateGeneratorControl));
            frameworkElementFactory.SetValue(_TemplateGeneratorControl.FactoryProperty, factory);

            var dataTemplate = new DataTemplate(typeof(DependencyObject));
            dataTemplate.VisualTree = frameworkElementFactory;
            return dataTemplate;
        }

        /// <summary>
        /// Creates a control-template that uses the given delegate to create new instances.
        /// </summary>
        public static ControlTemplate CreateControlTemplate(Type controlType, Func<object> factory)
        {
            if (controlType == null)
                throw new ArgumentNullException("controlType");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var frameworkElementFactory = new FrameworkElementFactory(typeof(_TemplateGeneratorControl));
            frameworkElementFactory.SetValue(_TemplateGeneratorControl.FactoryProperty, factory);

            var controlTemplate = new ControlTemplate(controlType);
            controlTemplate.VisualTree = frameworkElementFactory;
            return controlTemplate;
        }
    }
}
