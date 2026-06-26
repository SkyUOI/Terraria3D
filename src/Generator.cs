using System;
using System.ComponentModel.DataAnnotations;
using Godot;
using Terraria3D;

partial class Generator : VoxelGeneratorScript
{
    public override void _GenerateBlock(VoxelBuffer outBuffer, Vector3I originInVoxels, int lod)
    {
        base._GenerateBlock(outBuffer, originInVoxels, lod);
        InitTerrain(outBuffer, originInVoxels, lod);
    }

    public override int _GetUsedChannelsMask()
    {
        return 1 << (int)VoxelBuffer.ChannelId.ChannelType;
    }

    public static FastNoiseLite Noise = new();

    Generator()
    {
        Noise.Frequency = 0.0055f;
        Noise.FractalOctaves = 4;
    }

    /// <summary>Seed the terrain noise from the world seed.</summary>
    public static void SeedTerrainNoise(int seed)
    {
        Noise.Seed = seed;
    }

    public void InitTerrain(VoxelBuffer outBuffer, Vector3I originInVoxels, int lod)
    {
        var maxY = Constants.ChunkY;

        for (var i = 0; i < Constants.ChunkX; ++i)
        {
            for (var j = 0; j < Constants.ChunkZ; ++j)
            {
                var globalX = originInVoxels.X + i;
                var globalZ = originInVoxels.Z + j;

                // Resolve biome for this column (noise-based, for world generation)
                var biome = BiomeDetector.GetGenerationBiome(globalX, globalZ);
                var info = BiomeRegistry.Data[biome];

                // Compute biome-aware terrain height
                var rawNoise = Noise.GetNoise2D(globalX, globalZ);
                var height = ConvertNoiseToHeight(rawNoise * info.HeightMultiplier)
                             + (int)info.BaseHeightOffset;

                if (height < originInVoxels.Y)
                    continue; // Column is entirely above surface (all air)

                var columnTop = Math.Min(height - originInVoxels.Y, maxY - 1);

                // Bulk-fill underground section with stone (depth >= 5 below surface)
                var stoneTop = columnTop - 5;
                if (stoneTop >= 0)
                {
                    outBuffer.FillArea(
                        (ulong)Blocks.Model(BlockType.Stone),
                        new Vector3I(i, 0, j),
                        new Vector3I(i + 1, stoneTop + 1, j + 1),
                        (uint)VoxelBuffer.ChannelId.ChannelType
                    );
                }

                // Overwrite the top 5 blocks with biome surface / subsurface blocks
                var surfaceStart = Math.Max(0, columnTop - 4);
                for (var y = surfaceStart; y <= columnTop; ++y)
                {
                    var depthFromSurface = columnTop - y;
                    var block = depthFromSurface == 0
                        ? info.SurfaceBlock
                        : info.SubsurfaceBlock;
                    outBuffer.SetVoxel(
                        (ulong)Blocks.Model(block),
                        i, y, j,
                        (int)VoxelBuffer.ChannelId.ChannelType
                    );
                }
            }
        }
    }

    public static int ConvertNoiseToHeight(float noise)
    {
        var height = (int)((noise + 1) * 60) - 10;
        return height;
    }
};
