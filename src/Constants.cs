using System;
using System.Runtime.InteropServices;
using Godot;

namespace Terraria3D;

public static class Constants
{
    public const int YLimit = 8400;
    public const float BlockSize = 0.5f;
    public const int DayTime = 24 * 60;
    public const int ChunkX = 16;
    public const int ChunkZ = 16;
    public const int ChunkY = 16;
}

public static class BlocksLibrary
{
    public static int Air;
    public static int Dirt;
    public static VoxelBlockyLibrary library;

    static BlocksLibrary()
    {
        library = GD.Load<VoxelBlockyLibrary>("res://src/blocks_library.tres");
        Air = library.GetModelIndexFromResourceName("air");
        Dirt = library.GetModelIndexFromResourceName("dirt");
    }
}
