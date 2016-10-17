using Microsoft.Msagl.Drawing;

namespace RapidPliant.App.ViewModels
{
    public class GraphViewModel : ViewModel
    {
        public GraphViewModel()
        {
        }

        public Graph Graph { get { return get(()=>Graph); } set { set(()=>Graph, value); } }
        
        protected override void LoadData()
        {
        }
    }
}