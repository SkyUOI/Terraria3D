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

    public void InitTerrain(VoxelBuffer outBuffer, Vector3I originInVoxels, int lod)
    {
        for (var i = 0; i < Constants.ChunkX; ++i)
        {
            for (var j = 0; j < Constants.ChunkZ; ++j)
            {
                var globalPos = originInVoxels + new Vector3I(i, 0, j);
                var pos2d = new Vector2(globalPos.X, globalPos.Z);
                var height = ConvertNoiseToHeight(Noise.GetNoise2Dv(pos2d));
                if (height >= originInVoxels.Y)
                {
                    outBuffer.FillArea((ulong)Blocks.Model(BlockType.Dirt), new Vector3I(i, 0, j), new Vector3I(i + 1, Math.Min(height - originInVoxels.Y, Constants.ChunkY - 1) + 1, j + 1), (uint)VoxelBuffer.ChannelId.ChannelType);
                }
            }
        }
    }

    public static int ConvertNoiseToHeight(float noise)
    {
        var height = (int)((noise + 1) * 60) - 10;
        // GD.Print($"height: {height} noise: {noise}");
        return height;
    }
};
