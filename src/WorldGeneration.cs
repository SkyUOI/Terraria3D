using System;
using System.Threading.Tasks;
using Godot;

namespace Terraria3D;

public partial class WorldGeneration : Node
{
    public static FastNoiseLite Noise = new();

    public static void Init()
    {
        Noise.Frequency = 0.001f;
    }

    public static async Task GenerateChunk(Chunk chunk)
    {
        await Task.Run(() =>
        {
            var heightRange = chunk.HeightRange();
            // GD.Print($"heightRange: {heightRange}");
            for (var i = 0; i < Chunk.X; ++i)
            {
                for (var j = 0; j < Chunk.Z; ++j)
                {
                    var pos = chunk.GetGlobalPos(new Vector3I(i, 0, j));
                    var pos2d = new Vector2(pos.X, pos.Z);
                    var height = ConvertNoiseToHeight(Noise.GetNoise2Dv(pos2d));
                    for (var k = heightRange.Item1; k <= Math.Min(height, heightRange.Item2); ++k)
                    {
                        chunk.Blocks[i, k - heightRange.Item1, j] = BlockRegistry.NewBlock(BlockId.Dirt);
                    }
                }
            }
        });
    }

    public static int ConvertNoiseToHeight(float noise)
    {
        var height = (int)((noise + 1) * 30);
        // GD.Print($"height: {height} noise: {noise}");
        return height;
    }
}