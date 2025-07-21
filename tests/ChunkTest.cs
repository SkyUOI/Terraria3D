using GdUnit4;
using Godot;
using Terraria3D;
using static GdUnit4.Assertions;

[TestSuite]
public class ChunkTest
{
    [TestCase]
    [RequireGodotRuntime]
    public void TestHeight()
    {
        var chunk = new Chunk(new Vector3I(0, 0, 0));
        var (min, max) = chunk.HeightRange();
        AssertThat(min).IsEqual(0);
        AssertThat(max).IsEqual(15);
    }
}