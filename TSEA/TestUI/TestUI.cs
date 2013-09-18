using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using Microsoft.Win32;
using Color = Microsoft.Msagl.Drawing.Color;
using ModifierKeys = System.Windows.Input.ModifierKeys;
using Size = System.Windows.Size;

using Xin.SOMDiff;
using Xin.SOMDiff.Drawing;

namespace TestUI
{
    class TestUI : Application
    {


        public static readonly RoutedUICommand LoadSampleGraphCommand = new RoutedUICommand("Open File...", "OpenFileCommand",
                                                                                     typeof(TestUI));
        public static readonly RoutedUICommand HomeViewCommand = new RoutedUICommand("Home view...", "HomeViewCommand",
                                                                                     typeof(TestUI));



        Window appWindow;
        Grid mainGrid = new Grid();
        DockPanel graphViewerPanel = new DockPanel();
        ToolBar toolBar = new ToolBar();
        GraphViewer graphViewer = new GraphViewer();
        TextBox statusTextBox;


        protected override void OnStartup(StartupEventArgs e)
        {

            appWindow = new Window
            {
                Title = "WpfApplicationSample",
                Content = mainGrid,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Normal
            };

            SetupToolbar();
            graphViewerPanel.ClipToBounds = true;
            mainGrid.Children.Add(toolBar);
            toolBar.VerticalAlignment = VerticalAlignment.Top;
            graphViewer.ObjectUnderMouseCursorChanged += graphViewer_ObjectUnderMouseCursorChanged;

            mainGrid.Children.Add(graphViewerPanel);
            graphViewer.BindToPanel(graphViewerPanel);

            SetStatusBar();
            graphViewer.MouseDown += WpfApplicationSample_MouseDown;


            //graphViewer.MainPanel.MouseLeftButtonUp += TestApi;
            appWindow.Show();
        }

        void WpfApplicationSample_MouseDown(object sender, MsaglMouseEventArgs e)
        {
            statusTextBox.Text = "there was a click...";
        }

        void SetStatusBar()
        {
            var statusBar = new StatusBar();
            statusTextBox = new TextBox { Text = "No object" };
            statusBar.Items.Add(statusTextBox);
            mainGrid.Children.Add(statusBar);
            statusBar.VerticalAlignment = VerticalAlignment.Bottom;
        }

        void graphViewer_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            var node = graphViewer.ObjectUnderMouseCursor as IViewerNode;
            if (node != null)
            {
                var drawingNode = (Node)node.DrawingObject;
                statusTextBox.Text = drawingNode.Label.Text;
            }
            else
            {
                var edge = graphViewer.ObjectUnderMouseCursor as IViewerEdge;
                if (edge != null)
                    statusTextBox.Text = ((Edge)edge.DrawingObject).SourceNode.Label.Text + "->" +
                                         ((Edge)edge.DrawingObject).TargetNode.Label.Text;
                else
                    statusTextBox.Text = "No object";
            }
        }




        void SetupToolbar()
        {
            SetupCommands();
            DockPanel.SetDock(toolBar, Dock.Top);
            SetMainMenu();
            //edgeRangeSlider = CreateRangeSlider();
            // toolBar.Items.Add(edgeRangeSlider.Visual);
        }


        void SetupCommands()
        {
            appWindow.CommandBindings.Add(new CommandBinding(LoadSampleGraphCommand, CreateAndLayoutAndDisplayGraph));
            appWindow.CommandBindings.Add(new CommandBinding(HomeViewCommand, (a, b) => graphViewer.SetInitialTransform()));
            appWindow.InputBindings.Add(new InputBinding(LoadSampleGraphCommand, new KeyGesture(Key.L, ModifierKeys.Control)));
            appWindow.InputBindings.Add(new InputBinding(HomeViewCommand, new KeyGesture(Key.H, ModifierKeys.Control)));

        }


        void ScaleNodeUpTest(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void SetMainMenu()
        {
            var mainMenu = new Menu { IsMainMenu = true };
            toolBar.Items.Add(mainMenu);
            SetFileMenu(mainMenu);
            SetViewMenu(mainMenu);
        }

        void SetViewMenu(Menu mainMenu)
        {
            var viewMenu = new MenuItem { Header = "_View" };
            var viewMenuItem = new MenuItem { Header = "_Home", Command = HomeViewCommand };
            viewMenu.Items.Add(viewMenuItem);
            mainMenu.Items.Add(viewMenu);
        }

        void SetFileMenu(Menu mainMenu)
        {
            var fileMenu = new MenuItem { Header = "_File" };
            var openFileMenuItem = new MenuItem { Header = "_Load Sample Graph", Command = LoadSampleGraphCommand };
            fileMenu.Items.Add(openFileMenuItem);
            mainMenu.Items.Add(fileMenu);
        }



        void CreateAndLayoutAndDisplayGraph(object sender, ExecutedRoutedEventArgs ex)
        {
            try
            {
                Graph graph = new Graph();
                //graph.LayoutAlgorithmSettings=new MdsLayoutSettings();

                string sourcepath = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Request";

                SOMDiff sdiff = new SOMDiff();
                Dictionary<string, List<string>> dependency = sdiff.ParseSchemaDependency(sourcepath);
                graph = ReferencePainter.CreateGraphInstance(dependency);

                //layout the graph and draw it
                graphViewer.Graph = graph;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Load Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        [STAThread]
        static void Main(string[] args)
        {
            new TestUI { Args = args }.Run();
        }

        public string[] Args { get; set; }
    }
}
