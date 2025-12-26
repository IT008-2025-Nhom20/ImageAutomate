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
        public EditorView()
        {
            InitializeComponent();
            toolListBox.Items.AddRange([typeof(BrightnessBlock)]);
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
                graphRenderPanel1.Invalidate();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var block = new BrightnessBlock();
            graphRenderPanel1.AddPredecessor(block, new Socket("",""), new Socket("",""));
        }
    }
}
