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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using RapidPliant.App.ViewModels;
using Color = Microsoft.Msagl.Drawing.Color;

namespace RapidPliant.App.Controls
{
    /// <summary>
    /// Interaction logic for GraphView.xaml
    /// </summary>
    public partial class GraphView : IView<GraphViewModel>
    {
        public static DependencyProperty GraphPropery = DependencyProperty.Register("Graph", typeof(Graph), typeof(GraphView), new FrameworkPropertyMetadata(OnGraphPropertyChanged));

        private static void OnGraphPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graphView = d as GraphView;
            if (graphView != null)
            {
                graphView.UpdateGraph((Graph)e.NewValue);
            }
        }
        
        private GraphViewer GraphViewer;

        public GraphView()
        {
            InitializeComponent();

            GraphViewer = new GraphViewer();
            GraphViewer.BindToPanel(GraphViewerPanel);

            GraphViewer.ObjectUnderMouseCursorChanged += GraphObjectMouseHover;
        }

        public Graph Graph
        {
            get { return GetValue(GraphPropery) as Graph; }
            set { SetValue(GraphPropery, value); }
        }

        private void UpdateGraph(Graph graph)
        {
            GraphViewer.Graph = graph;
        }

        private void GraphObjectMouseHover(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            var node = GraphViewer.ObjectUnderMouseCursor as IViewerNode;
            if (node != null)
            {
                var drawingNode = (Node)node.DrawingObject;
                //statusTextBox.Text = drawingNode.Label.Text;
            }
            else
            {
                var edge = GraphViewer.ObjectUnderMouseCursor as IViewerEdge;
                if (edge != null)
                {
                    /*statusTextBox.Text = ((Edge) edge.DrawingObject).SourceNode.Label.Text + "->" +
                                         ((Edge) edge.DrawingObject).TargetNode.Label.Text;*/
                }
                else
                {
                    //statusTextBox.Text = "No object";
                }
            }
        }
    }
}
