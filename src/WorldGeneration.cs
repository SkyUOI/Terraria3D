using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class WorldGeneration : Node
{
    public static FastNoiseLite noise = new FastNoiseLite();

    public static void Init()
    {
        noise.Frequency = 0.5f;
    }

    public static void GenerateChunk(Chunk chunk)
    {
        for (int i = 0; i < Chunk.X; ++i)
        {
            for (int j = 0; j < Chunk.Y; ++j)
            {
                var pos = chunk.GetGlobalPos(new Vector3I(i, j, 0));
                var pos2d = new Vector2(pos.X, pos.Z);
                var height = ConvertNoiseToHeight(noise.GetNoise2Dv(pos2d));
                var height_range = chunk.HeightRange();
                for (int k = height_range.Item1; k <= Math.Min(height, height_range.Item2); ++k)
                {
                    chunk.blocks[i, k - height_range.Item1, j] = new Block(BlockId.Dirt);
                }
            }
        }
    }

    public static int ConvertNoiseToHeight(float noise)
    {
        return (int)(noise * (Consts.Y_LIMIT / 2));
    }
}
