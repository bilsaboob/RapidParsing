using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RapidPliant.App.ViewModels;

namespace RapidPliant.App.Controls
{
    public interface IView
    {
        Type ViewModelType { get; }
        ViewModel ViewModel { get; }
        
        MvxContext Context { get;  }
        void BindContext(MvxContext context);
    }

    public interface IView<TViewModel> : IView
    {
    }

    public class RapidView : UserControl, IView
    {
        private List<DependencyObject> _relatedObjects;

        public RapidView()
        {
            _relatedObjects = new List<DependencyObject>();
        }

        public void AddRelatedObject(DependencyObject depObj)
        {
            if(!_relatedObjects.Contains(depObj))
                _relatedObjects.Add(depObj);
        }

        private Type _viewModelType;
        public virtual Type ViewModelType
        {
            get
            {
                if (_viewModelType == null)
                {
                    _viewModelType = Mvx.GetViewModelTypeForView(GetType());
                }
                return _viewModelType;
            }
        }
        
        public bool HasViewModel { get; protected set; }
        public ViewModel ViewModel { get; protected set; }
        
        public MvxContext Context { get; private set; }

        public void BindContext(MvxContext context)
        {
            if(Context == context)
                return;

            Context = context;
            ViewModel viewModel = null;
            
            if (Context != null)
            {
                viewModel = Context.ViewModel;
            }

            BindViewModel(viewModel);
        }

        public void BindViewModel(ViewModel viewModel)
        {
            ViewModel = viewModel;
            HasViewModel = viewModel != null;

            //Set the datacontext!
            DataContext = ViewModel;
        }
    }
}
