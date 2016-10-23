using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using RapidPliant.WPF.Controls;
using RapidPliant.WPF.Utils;

namespace RapidPliant.WPF.Mvx
{
    public class MvxContext
    {
        private RapidView _view;
        private ViewModel _viewModel;
        private List<ViewModel> _viewModels;
        private List<MvxContext> _childContexts;

        private bool _hasInitializedForView;
        private bool _hasInitializedForViewModel;


        public MvxContext(Type viewModelType, Type viewType)
        {
            ViewModelType = viewModelType;
            ViewType = viewType;
            _viewModels = new List<ViewModel>();
            _childContexts = new List<MvxContext>();
        }

        public Type ViewModelType { get; private set; }

        public ViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                var prevViewModel = _viewModel;
                
                //Set the new viewmodel
                _viewModel = value;

                if (prevViewModel != null && prevViewModel.IsLoaded)
                {
                    if (prevViewModel != null)
                    {
                        //Unload previous viewmodel
                        prevViewModel.Unload();
                    }
                }

                if(!_hasInitializedForViewModel)
                    return;

                InitializedForViewModel();
            }
        }

        private void InitializedForViewModel()
        {
            if (_viewModel != null)
            {
                //Bind the viewmodel context to this!
                _viewModel.BindContext(this);
            }

            if (_view != null)
            {
                _view.BindViewModel(_viewModel);
            }
        }

        public Type ViewType { get; private set; }

        public RapidView View
        {
            get { return _view; }
            set
            {
                _view = value;

                if (!_hasInitializedForView)
                    return;

                InitializeForView();
            }
        }

        private void InitializeForView()
        {
            //Bind the view to the viewmodel
            if (_viewModel != null)
            {
                _viewModel.BindView(_view);
            }

            if (_view != null)
            {
                //Bind the context to this!
                _view.BindContext(this);

                //Bind viewmodel if exists
                if (_viewModel != null)
                {
                    //Make sure the viewmodel is loaded!
                    if (!_viewModel.IsLoaded)
                    {
                        _viewModel.Load();
                    }
                }
            }
        }

        public MvxContext ParentContext { get; private set; }

        public IEnumerable<MvxContext> ChildContexts
        {
            get { return _childContexts; }
        }

        public void BindParentContext(MvxContext parentContext)
        {
            var prevParentContext = ParentContext;
            ParentContext = parentContext;

            if (prevParentContext != null)
            {
                prevParentContext.RemoveChildContext(this);
            }

            if (ParentContext != null)
            {
                ParentContext.AddChildContext(this);
            }
        }

        private void RemoveChildContext(MvxContext mvxContext)
        {
            _childContexts.Remove(mvxContext);
        }

        private void AddChildContext(MvxContext mvxContext)
        {
            if(!_childContexts.Contains(mvxContext))
                _childContexts.Add(mvxContext);
        }

        public ViewModel GetOrCreateViewModel()
        {
            return GetOrCreateViewModel(ViewModelType);
        }

        public ViewModel GetOrCreateViewModel(Type viewModelType, bool create = true)
        {
            ViewModel viewModel = null;

            var matchingViewModels = FindMatchingViewModels(viewModelType);
            if (matchingViewModels != null && matchingViewModels.Count > 0)
            {
                //We have a viewmodel 
                viewModel = ResolveBestViewModel(viewModelType, matchingViewModels);
                return viewModel;
            }
            
            //Try getting from the parent!
            if (ParentContext != null)
            {
                viewModel = ParentContext.GetOrCreateViewModel(viewModelType, false);
            }

            if (viewModel == null && create)
            {
                //No existing viewmodels to get - create a new one!
                viewModel = CreateViewModel(viewModelType);
                if (viewModel != null)
                {
                    AddViewModel(viewModel);
                }
            }

            return viewModel;
        }

        private void AddViewModel(ViewModel viewModel)
        {
            _viewModels.Add(viewModel);
        }

        private ViewModel CreateViewModel(Type viewModelType)
        {
            var viewModel = Activator.CreateInstance(viewModelType);
            return viewModel as ViewModel;
        }

        private ViewModel ResolveBestViewModel(Type viewModelType, List<ViewModel> viewModels)
        {
            var viewModelsByRelevance = new SortedList<int, ViewModel>();

            foreach (var viewModel in viewModels)
            {
                var type = viewModel.GetType();
                if (type == viewModelType)
                {
                    viewModelsByRelevance.Add(0, viewModel);
                }
                else
                {
                    var typeDistance = type.GetTypeDistanceTo(viewModelType);
                    if (!viewModelsByRelevance.ContainsKey(typeDistance))
                    {
                        viewModelsByRelevance.Add(typeDistance, viewModel);
                    }
                }
            }
            
            //Get the first viewmodel - thats the one that matches best!
            return viewModelsByRelevance.Values.FirstOrDefault();
        }

        private List<ViewModel> FindMatchingViewModels(Type viewModelType)
        {
            if (_viewModels.Count == 0)
                return null;

            var matchingViewModels = new List<ViewModel>();

            foreach (var viewModel in _viewModels)
            {
                var type = viewModel.GetType();
                if (viewModelType.IsAssignableFrom(type))
                {
                    matchingViewModels.Add(viewModel);
                }
            }

            return matchingViewModels;
        }

        public void Initialize()
        {
            InitializedForViewModel();
            InitializeForView();
        }
    }

    public static class Mvx
    {
        public static void Init()
        {
        }

        public static void LoadView(Control viewControl)
        {
            var view = viewControl as IView;
            if(view == null)
                return;

            //Build the mvx contexts!
            var rootContext = BuildMvxContextRecursive(viewControl, null);

            ResolveViewModelsRecursive(rootContext);

            InitializeContextsRecursive(rootContext);
        }

        private static void InitializeContextsRecursive(MvxContext context)
        {
            if (context == null)
                return;

            context.Initialize();
            
            foreach (var childContext in context.ChildContexts)
            {
                InitializeContextsRecursive(childContext);
            }
        }

        private static MvxContext BuildMvxContextRecursive(DependencyObject viewControl, MvxContext parentContext)
        {
            if(viewControl == null)
                return parentContext;

            var returnContext = parentContext;

            //Build the context for the current view
            MvxContext context = null;
            var view = viewControl as IView;
            if (view != null)
            {
                context = view.Context;

                if (context == null)
                {
                    context = CreateContextForView(view);
                    context.BindParentContext(parentContext);
                    view.BindContext(context);
                    context.View = (RapidView)view;
                }

                if (returnContext == null)
                {
                    returnContext = context;
                }
            }

            //Build contexts for the children!
            var children = viewControl.GetAllChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    var childParentContext = context;
                    if (childParentContext == null)
                    {
                        childParentContext = parentContext;
                    }

                    var childContext = BuildMvxContextRecursive(child, childParentContext);
                    if (returnContext == null)
                    {
                        returnContext = childContext;
                    }
                }
            }

            return returnContext;
        }

        private static void ResolveViewModelsRecursive(MvxContext context)
        {
            if(context == null)
                return;

            var viewModel = context.ViewModel;
            if (viewModel == null)
            {
                viewModel = context.GetOrCreateViewModel();
                context.ViewModel = viewModel;
            }

            if (viewModel != null)
            {
                viewModel.LoadViewModels();
                ResolveViewModelMembers(context, viewModel);
            }
            
            foreach (var childContext in context.ChildContexts)
            {
                ResolveViewModelsRecursive(childContext);
            }
        }

        private static void ResolveViewModelMembers(MvxContext context, ViewModel viewModel)
        {
            var properties = viewModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var viewModelProperties = properties.Where(p => typeof(ViewModel).IsAssignableFrom(p.PropertyType)).ToList();
            foreach (var viewModelProperty in viewModelProperties)
            {
                var val = viewModelProperty.GetValue(viewModel);
                if (val == null)
                {
                    val = context.GetOrCreateViewModel(viewModelProperty.PropertyType);
                    if (val != null)
                    {
                        viewModelProperty.SetValue(viewModel, val);
                    }
                }
            }
        }

        private static MvxContext CreateContextForView(IView view)
        {
            var viewModelType = view.ViewModelType;
            var viewType = view.GetType();

            var context = new MvxContext(viewModelType, viewType);
            return context;
        }

        public static Type GetViewModelTypeForView(Type viewType)
        {
            var viewModelType = viewType.GetInterfaces().Where(t => {
                if (!t.IsGenericType)
                    return false;
                if (t.GetGenericTypeDefinition() != typeof(IView<>))
                    return false;
                if (t.GenericTypeArguments.Length == 0)
                    return false;
                if (!typeof(ViewModel).IsAssignableFrom(t.GenericTypeArguments[0]))
                    return false;
                return true;
            }).Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
            return viewModelType;
        }

        public static ViewModel CreateViewModelForView(Type viewType)
        {
            var viewModelType = GetViewModelTypeForView(viewType);

            if (viewModelType == null)
                return null;
            
            return GetOrCreateViewModel(viewModelType);
        }

        public static ViewModel GetOrCreateViewModel(Type viewModelType)
        {
            var viewModel = Activator.CreateInstance(viewModelType);
            if (viewModel == null)
                return null;

            return (ViewModel) viewModel;
        }
    }
}
