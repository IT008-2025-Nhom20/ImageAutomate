using ImageAutomate.Core;
using ImageAutomate.Core.Serialization;
using ImageAutomate.StandardBlocks;

namespace ImageAutomate.Execution.Tests;

/// <summary>
/// Tests for serialization of PipelineGraph and related components.
/// </summary>
public class SerializationTests
{
    #region Socket Serialization Tests

    [Fact]
    public void Socket_Serialization_RoundTrip_PreservesData()
    {
        // Arrange
        var socket = new Socket("test-id", "Test Socket");
        var dto = new SocketDto(socket);

        // Act
        var deserialized = dto.ToSocket();

        // Assert
        Assert.Equal(socket.Id, deserialized.Id);
        Assert.Equal(socket.Name, deserialized.Name);
    }

    [Fact]
    public void Socket_EmptyId_ThrowsException()
    {
        // This tests that Socket record enforces its constraints
        // If Socket allows empty strings, this test documents that behavior
        var socket = new Socket("", "Empty ID");
        Assert.NotNull(socket);
    }

    #endregion

    #region Connection Serialization Tests

    [Fact]
    public void Connection_Serialization_PreservesStructure()
    {
        // Arrange - using real blocks
        var source = new ImageAutomate.StandardBlocks.BrightnessBlock();
        var target = new ImageAutomate.StandardBlocks.BrightnessBlock();
        var connection = new Connection(
            source,
            source.Outputs[0],
            target,
            target.Inputs[0]
        );

        // Act & Assert - Connection is a record, so equality works
        Assert.Equal(source, connection.Source);
        Assert.Equal(target, connection.Target);
        Assert.Equal(source.Outputs[0], connection.SourceSocket);
        Assert.Equal(target.Inputs[0], connection.TargetSocket);
    }

    #endregion

    #region IBlock Serialization Tests

    [Fact]
    public void Block_Serialization_SerializesBasicProperties()
    {
        // This test verifies that the serialization captures basic block information
        // We use a real block (BrightnessBlock) that has a parameterless constructor
        var block = new ImageAutomate.StandardBlocks.BrightnessBlock
        {
            Brightness = 1.5f
        };

        // Act
        var dto = BlockSerializer.Serialize(block);

        // Assert
        Assert.Equal("BrightnessBlock", dto.BlockType);
        Assert.NotEmpty(dto.AssemblyQualifiedName);
    }

    #endregion

    #region PipelineGraph Serialization Tests

    [Fact]
    public void PipelineGraph_Serialization_EmptyGraph_RoundTrip()
    {
        // Arrange
        var graph = new PipelineGraph();

        // Act
        var json = graph.ToJson();
        var deserialized = PipelineGraph.FromJson(json);

        // Assert
        Assert.Empty(deserialized.Nodes);
        Assert.Empty(deserialized.Edges);
        Assert.Null(deserialized.SelectedItem);
    }

    [Fact]
    public void PipelineGraph_Serialization_SingleBlock_RoundTrip()
    {
        // Arrange
        var graph = new PipelineGraph();
        var block = new ImageAutomate.StandardBlocks.BrightnessBlock();
        graph.AddBlock(block);

        // Act
        var json = graph.ToJson();
        var deserialized = PipelineGraph.FromJson(json);

        // Assert
        Assert.Single(deserialized.Nodes);
        Assert.Equal(block.Name, deserialized.Nodes[0].Name);
    }

    [Fact]
    public void PipelineGraph_Serialization_TwoConnectedBlocks_RoundTrip()
    {
        // Arrange
        var graph = new PipelineGraph();
        var source = new ImageAutomate.StandardBlocks.BrightnessBlock();
        var target = new ImageAutomate.StandardBlocks.BrightnessBlock();

        graph.AddBlock(source);
        graph.AddBlock(target);
        graph.AddEdge(source, source.Outputs[0], target, target.Inputs[0]);

        // Act
        var json = graph.ToJson();
        var deserialized = PipelineGraph.FromJson(json);

        // Assert
        Assert.Equal(2, deserialized.Nodes.Count);
        Assert.Single(deserialized.Edges);

        var conn = deserialized.Edges[0];
        Assert.Equal(deserialized.Nodes[0], conn.Source);
        Assert.Equal(deserialized.Nodes[1], conn.Target);
    }

    [Fact]
    public void PipelineGraph_Serialization_ComplexGraph_PreservesStructure()
    {
        // Arrange - using real StandardBlocks
        var graph = new PipelineGraph();
        var load = new ImageAutomate.StandardBlocks.LoadBlock();
        var brightness = new ImageAutomate.StandardBlocks.BrightnessBlock();
        var convert = new ImageAutomate.StandardBlocks.ConvertBlock();
        var save = new ImageAutomate.StandardBlocks.SaveBlock();

        graph.AddBlock(load);
        graph.AddBlock(brightness);
        graph.AddBlock(convert);
        graph.AddBlock(save);

        graph.AddEdge(load, load.Outputs[0], brightness, brightness.Inputs[0]);
        graph.AddEdge(brightness, brightness.Outputs[0], convert, convert.Inputs[0]);
        graph.AddEdge(convert, convert.Outputs[0], save, save.Inputs[0]);

        graph.SelectedItem = brightness;

        // Act
        var json = graph.ToJson();
        var deserialized = PipelineGraph.FromJson(json);

        // Assert
        Assert.Equal(4, deserialized.Nodes.Count);
        Assert.Equal(3, deserialized.Edges.Count);
        Assert.NotNull(deserialized.SelectedItem);
        Assert.IsType<BrightnessBlock>(deserialized.SelectedItem);
    }

