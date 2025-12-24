using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;

namespace FormsScratch
{
    public partial class Form1 : Form
    {
        PipelineGraph pipelineGraph = new();

        public Form1()
        {
            InitializeComponent();
        }

        private void OnGraphChanged(object? sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = pipelineGraph.SelectedItem;
            graphRenderPanel1.Invalidate();
        }

        private void CustomInit(object sender, EventArgs e)
        {
            ConvertBlock convertBlock = new();
            ResizeBlock resizeBlock = new();
            LoadBlock loadBlock = new();
            SaveBlock saveBlockA = new(), saveBlockB = new();

            pipelineGraph.AddBlock(loadBlock);
            pipelineGraph.AddBlock(resizeBlock);
            pipelineGraph.AddBlock(convertBlock);
            pipelineGraph.AddBlock(saveBlockA);
            pipelineGraph.AddBlock(saveBlockB);
            pipelineGraph.AddEdge(loadBlock, loadBlock.Outputs[0], resizeBlock, resizeBlock.Inputs[0]);
            pipelineGraph.AddEdge(loadBlock, loadBlock.Outputs[0], convertBlock, convertBlock.Inputs[0]);
            pipelineGraph.AddEdge(resizeBlock, resizeBlock.Outputs[0], saveBlockA, saveBlockA.Inputs[0]);
            pipelineGraph.AddEdge(convertBlock, convertBlock.Outputs[0], saveBlockB, saveBlockB.Inputs[0]);

            pipelineGraph.SelectedItem = loadBlock;

            propertyGrid1.SelectedObject = loadBlock;
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            graphRenderPanel1.RenderScale = Math.Clamp(graphRenderPanel1.RenderScale + 0.1f, 0.1f, 5f);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            graphRenderPanel1.RenderScale = Math.Clamp(graphRenderPanel1.RenderScale - 0.1f, 0.1f, 5f);
        }
    }
}
