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

            toolListBox.Items.AddRange("BrightnessBlock");
            toolListBox.Items.AddRange("ContrastBlock");
            toolListBox.Items.AddRange("ConvertBlock");
            toolListBox.Items.AddRange("CropBlock");
            toolListBox.Items.AddRange("FlipBlock");
            toolListBox.Items.AddRange("GaussianBlurBlock");
            toolListBox.Items.AddRange("GrayscaleBlock");
            toolListBox.Items.AddRange("HueBlock");
            toolListBox.Items.AddRange("LoadBlock");
            toolListBox.Items.AddRange("PixelateBlock");
            toolListBox.Items.AddRange("ResizeBlock");
            toolListBox.Items.AddRange("SaturationBlock");
            toolListBox.Items.AddRange("SaveBlock");
            toolListBox.Items.AddRange("SharpenBlock");
            toolListBox.Items.AddRange("VignetteBlock");
        }

        private void toolListBox_MouseDown(object sender, MouseEventArgs e)
        {
            ListBox lb = toolListBox;
            Point pt = new Point(e.X, e.Y);
            int index = lb.IndexFromPoint(pt);

            if (index >= 0)
            {
                lb.DoDragDrop(MapType(lb.Items[index].ToString()), DragDropEffects.Copy);
            }
        }

        private void graphRenderPanel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(String)))
                e.Effect = DragDropEffects.Copy;
        }

        private void graphRenderPanel1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(String)))
            {
                var type = (String)e.Data.GetData(typeof(String));
                
                var blocktype = MapType(type);

                var block = (IBlock)Activator.CreateInstance(blocktype);

                Point clientPoint = graphRenderPanel1.PointToClient(new Point(e.X, e.Y));

                block.X = clientPoint.X;
                block.Y = clientPoint.Y;

                graphRenderPanel1.Graph.AddBlock(block);
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

        private Type MapType(String str)
        {
            switch (str)
            {
                case "BrightnessBlock":
                    return typeof(BrightnessBlock);
                case "ContrastBlock":
                    return typeof(ContrastBlock);
                case "ConvertBlock":
                    return typeof(ConvertBlock);
                case "CropBlock":
                    return typeof(CropBlock);
                case "FlipBlock":
                    return typeof(FlipBlock);
                case "GaussianBlurBlock":
                    return typeof(GaussianBlurBlock);
                case "GrayscaleBlock":
                    return typeof(GrayscaleBlock);
                case "HueBlock":
                    return typeof(HueBlock);
                case "LoadBlock":
                    return typeof(LoadBlock);
                case "PixelateBlock":
                    return typeof(PixelateBlock);
                case "ResizeBlock":
                    return typeof(ResizeBlock);
                case "SaturationBlock":
                    return typeof(SaturationBlock);
                case "SaveBlock":
                    return typeof(SaveBlock);
                case "SharpenBlock":
                    return typeof(SharpenBlock);
                case "VignetteBlock":
                    return typeof(VignetteBlock);
                default:
                    throw new ArgumentException($"{str} does not exist");
            }
        }
    }
}
