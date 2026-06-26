using System.Collections.Generic;
using Godot;

namespace Terraria3D;

/// <summary>Vertical depth layer relative to the surface.</summary>
public enum DepthLayer
{
    /// <summary>0–5 blocks below the surface.</summary>
    Surface,
    /// <summary>5–100 blocks below the surface.</summary>
    Underground,
    /// <summary>100–400 blocks below the surface.</summary>
    Cavern,
    /// <summary>400+ blocks below the surface.</summary>
    Underworld,
}

/// <summary>Biome type identifiers used for terrain generation and spawning.</summary>
public enum BiomeType
{
    Forest,
    Desert,
    Snow,
    Jungle,
}

/// <summary>
/// Data-driven biome definition. Determines surface blocks, terrain shape,
/// noise thresholds for world generation, and the minimum number of
/// biome-affiliated blocks needed to activate this biome at runtime.
/// </summary>
public record BiomeInfo(
    string Name,
    BlockType SurfaceBlock,
    BlockType SubsurfaceBlock,
    BlockType UndergroundBlock,
    float HeightMultiplier,
    float BaseHeightOffset,
    float TempThreshold,
    float HumidThreshold,
    /// <summary>
    /// Minimum number of blocks with this biome's affiliation needed within
    /// the scan area for the biome to activate. Forest has threshold 0
    /// (always active by default when no other biome qualifies).
    /// </summary>
    int BlockThreshold = 0
);

/// <summary>
/// Registry of all biome definitions. Follows the same pattern as
/// <see cref="Blocks.Registry"/> and <see cref="entities.EntityRegistry"/>.
/// </summary>
public static class BiomeRegistry
{
    public static readonly Dictionary<BiomeType, BiomeInfo> Data = new()
    {
        [BiomeType.Forest] = new(
            Name: "Forest",
            SurfaceBlock: BlockType.Grass,
            SubsurfaceBlock: BlockType.Dirt,
            UndergroundBlock: BlockType.Stone,
            HeightMultiplier: 1.0f,
            BaseHeightOffset: 0f,
            TempThreshold: 0f,
            HumidThreshold: 0f,
            BlockThreshold: 0        // default — always active
        ),

        [BiomeType.Desert] = new(
            Name: "Desert",
            SurfaceBlock: BlockType.Sand,
            SubsurfaceBlock: BlockType.Sandstone,
            UndergroundBlock: BlockType.Sandstone,
            HeightMultiplier: 0.7f,
            BaseHeightOffset: -5f,
            TempThreshold: 0.35f,
            HumidThreshold: -0.1f,
            BlockThreshold: 100      // at least 100 desert blocks
        ),

        [BiomeType.Snow] = new(
            Name: "Snow",
            SurfaceBlock: BlockType.Snow,
            SubsurfaceBlock: BlockType.Ice,
            UndergroundBlock: BlockType.Ice,
            HeightMultiplier: 1.2f,
            BaseHeightOffset: 10f,
            TempThreshold: -0.35f,
            HumidThreshold: 0f,
            BlockThreshold: 150      // at least 150 snow/ice blocks
        ),

        [BiomeType.Jungle] = new(
            Name: "Jungle",
            SurfaceBlock: BlockType.Mud,
            SubsurfaceBlock: BlockType.Mud,
            UndergroundBlock: BlockType.Mud,
            HeightMultiplier: 1.0f,
            BaseHeightOffset: 2f,
            TempThreshold: 0.1f,
            HumidThreshold: 0.3f,
            BlockThreshold: 80       // at least 80 jungle blocks
        ),
    };

    /// <summary>Priority order for biome resolution (first to reach threshold wins).</summary>
    public static readonly BiomeType[] PriorityOrder =
        [BiomeType.Desert, BiomeType.Snow, BiomeType.Jungle, BiomeType.Forest];
}

/// <summary>
/// Biome detection — two separate systems:
/// <list type="bullet">
/// <item><see cref="GetGenerationBiome"/> — noise-based, used during world generation</item>
/// <item><see cref="GetBiome"/> — block-counting based, used at runtime for spawning</item>
/// </list>
/// Must be seeded via <see cref="SeedAll"/> during world load.
/// </summary>
public static class BiomeDetector
{
    /// <summary>Large-scale temperature noise (frequency ~0.0003).</summary>
    public static readonly FastNoiseLite TemperatureNoise = new()
    {
        Frequency = 0.0003f,
        FractalOctaves = 2,
    };

    /// <summary>Large-scale humidity noise (frequency ~0.0004).</summary>
    public static readonly FastNoiseLite HumidityNoise = new()
    {
        Frequency = 0.0004f,
        FractalOctaves = 2,
    };

    /// <summary>
    /// Seed all noise generators (temperature, humidity, terrain) from the
    /// world seed. Called once during world load for deterministic biome
    /// placement.
    /// </summary>
    public static void SeedAll(int seed)
    {
        TemperatureNoise.Seed = seed + 1000;
        HumidityNoise.Seed = seed + 2000;
        Generator.SeedTerrainNoise(seed);
    }

    // ── World-generation biome (noise-based) ──────────────────────────

    /// <summary>
    /// Resolve which biome to generate at a given world XZ position using
    /// temperature and humidity noise. Biomes are checked in priority order
    /// — first match wins. Forest is the default fallback.
    /// Used by <see cref="Generator"/> during chunk generation.
    /// </summary>
    public static BiomeType GetGenerationBiome(float worldX, float worldZ)
    {
        float temp = TemperatureNoise.GetNoise2D(worldX, worldZ);
        float humid = HumidityNoise.GetNoise2D(worldX, worldZ);

        // Desert: hot and dry
        if (temp > 0.35f && humid < -0.1f)
            return BiomeType.Desert;

        // Snow: cold
        if (temp < -0.35f)
            return BiomeType.Snow;

        // Jungle: warm and wet
        if (temp > 0.1f && humid > 0.3f)
            return BiomeType.Jungle;

        // Default: Forest
        return BiomeType.Forest;
    }

