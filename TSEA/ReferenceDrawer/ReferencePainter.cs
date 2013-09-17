using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xin.SOMDiff.Drawing
{
    using Microsoft.Msagl.Drawing;
    using Microsoft.Msagl.WpfGraphControl;
    using System.Windows;
    using System.Windows.Controls;

    public class ReferencePainter
    {
        GraphViewer graphViewer;

        public ReferencePainter()
        {
            this.graphViewer = new GraphViewer();
        }

        /// <summary>
        /// Display a reference graph.
        /// </summary>
        public void DisplayReferenceGraph(Dictionary<string, List<string>> references)
        {
            //Graph graph = this.CreateGraphInstance(references);

            //if (graph != null)
            //{
            //    this.graphViewer.Graph = graph;
            //    this.Display(this.graphViewer);
            //}
        }

        /// <summary>
        /// Create a reference graph
        /// </summary>
        /// <returns></returns>
        public static Graph CreateGraphInstance(Dictionary<string, List<string>> references)
        {
            Graph graph = null;

            if (references != null && references.Count > 0)
            {
                graph = new Graph();

                foreach (string key in references.Keys)
                {
                    if (references[key] != null)
                    {
                        foreach (string value in references[key])
                        {
                            graph.AddEdge(key, value);
                        }
                    }
                    else
                    {
                        graph.AddNode(key);
                    }
                }
            }

            return graph;
        }

        private void Display(GraphViewer graphViewer)
        {
            Grid mainGrid = new Grid();
            Window appWindow = new Window
            {
                Title = "WpfApplicationSample",
                Content = mainGrid,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Normal
            };

            DockPanel graphViewerPanel = new DockPanel();
            graphViewerPanel.ClipToBounds = true;
            
            mainGrid.Children.Add(graphViewerPanel);
            graphViewer.BindToPanel(graphViewerPanel);
            
            appWindow.Show();
        }
    }
}
