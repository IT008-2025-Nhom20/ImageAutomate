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
            pipelineGraph.Connect(loadBlock, loadBlock.Outputs[0], resizeBlock, resizeBlock.Inputs[0]);
            pipelineGraph.Connect(loadBlock, loadBlock.Outputs[0], convertBlock, convertBlock.Inputs[0]);
            pipelineGraph.Connect(resizeBlock, resizeBlock.Outputs[0], saveBlockA, saveBlockA.Inputs[0]);
            pipelineGraph.Connect(convertBlock, convertBlock.Outputs[0], saveBlockB, saveBlockB.Inputs[0]);

            pipelineGraph.Center = loadBlock;
            graphRenderPanel1.SetCenterBlock(loadBlock);

            graphRenderPanel1.Graph = pipelineGraph;
            propertyGrid1.SelectedObject = loadBlock;
            graphRenderPanel1.Refresh();
        }
    }
}
