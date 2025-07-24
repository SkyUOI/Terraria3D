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
        // Test positive positions
        var chunk = new Chunk(new Vector3I(2, 3, 4));
        var localPos = new Vector3I(5, 6, 7);

        var globalPos = chunk.GetGlobalChunkPos(localPos);
        var expectedGlobal = new Vector3I(2 * Chunk.X + 5, 3 * Chunk.Y + 6, 4 * Chunk.Z + 7);
        AssertThat(globalPos).IsEqual(expectedGlobal);

        var convertedLocal = chunk.GetLocalChunkPosFromGlobalRealPos(globalPos);
        AssertThat(convertedLocal).IsEqual(new Vector3(localPos.X, localPos.Y, localPos.Z));

        // Test negative positions
        chunk = new Chunk(new Vector3I(-1, -2, -3));
        localPos = new Vector3I(5, 6, 7);

        globalPos = chunk.GetGlobalChunkPos(localPos);
        expectedGlobal = new Vector3I(-1 * Chunk.X + 5, -2 * Chunk.Y + 6, -3 * Chunk.Z + 7);
        AssertThat(globalPos).IsEqual(expectedGlobal);

        convertedLocal = chunk.GetLocalChunkPosFromGlobalRealPos(globalPos);
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
    public void TestGetStartPoint()
    {
        // Test positive position
        var chunk = new Chunk(new Vector3I(2, 3, 4));
        var startPoint = chunk.GetRealStartPoint();
        var expected = new Vector3(2 * Chunk.X, 3 * Chunk.Y, 4 * Chunk.Z);
        AssertThat(startPoint).IsEqual(expected);

        // Test negative position
        chunk = new Chunk(new Vector3I(-1, -2, -3));
        startPoint = chunk.GetRealStartPoint();
        expected = new Vector3(-1 * Chunk.X, -2 * Chunk.Y, -3 * Chunk.Z);
        AssertThat(startPoint).IsEqual(expected);
    }
}