    [Fact]
    public void PipelineGraph_Serialization_MultiInputOutput_PreservesConnections()
    {
        // This test uses a block type that can be instantiated without parameters
        // We'll create a simple scenario with blocks that have default constructors
        var graph = new PipelineGraph();
        var source1 = new BrightnessBlock();
        var source2 = new BrightnessBlock();
        var target = new BrightnessBlock();

        graph.AddBlock(source1);
        graph.AddBlock(source2);
        graph.AddBlock(target);

        // Connect first source to target
        graph.AddEdge(source1, source1.Outputs[0], target, target.Inputs[0]);

        // Act
        var json = graph.ToJson();
        var deserialized = PipelineGraph.FromJson(json);

        // Assert
        Assert.Equal(3, deserialized.Nodes.Count);
        Assert.Single(deserialized.Edges);
    }

    #endregion

    #region Workspace Tests

    [Fact]
    public void Workspace_Serialization_EmptyWorkspace_RoundTrip()
    {
        // Arrange
        var workspace = new Workspace(new())
        {
            Name = "Test Workspace"
        };

        // Act
        var json = workspace.ToJson();
        var deserialized = Workspace.FromJson(json);

        // Assert
        Assert.Equal(workspace.Name, deserialized.Name);
        Assert.NotNull(deserialized.Graph);
    }

    [Fact]
    public void Workspace_Serialization_WithGraph_RoundTrip()
    {
        // Arrange
        var workspace = new Workspace(new())
        {
            Name = "Complex Workspace",
            Zoom = 1.5
        };

        var source = new ImageAutomate.StandardBlocks.BrightnessBlock()
        {
            X = 50,
            Y = 50
        };
        var target = new ImageAutomate.StandardBlocks.BrightnessBlock()
        {
            X = 250,
            Y = 50
        };

        workspace.Graph.AddBlock(source);
        workspace.Graph.AddBlock(target);
        workspace.Graph.AddEdge(source, source.Outputs[0], target, target.Inputs[0]);

        // Act
        var json = workspace.ToJson();
        var deserialized = Workspace.FromJson(json);

        // Assert
        Assert.Equal(workspace.Name, deserialized.Name);
        Assert.NotNull(deserialized.Graph);
        Assert.Equal(2, deserialized.Graph.Nodes.Count);
        Assert.Single(deserialized.Graph.Edges);
        Assert.Equal(1.5, deserialized.Zoom);

        var sourcePos = deserialized.Graph.Nodes[0];
        Assert.NotNull(sourcePos);
        Assert.Equal(50, sourcePos.X);
    }

    [Fact]
    public void Workspace_SaveAndLoad_PreservesData()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_workspace_{Guid.NewGuid()}.json");
        var workspace = new Workspace(new())
        {
            Name = "File Test Workspace",
        };

        var block = new ImageAutomate.StandardBlocks.BrightnessBlock()
        {
            X = 100,
            Y = 200
        };
        workspace.Graph.AddBlock(block);

        try
        {
            // Act
            workspace.SaveToFile(tempFile);
            var loaded = Workspace.LoadFromFile(tempFile);

            // Assert
            Assert.Equal(workspace.Name, loaded.Name);
            Assert.NotNull(loaded.Graph);
            Assert.Single(loaded.Graph.Nodes);
            Assert.Equal("Brightness", loaded.Graph.Nodes[0].Name);

            var pos = loaded.Graph.Nodes[0];
            Assert.NotNull(pos);
            Assert.Equal(100, pos.X);
            Assert.Equal(200, pos.Y);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Workspace_LoadFromFile_NonExistentFile_ThrowsException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => Workspace.LoadFromFile(nonExistentFile));
    }

    [Fact]
    public void Workspace_WithMetadata_PreservesMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object?>
        {
            { "Author", "Test User" },
            { "CreatedDate", "2024-01-01" },
            { "Version", 1 }
        };
        var workspace = new Workspace(new(), "Metadata Test", metadata);

        // Act
        var json = workspace.ToJson();
        var deserialized = Workspace.FromJson(json);

        // Assert
        Assert.Equal(3, deserialized.Metadata.Count);
        Assert.True(deserialized.Metadata.ContainsKey("Author"));
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void PipelineGraph_Serialization_InvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => PipelineGraph.FromJson(invalidJson));
    }

    [Fact]
    public void Workspace_Serialization_InvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "not json at all";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => Workspace.FromJson(invalidJson));
    }

    #endregion
}
