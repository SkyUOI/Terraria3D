using System;
using Godot;

namespace Terraria3D;

public partial class WorldGeneration : Node
{
    public static FastNoiseLite Noise = new FastNoiseLite();

    public static void Init()
    {
        Noise.Frequency = 0.05f;
    }

    public static void GenerateChunk(Chunk chunk)
    {
        var heightRange = chunk.HeightRange();
        GD.Print($"heightRange: {heightRange}");
        for (int i = 0; i < Chunk.X; ++i)
        {
            for (int j = 0; j < Chunk.Y; ++j)
            {
                var pos = chunk.GetGlobalPos(new Vector3I(i, j, 0));
                var pos2d = new Vector2(pos.X, pos.Z);
                var height = ConvertNoiseToHeight(Noise.GetNoise2Dv(pos2d));
                for (int k = heightRange.Item1; k <= Math.Min(height, heightRange.Item2); ++k)
                {
                    chunk.Blocks[i, k - heightRange.Item1, j] = BlockRegistry.NewBlock(BlockId.Dirt);
                }
            }
        }
    }

    public static int ConvertNoiseToHeight(float noise)
    {
        var height = (int)((noise + 1) * 200);
        // GD.Print($"height: {height} noise: {noise}");
        return height;
    }
}