using Accessibility;
using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageAutomate.Views
{
    public partial class EditorView : UserControl
    {
        private PipelineGraph graph = new PipelineGraph();
        public EditorView()
        {
            InitializeComponent();

            var workspace = new Workspace(graph);
            graphRenderPanel1.Workspace = workspace;

            toolListBox.Items.AddRange([typeof(BrightnessBlock)]);
            toolListBox.Items.AddRange([typeof(ContrastBlock)]);
            toolListBox.Items.AddRange([typeof(ConvertBlock)]);
            toolListBox.Items.AddRange([typeof(CropBlock)]);
            toolListBox.Items.AddRange([typeof(FlipBlock)]);
            toolListBox.Items.AddRange([typeof(GaussianBlurBlock)]);
            toolListBox.Items.AddRange([typeof(GrayscaleBlock)]);
            toolListBox.Items.AddRange([typeof(HueBlock)]);
            toolListBox.Items.AddRange([typeof(LoadBlock)]);
            toolListBox.Items.AddRange([typeof(PixelateBlock)]);
            toolListBox.Items.AddRange([typeof(ResizeBlock)]);
            toolListBox.Items.AddRange([typeof(SaturationBlock)]);
            toolListBox.Items.AddRange([typeof(SaveBlock)]);
            toolListBox.Items.AddRange([typeof(SharpenBlock)]);
            toolListBox.Items.AddRange([typeof(VignetteBlock)]);
        }

        private void toolListBox_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox lb = toolListBox;
            Point pt = new Point(e.X, e.Y);
            int index = lb.IndexFromPoint(pt);

            if (index >= 0)
            {
                lb.DoDragDrop(lb.Items[index], DragDropEffects.Copy);
            }
        }

        private void graphRenderPanel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Type)))
                e.Effect = DragDropEffects.Copy;
        }

        private void graphRenderPanel1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Type)))
            {
                var type = (Type)e.Data.GetData(typeof(Type));
                var block = (IBlock)Activator.CreateInstance(type);

                Point clientPoint = graphRenderPanel1.PointToClient(new Point(e.X, e.Y));

                block.X = clientPoint.X;
                block.Y = clientPoint.Y;

                graphRenderPanel1.Graph.AddBlock(block);
                propertyGrid1.SelectedObject = block;
                graphRenderPanel1.Invalidate();
            }

        }

        private void graphRenderPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (graphRenderPanel1.Graph != null)
            {
                propertyGrid1.SelectedObject = graphRenderPanel1.Graph.SelectedItem;
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            if (graphRenderPanel1.Graph != null)
            {
                graphRenderPanel1.Graph.Clear();
                graphRenderPanel1.Invalidate();
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (graphRenderPanel1.Graph != null)
            {
                graphRenderPanel1.DeleteSelectedItem();
                graphRenderPanel1.Invalidate();
            }
        }
    }
}
