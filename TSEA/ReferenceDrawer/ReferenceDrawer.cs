using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceDrawer
{
    using Microsoft.Msagl.Drawing;
    using Microsoft.Msagl.WpfGraphControl;

    public class ReferenceDrawer
    {
        GraphViewer graphViewer = new GraphViewer();

        /// <summary>
        /// Create a reference graph
        /// </summary>
        /// <returns></returns>
        public Graph CreateReferenceGraph()
        {
            Graph graph = new Graph();

            // TODO:

            return graph;
        }

        /// <summary>
        /// Display a reference graph.
        /// </summary>
        public void DisplayReferenceGraph()
        {
            Graph graph = this.CreateReferenceGraph();

            if (graph != null)
            {
                this.graphViewer.Graph = graph;
            }
        }
    }
}
