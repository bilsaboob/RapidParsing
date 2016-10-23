using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using RapidPliant.WPF.Controls;
using RapidPliant.WPF.Mvx;

namespace RapidPliant.WPF.Binding
{
    public enum PathType
    {
        None,
        RelativeToRoot,
        RelativeToTarget
    }

    public class RapidBindingPropertyValueProviderConverter : IValueConverter, INotifyPropertyChanged
    {
        public const string FakeValuePath = "RapidBindingPropertyValueProviderConverterValuePath";

        private FrameworkElement _initializeRootElement;
        private FrameworkElement _initializedTargetElement;

        public RapidBindingPropertyDelegate Delegate { get; set; }
        public BindTo RapidBinding { get { return Delegate.Binding; } }
        private FrameworkElement FrameworkElement { get { return Delegate.FrameworkElement; } }
        public System.Windows.Data.Binding Binding { get { return RapidBinding.Binding; } }
        public BindingExpression BindingExpression { get; set; }

        private IValueConverter ActualConverter { get; set; }
        public PropertyPath ActualPath { get; set; }

        public PathType PathType { get; set; }
        
        public RapidBindingPropertyValueProviderConverter(RapidBindingPropertyDelegate rapidBindingDelegate)
        {
            Delegate = rapidBindingDelegate;
        }

        public object RapidBindingPropertyValueProviderConverterValuePath { get; set; }

        public object RootViewModel { get; set; }

        public object TargetViewModel { get; set; }

        public void SetupBinding()
        {
            RapidBinding.ActualPath = RapidBinding.Path;
            RapidBinding.ActualConverter = RapidBinding.Converter;

            ActualConverter = RapidBinding.ActualConverter;
            ActualPath = RapidBinding.ActualPath;

            //Override with custon source and handling
            RapidBinding.Converter = this;
            RapidBinding.Path = new PropertyPath(FakeValuePath);
            RapidBinding.Source = this;
        }

        private void Initialize()
        {
            if(FrameworkElement == null)
                return;

            InitializePathConfig();

            if (_initializedTargetElement != null && FrameworkElement != _initializedTargetElement)
            {
                //Unhook anything of the initialized framework element
                _initializedTargetElement = null;
                _initializeRootElement = null;
            }

            if (FrameworkElement != null && _initializedTargetElement == null)
            {
                _initializedTargetElement = FrameworkElement;
                
                UpdateTragetViewModel(_initializedTargetElement.DataContext);
                _initializedTargetElement.DataContextChanged += OnTargetElementDataContextChanged;

                _initializeRootElement = _initializedTargetElement.FindParent<RapidView>();
                if (_initializeRootElement != null)
                {
                    UpdateTragetViewModel(_initializeRootElement.DataContext);
                    _initializeRootElement.DataContextChanged += OnRootElementDataContextChanged;
                }
            }
        }

        private void InitializePathConfig()
        {
            if (PathType == PathType.None)
            {
                PathType = PathType.RelativeToTarget;

                var path = new PathIterator(ActualPath.Path);
                if (path.MoveNext() && path.Current.Name == "root")
                {
                    PathType = PathType.RelativeToRoot;
                }
            }
        }

        private void EnsureInitialized()
        {
            Initialize();
        }

        private void OnTargetElementDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateTragetViewModel(e.NewValue);
            OnPropertyChanged("RapidBindingPropertyValueProviderConverterValuePath");
        }

        private void OnRootElementDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateRootViewModel(e.NewValue);
            OnPropertyChanged("RapidBindingPropertyValueProviderConverterValuePath");
        }

        public object ProvideValue(IServiceProvider provider)
        {
            EnsureInitialized();
            BindingExpression = Binding.ProvideValue(provider) as BindingExpression;
            return BindingExpression;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnsureInitialized();
            
            var pathIter = new PathIterator(ActualPath.Path);
            var targetViewModel = GetTargetViewModel(pathIter);
            if (targetViewModel != null)
            {
                value = GetPathValue(pathIter);
            }
            
            return value;
        }

        private object GetPathValue(PathIterator path)
        {
            var prop = new PropertyWithPath();
            return prop.GetPropertyValue(RootViewModel, TargetViewModel, TargetViewModel, path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnsureInitialized();

            var pathIter = new PathIterator(ActualPath.Path);
            var targetViewModel = GetTargetViewModel(pathIter);
            if (targetViewModel != null)
            {
                value = SetPathValue(pathIter, value);
            }

            return value;
        }

        private object SetPathValue(PathIterator path, object value)
        {
            var prop = new PropertyWithPath();
            return prop.SetPropertyValue(RootViewModel, TargetViewModel, TargetViewModel, path, value);
        }

        private object GetTargetViewModel(PathIterator path)
        {
            if (path.MoveNext())
            {
                if (path.Current.Name == "root")
                {
                    return RootViewModel;
                }
            }

            return TargetViewModel;
        }

        private INotifyPropertyChanged _initializedTargetViewModel;

        private void UpdateTragetViewModel(object targetViewModel)
        {
            UpdateTragetViewModel_(targetViewModel);

            targetViewModel = TryFetchEvaluatedPathTargetViewModel();

            if (targetViewModel != null)
            {
                UpdateTragetViewModel_(targetViewModel);
            }
        }

        private object TryFetchEvaluatedPathTargetViewModel()
        {
            if(RootViewModel == null)
                return null;

            var path = new PathIterator(ActualPath.Path);
            if (path.Parts.Length > 1)
            {
                path = new PathIterator(path.Parts.Take(path.Parts.Length-1));
                var targetViewModel = new PropertyWithPath().GetPropertyValue(RootViewModel, RootViewModel, RootViewModel, path);
                return targetViewModel;
            }

            return null;
        }

        private void UpdateTragetViewModel_(object targetViewModel)
        {
            if (_initializedTargetViewModel != null && _initializedTargetViewModel != targetViewModel)
            {
                _initializedTargetViewModel.PropertyChanged -= OnTargetViewModelPropertyChanged;
            }

            TargetViewModel = targetViewModel;

            if (_initializedTargetViewModel != targetViewModel)
            {
                _initializedTargetViewModel = TargetViewModel as INotifyPropertyChanged;
                if (_initializedTargetViewModel != null)
                {
                    _initializedTargetViewModel.PropertyChanged += OnTargetViewModelPropertyChanged;
                }
            }
        }

        private INotifyPropertyChanged _initializedRootViewModel;

        private void UpdateRootViewModel(object rootViewModel)
        {
            UpdateRootViewModel_(rootViewModel);
        }
        private void UpdateRootViewModel_(object rootViewModel)
        {
            if (_initializedRootViewModel != null && _initializedRootViewModel != rootViewModel)
            {
                _initializedRootViewModel.PropertyChanged -= OnRootViewModelPropertyChanged;
            }

            RootViewModel = rootViewModel;

            if (_initializedRootViewModel != rootViewModel)
            {
                _initializedRootViewModel = RootViewModel as INotifyPropertyChanged;
                if (_initializedRootViewModel != null)
                {
                    _initializedRootViewModel.PropertyChanged += OnRootViewModelPropertyChanged;
                }
            }
        }

        private void OnTargetViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("RapidBindingPropertyValueProviderConverterValuePath");
        }
        
        private void OnRootViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("RapidBindingPropertyValueProviderConverterValuePath");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}