using System.Collections.Generic;
using System.Threading.Tasks;
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
        AssertThat(Chunk.InLocalChunkPos(new Vector3(0, 0, 0))).IsTrue();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(15, 15, 15))).IsTrue();

        // Invalid positions
        AssertThat(Chunk.InLocalChunkPos(new Vector3(16, 0, 0))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(0, 16, 0))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(0, 0, 16))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(-1, 0, 0))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(0, -1, 0))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(0, 0, -1))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(-1, -1, -1))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(16, 15, 15))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(15, 16, 15))).IsFalse();
        AssertThat(Chunk.InLocalChunkPos(new Vector3(15, 15, 16))).IsFalse();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestPositionConversions()
    {
        var chunk = new Chunk(new Vector3I(2, 3, 4));

        // Test FromBlockPos
        var blockPos = new Vector3I(1, 2, 3);
        var transform = Chunk.FromBlockPos(blockPos);
        AssertThat(transform.Origin).IsEqual(blockPos);

        // Test GetGlobalChunkPos
        var localPos = new Vector3I(5, 6, 7);
        var globalPos = chunk.GetGlobalChunkPosFromLocalChunkPos(localPos);
        AssertThat(globalPos).IsEqual(new Vector3I(2 * Chunk.X + 5, 3 * Chunk.Y + 6, 4 * Chunk.Z + 7));

        // Test ConvertLocalChunkPosToLocalRealPos
        var realPos = chunk.ConvertLocalChunkPosToLocalRealPos(localPos);
        AssertThat(realPos).IsEqual(new Vector3(5 * Consts.BlockSize, 6 * Consts.BlockSize, 7 * Consts.BlockSize));

        // Test ConvertGlobalChunkPosToGlobalRealPos 
        var globalRealPos = chunk.ConvertGlobalChunkPosToGlobalRealPos(globalPos);
        AssertThat(globalRealPos).IsEqual(new Vector3(
            (2 * Chunk.X + 5) * Consts.BlockSize,
            (3 * Chunk.Y + 6) * Consts.BlockSize,
            (4 * Chunk.Z + 7) * Consts.BlockSize));

        // Test ConvertLocalChunkPosToGlobalRealPos
        var globalRealPos2 = chunk.ConvertLocalChunkPosToGlobalRealPos(localPos);
        AssertThat(globalRealPos2).IsEqual(globalRealPos);

        // Test GetLocalChunkPosFromGlobalRealPos
        var localPos2 = chunk.GetLocalChunkPosFromGlobalRealPos(globalRealPos);
        AssertThat(localPos2).IsEqual(new Vector3I(5, 6, 7));

        // Test GetLocalChunkPosFromGlobalChunkPos
        var localPos3 = chunk.GetLocalChunkPosFromGlobalChunkPos(globalPos);
        AssertThat(localPos3).IsEqual(localPos);
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
    public async Task TestMeshGeneration()
    {
        var chunk = new Chunk(Vector3I.Zero);

        // Test with empty chunk
        var emptyMesh = await chunk.GenerateMesh();
        AssertThat(emptyMesh.GetSurfaceCount()).IsEqual(0);

        // Test with one block
        chunk.Blocks[0, 0, 0] = new Block(BlockId.Dirt, () => Colors.White, () => new BoxShape3D());
        var mesh = await chunk.GenerateMesh();
        AssertThat(mesh.GetSurfaceCount()).IsEqual(1);

        // Test mesh instance creation
        var meshInstance = await chunk.GenerateMeshInstance3D();
        AssertThat(meshInstance.Mesh).IsNotNull();
        AssertThat(meshInstance.MaterialOverride).IsNotNull();
        AddNode(meshInstance);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void TestGetStartPoint()
    {
        // Test positive position
        var chunk = new Chunk(new Vector3I(2, 3, 4));
        var startPoint = chunk.GetRealStartPoint();
        var expected = new Vector3(2 * Chunk.X * Consts.BlockSize, 3 * Chunk.Y * Consts.BlockSize, 4 * Chunk.Z * Consts.BlockSize);
        AssertThat(startPoint).IsEqual(expected);

        // Test negative position
        chunk = new Chunk(new Vector3I(-1, -2, -3));
        startPoint = chunk.GetRealStartPoint();
        expected = new Vector3(-1 * Chunk.X * Consts.BlockSize, -2 * Chunk.Y * Consts.BlockSize, -3 * Chunk.Z * Consts.BlockSize);
        AssertThat(startPoint).IsEqual(expected);
    }
}
