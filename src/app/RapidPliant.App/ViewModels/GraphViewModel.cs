using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;
using RapidPliant.Common.Rule;
using RapidPliant.Lexing.Graph;
using RapidPliant.WPF.Mvx;

namespace RapidPliant.App.ViewModels
{
    public class GraphViewModel : ViewModel
    {
        public GraphViewModel()
        {
        }

        public Graph Graph
        {
            get { return get(()=>Graph); }
            set { set(()=>Graph, value); }
        }
        
        protected override void LoadData()
        {
        }
    }
}