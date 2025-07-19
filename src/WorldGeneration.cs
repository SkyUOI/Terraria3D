using Godot;
using System;

public partial class WorldGeneration : Node
{
    public static FastNoiseLite noise = new FastNoiseLite();

    public static void Init()
    {
        noise.Frequency = 0.05f;
    }

    public static void GenerateChunk(Chunk chunk)
    {
        for (int i = 1; i <= 10; ++i)
        {
            chunk.blocks[i, i, i] = new Block(BlockId.Dirt);
        }
    }
}
