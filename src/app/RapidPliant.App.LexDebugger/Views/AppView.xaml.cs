using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RapidPliant.App.Binding;
using RapidPliant.App.Controls;
using RapidPliant.App.LexDebugger.ViewModels;

namespace RapidPliant.App.LexDebugger.Views
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class AppView : IView<AppViewModel>
    {
        public DependencyPropertyListener _listener;

        public AppView()
        {
            InitializeComponent();
        }
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ((AppViewModel) ViewModel).AddLexPattern();
        }
    }


}