    // ── Runtime biome detection (block-counting) ──────────────────────

    /// <summary>
    /// Detect the active biome at a voxel position by counting surrounding
    /// blocks. Copies a voxel buffer around <paramref name="voxelCenter"/>,
    /// tallies each block's <see cref="BlockInfo.BiomeAffiliation"/>, and
    /// returns the first biome in priority order whose threshold is met.
    /// Forest (threshold 0) is the default fallback.
    /// </summary>
    /// <param name="voxelCenter">Center of the scan area in voxel coordinates.</param>
    /// <param name="voxelTool">VoxelTool obtained from the VoxelTerrain.</param>
    /// <param name="scanRadiusXZ">Half-size of the scan area in X and Z (default 16 → 33×33 buffer).</param>
    /// <param name="scanRadiusY">Half-size of the scan area in Y (default 8 → 17-tall buffer).</param>
    public static BiomeType GetBiome(
        Vector3I voxelCenter,
        VoxelTool voxelTool,
        int scanRadiusXZ = 16,
        int scanRadiusY = 8)
    {
        var bufSize = new Vector3I(
            scanRadiusXZ * 2 + 1,
            scanRadiusY * 2 + 1,
            scanRadiusXZ * 2 + 1
        );
        var bufOrigin = voxelCenter - new Vector3I(scanRadiusXZ, scanRadiusY, scanRadiusXZ);

        var buffer = new VoxelBuffer();
        buffer.Create(bufSize.X, bufSize.Y, bufSize.Z);

        // Copy terrain voxels into the local buffer (type channel only)
        // Type-channel bit = 1 << CHANNEL_TYPE (which is 0), i.e. bit 0 = 1
        voxelTool.Copy(
            bufOrigin,
            buffer,
            1,
            withMetadata: false
        );

        var counts = CountBiomeBlocks(buffer);
        return ResolveFromCounts(counts);
    }

    /// <summary>
    /// Scan a <see cref="VoxelBuffer"/> and count how many blocks belong
    /// to each biome, based on each block type's <see cref="BlockInfo.BiomeAffiliation"/>.
    /// </summary>
    public static Dictionary<BiomeType, int> CountBiomeBlocks(VoxelBuffer buffer)
    {
        var counts = new Dictionary<BiomeType, int>();
        var size = buffer.GetSize();
        uint channel = (uint)VoxelBuffer.ChannelId.ChannelType;

        for (int z = 0; z < size.Z; ++z)
        {
            for (int y = 0; y < size.Y; ++y)
            {
                for (int x = 0; x < size.X; ++x)
                {
                    int modelIndex = (int)buffer.GetVoxel(x, y, z, channel);
                    var blockType = BlockTypeFromModel(modelIndex);
                    var affiliation = Blocks.Info(blockType).BiomeAffiliation;
                    if (affiliation != null)
                    {
                        counts.TryGetValue(affiliation.Value, out int current);
                        counts[affiliation.Value] = current + 1;
                    }
                }
            }
        }

        return counts;
    }

    /// <summary>
    /// Determine the active biome from block counts, checking each biome
    /// in priority order. Returns the first biome whose count meets or
    /// exceeds its <see cref="BiomeInfo.BlockThreshold"/>. Forest (threshold 0)
    /// always matches as a fallback.
    /// </summary>
    public static BiomeType ResolveFromCounts(Dictionary<BiomeType, int> counts)
    {
        foreach (var biome in BiomeRegistry.PriorityOrder)
        {
            int threshold = BiomeRegistry.Data[biome].BlockThreshold;
            int count = counts.GetValueOrDefault(biome, 0);
            if (count >= threshold)
                return biome;
        }

        return BiomeType.Forest;
    }

    /// <summary>
    /// Map a voxel model index back to a <see cref="BlockType"/>.
    /// Uses a simple linear scan of all registered block types — acceptable
    /// because the registry is small (~10 entries).
    /// </summary>
    private static BlockType BlockTypeFromModel(int modelIndex)
    {
        foreach (var kv in Blocks.Registry)
        {
            if (Blocks.Model(kv.Key) == modelIndex)
                return kv.Key;
        }
        return BlockType.Air;
    }

    // ── Depth & surface utilities ─────────────────────────────────────

    /// <summary>
    /// Get the depth layer for a given voxel Y coordinate relative to
    /// the surface height at that column.
    /// </summary>
    public static DepthLayer GetDepthLayer(int voxelY, int surfaceHeight)
    {
        int depthBelowSurface = surfaceHeight - voxelY;

        return depthBelowSurface switch
        {
            <= 5   => DepthLayer.Surface,
            <= 100 => DepthLayer.Underground,
            <= 400 => DepthLayer.Cavern,
            _      => DepthLayer.Underworld,
        };
    }

    /// <summary>
    /// Compute the biome-aware surface height (in voxels) at a given
    /// world XZ position. Used by both terrain generation and the spawn
    /// system for depth-layer calculation.
    /// </summary>
    public static int GetSurfaceHeight(float worldX, float worldZ)
    {
        float rawNoise = Generator.Noise.GetNoise2D(worldX, worldZ);
        var biome = GetGenerationBiome(worldX, worldZ);
        var info = BiomeRegistry.Data[biome];
        float modifiedNoise = rawNoise * info.HeightMultiplier;
        return Generator.ConvertNoiseToHeight(modifiedNoise) + (int)info.BaseHeightOffset;
    }
}
