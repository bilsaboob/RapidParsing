using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace RapidPliant.WPF.Binding
{
    [MarkupExtensionReturnType(typeof(object))]
    public abstract class BindingMarkupExtensionBase : MarkupExtension
    {
        private System.Windows.Data.Binding _binding = new System.Windows.Data.Binding();

        public BindingMarkupExtensionBase()
        {
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public System.Windows.Data.Binding Binding
        {
            get { return _binding; }
            set { _binding = value; }
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public object AsyncState
        {
            get { return _binding.AsyncState; }
            set { _binding.AsyncState = value; }
        }
        
        [DefaultValue(false)]
        public bool BindsDirectlyToSource
        {
            get { return _binding.BindsDirectlyToSource; }
            set { _binding.BindsDirectlyToSource = value; }
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public IValueConverter ActualConverter { get; set; }
        
        [DefaultValue(null)]
        public IValueConverter Converter
        {
            get { return _binding.Converter; }
            set
            {
                var rapidValueProviderConverter = value as RapidBindingPropertyValueProviderConverter;
                if (rapidValueProviderConverter == null)
                {
                    ActualConverter = value;
                }

                _binding.Converter = value;
            }
        }
        
        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter)), DefaultValue(null)]
        public CultureInfo ConverterCulture
        {
            get { return _binding.ConverterCulture; }
            set { _binding.ConverterCulture = value; }
        }
        
        [DefaultValue(null)]
        public object ConverterParameter
        {
            get { return _binding.ConverterParameter; }
            set { _binding.ConverterParameter = value; }
        }
        
        [DefaultValue(null)]
        public string ElementName
        {
            get { return _binding.ElementName; }
            set { _binding.ElementName = value; }
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public object FallbackValue
        {
            get { return _binding.FallbackValue; }
            set { _binding.FallbackValue = value; }
        }
        
        [DefaultValue(false)]
        public bool IsAsync
        {
            get { return _binding.IsAsync; }
            set { _binding.IsAsync = value; }
        }
        
        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode
        {
            get { return _binding.Mode; }
            set { _binding.Mode = value; }
        }
        
        [DefaultValue(false)]
        public bool NotifyOnSourceUpdated
        {
            get { return _binding.NotifyOnSourceUpdated; }
            set { _binding.NotifyOnSourceUpdated = value; }
        }
        
        [DefaultValue(false)]
        public bool NotifyOnTargetUpdated
        {
            get { return _binding.NotifyOnTargetUpdated; }
            set { _binding.NotifyOnTargetUpdated = value; }
        }
        
        [DefaultValue(false)]
        public bool NotifyOnValidationError
        {
            get { return _binding.NotifyOnValidationError; }
            set { _binding.NotifyOnValidationError = value; }
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public PropertyPath ActualPath { get; set; }
        
        [DefaultValue(null)]
        public PropertyPath Path
        {
            get { return _binding.Path; }
            set
            {
                if (value != null && value.Path != RapidBindingPropertyValueProviderConverter.FakeValuePath)
                {
                    ActualPath = value;
                }
                _binding.Path = value;
            }
        }
        
        [DefaultValue(null)]
        public RelativeSource RelativeSource
        {
            get { return _binding.RelativeSource; }
            set { _binding.RelativeSource = value; }
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public object Source
        {
            get { return _binding.Source; }
            set { _binding.Source = value; }
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
        {
            get { return _binding.UpdateSourceExceptionFilter; }
            set { _binding.UpdateSourceExceptionFilter = value; }
        }
        
        [DefaultValue(UpdateSourceTrigger.Default)]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return _binding.UpdateSourceTrigger; }
            set { _binding.UpdateSourceTrigger = value; }
        }
        
        [DefaultValue(false)]
        public bool ValidatesOnDataErrors
        {
            get { return _binding.ValidatesOnDataErrors; }
            set { _binding.ValidatesOnDataErrors = value; }
        }
        
        [DefaultValue(false)]
        public bool ValidatesOnExceptions
        {
            get { return _binding.ValidatesOnExceptions; }
            set { _binding.ValidatesOnExceptions = value; }
        }
        
        [DefaultValue(null)]
        public string XPath
        {
            get { return _binding.XPath; }
            set { _binding.XPath = value; }
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public Collection<ValidationRule> ValidationRules
        {
            get { return _binding.ValidationRules; }
        }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override object ProvideValue(IServiceProvider provider)
        {
            return _binding.ProvideValue(provider);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual bool TryGetTargetItems<TTarget, TTargetProp>(IServiceProvider provider, out TTarget target, out TTargetProp targetProp)
            where TTarget : class
            where TTargetProp : class
        {
            target = default(TTarget);
            targetProp = default(TTargetProp);

            if (provider == null)
                return false;

            var pvt = provider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (pvt == null)
                return false;

            target = pvt.TargetObject as TTarget;
            targetProp = pvt.TargetProperty as TTargetProp;

            if (target == null || targetProp == null)
                return false;

            return true;
        }
    }
}
