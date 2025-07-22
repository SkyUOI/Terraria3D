using System.Collections.Generic;
using GdUnit4;
using Godot;
using Terraria3D;
using static GdUnit4.Assertions;

[TestSuite]
public class ChunkTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void TestChunkInitialization()
    {
        // Test positive position
        var chunkPos = new Vector3I(1, 2, 3);
        var chunk = new Chunk(chunkPos);

        AssertThat(chunk.Pos).IsEqual(chunkPos);
        AssertThat(chunk.Blocks.GetLength(0)).IsEqual(Chunk.X);
        AssertThat(chunk.Blocks.GetLength(1)).IsEqual(Chunk.Y);
        AssertThat(chunk.Blocks.GetLength(2)).IsEqual(Chunk.Z);

        // Test negative position
        chunkPos = new Vector3I(-1, -2, -3);
        chunk = new Chunk(chunkPos);

        AssertThat(chunk.Pos).IsEqual(chunkPos);
        AssertThat(chunk.Blocks.GetLength(0)).IsEqual(Chunk.X);
        AssertThat(chunk.Blocks.GetLength(1)).IsEqual(Chunk.Y);
        AssertThat(chunk.Blocks.GetLength(2)).IsEqual(Chunk.Z);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestLocalPosValidation()
    {
        // Valid positions
        AssertThat(Chunk.LocalPosInChunk(new Vector3(0, 0, 0))).IsTrue();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(15, 15, 15))).IsTrue();

        // Invalid positions
        AssertThat(Chunk.LocalPosInChunk(new Vector3(16, 0, 0))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(0, 16, 0))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(0, 0, 16))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(-1, 0, 0))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(0, -1, 0))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(0, 0, -1))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(-1, -1, -1))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(16, 15, 15))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(15, 16, 15))).IsFalse();
        AssertThat(Chunk.LocalPosInChunk(new Vector3(15, 15, 16))).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestPositionConversions()
    {
        // Test positive positions
        var chunk = new Chunk(new Vector3I(2, 3, 4));
        var localPos = new Vector3I(5, 6, 7);

        var globalPos = chunk.GetGlobalPos(localPos);
        var expectedGlobal = new Vector3(2 * Chunk.X + 5, 3 * Chunk.Y + 6, 4 * Chunk.Z + 7);
        AssertThat(globalPos).IsEqual(expectedGlobal);

        var convertedLocal = chunk.GetLocalPos(globalPos);
        AssertThat(convertedLocal).IsEqual(new Vector3(localPos.X, localPos.Y, localPos.Z));

        // Test negative positions
        chunk = new Chunk(new Vector3I(-1, -2, -3));
        localPos = new Vector3I(5, 6, 7);

        globalPos = chunk.GetGlobalPos(localPos);
        expectedGlobal = new Vector3(-1 * Chunk.X + 5, -2 * Chunk.Y + 6, -3 * Chunk.Z + 7);
        AssertThat(globalPos).IsEqual(expectedGlobal);

        convertedLocal = chunk.GetLocalPos(globalPos);
        AssertThat(convertedLocal).IsEqual(new Vector3(localPos.X, localPos.Y, localPos.Z));
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestHeightRange()
    {
        // Test positive positions
        var chunk = new Chunk(new Vector3I(0, 0, 0));
        var (min, max) = chunk.HeightRange();
        AssertThat(min).IsEqual(0);
        AssertThat(max).IsEqual(15);

        chunk = new Chunk(new Vector3I(0, 1, 0));
        (min, max) = chunk.HeightRange();
        AssertThat(min).IsEqual(16);
        AssertThat(max).IsEqual(31);

        // Test negative positions
        chunk = new Chunk(new Vector3I(0, -1, 0));
        (min, max) = chunk.HeightRange();
        AssertThat(min).IsEqual(-16);
        AssertThat(max).IsEqual(-1);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestMultiMeshGeneration()
    {
        var chunk = new Chunk(new Vector3I(0, 0, 0));

        // Test with empty chunk
        var multiMesh = chunk.GenerateMultiMesh();
        AssertThat(multiMesh.InstanceCount).IsEqual(0);

        // Add blocks in a cross pattern to ensure proper exposure
        chunk.Blocks[7, 7, 7] = BlockRegistry.NewBlock(BlockId.Dirt);  // Center block
        chunk.Blocks[7, 7, 6] = BlockRegistry.NewBlock(BlockId.Dirt);  // North
        chunk.Blocks[7, 7, 8] = BlockRegistry.NewBlock(BlockId.Dirt); // South
        chunk.Blocks[6, 7, 7] = BlockRegistry.NewBlock(BlockId.Dirt);  // West
        chunk.Blocks[8, 7, 7] = BlockRegistry.NewBlock(BlockId.Dirt); // East
        chunk.Blocks[7, 6, 7] = BlockRegistry.NewBlock(BlockId.Dirt);  // Down
        chunk.Blocks[7, 8, 7] = BlockRegistry.NewBlock(BlockId.Dirt); // Up

        // Add an isolated block that should be fully exposed
        chunk.Blocks[0, 0, 0] = BlockRegistry.NewBlock(BlockId.Dirt);

        multiMesh = chunk.GenerateMultiMesh();
        // Center block should be surrounded and not rendered
        // Isolated block should be rendered
        // All directional blocks should be rendered (7 total)
        AssertThat(multiMesh.InstanceCount).IsEqual(7);

        // Verify transforms for each instance
        var expectedPositions = new List<Vector3>
        {
            new Vector3(7, 7, 6),  // North
            new Vector3(7, 7, 8),  // South
            new Vector3(6, 7, 7),  // West
            new Vector3(8, 7, 7),  // East
            new Vector3(7, 6, 7),  // Down
            new Vector3(7, 8, 7),  // Up
            new Vector3(0, 0, 0)   // Isolated block
        };

        var dirtShaderData = BlockRegistry.GetShaderData(BlockId.Dirt);

        for (int i = 0; i < multiMesh.InstanceCount; i++)
        {
            var transform = multiMesh.GetInstanceTransform(i);
            var position = transform.Origin;

            // Verify position is in expected positions
            AssertThat(expectedPositions).Contains(position);
            expectedPositions.Remove(position);

            // Verify custom data (shader data) based on block type
            var customData = multiMesh.GetInstanceCustomData(i);
            AssertThat(customData).IsEqual(dirtShaderData);
        }

        // Ensure all expected positions were found
        AssertThat(expectedPositions).IsEmpty();

        // Test MultiMeshInstance3D creation
        var instance = chunk.GenerateMultiMeshInstance3D();
        AddNode(instance);
        AssertThat(instance).IsNotNull();
        AssertThat(instance.Multimesh).IsEqual(multiMesh);

        // Verify material properties
        var material = instance.MaterialOverride as ShaderMaterial;
        AssertThat(material).IsNotNull();
        AssertThat(material.Shader).IsNotNull();
        AssertThat(material.GetShaderParameter("atlas")).IsNotNull();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestGetStartPoint()
    {
        // Test positive position
        var chunk = new Chunk(new Vector3I(2, 3, 4));
        var startPoint = chunk.GetStartPoint();
        var expected = new Vector3(2 * Chunk.X, 3 * Chunk.Y, 4 * Chunk.Z);
        AssertThat(startPoint).IsEqual(expected);

        // Test negative position
        chunk = new Chunk(new Vector3I(-1, -2, -3));
        startPoint = chunk.GetStartPoint();
        expected = new Vector3(-1 * Chunk.X, -2 * Chunk.Y, -3 * Chunk.Z);
        AssertThat(startPoint).IsEqual(expected);
    }
}
