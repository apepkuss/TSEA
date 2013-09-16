using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceDrawer
{
    using Microsoft.Glee.Drawing;


    public class ReferenceDrawer
    {


        private static void CreateSourceNode(Node a)
        {
            a.Attr.Shape = Microsoft.Glee.Drawing.Shape.Box;
            a.Attr.XRad = 3;
            a.Attr.YRad = 3;
            a.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.Green;
            a.Attr.LineWidth = 10;
        }

        private void CreateTargetNode(Node a)
        {
            a.Attr.Shape = Microsoft.Glee.Drawing.Shape.DoubleCircle;
            a.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightGray;

            a.Attr.LabelMargin = -4;
        }
    }
}
