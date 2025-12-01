using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ConvertBlockPoC
{
    public partial class MainForm : Form
    {
        PipelineGraph graph = new();
        ConvertBlock centerConvertBlock = new ConvertBlock();
        public MainForm()
        {
            InitializeComponent();

            graph.AddNode(centerConvertBlock);
            graph.CenterNode = graph.GetNode(centerConvertBlock);

            graphRenderPanel1.Graph = graph;
            propertyGrid1.SelectedObject = centerConvertBlock;
        }

        private void butAddSuccessor_Click(object sender, EventArgs e)
        {
            ConvertBlock newConvert = new ConvertBlock();
            graphRenderPanel1.AddSuccessor(newConvert);
            graphRenderPanel1.Invalidate();
        }

        private void butAddPredecessor_Click(object sender, EventArgs e)
        {
            ConvertBlock newConvert = new ConvertBlock();
            graphRenderPanel1.AddPredecessor(newConvert);
            graphRenderPanel1.Invalidate();
        }

        private void butClear_Click(object sender, EventArgs e)
        {
            graph.Clear();
            ConvertBlock newConvert = new ConvertBlock();
            graphRenderPanel1.Initialize(newConvert);
            graphRenderPanel1.Invalidate();
        }

        private void butSelectRandom_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
