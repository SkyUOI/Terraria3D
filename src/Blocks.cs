using System;
using System.Collections.Generic;
using Godot;

namespace Terraria3D;

/// <summary>
/// Block type identifiers. Enum member names match resource_name in
/// blocks_library.tres by convention: <c>BlockType.Dirt</c> → <c>"dirt"</c>.
/// <br/>
/// To add a new block:
/// 1. Add the model to blocks_library.tres with a matching resource_name
/// 2. Add the enum member here + one <c>BlockInfo</c> line in <see cref="Blocks.Registry"/>
/// </summary>
public enum BlockType
{
    Air,
    Dirt,
    Grass,
    Stone,
    Sand,
    Sandstone,
    Snow,
    Ice,
    Mud,
}

/// <summary>Per-block gameplay properties, independent of the voxel model.</summary>
public record BlockInfo(
    string Name,
    float Hardness = 1f,
    bool IsSolid = true,
    bool CanPlace = true,
    BiomeType? BiomeAffiliation = null
);

/// <summary>
/// Single entry point for all block-related queries — model indices, gameplay
/// metadata, and registration. Model indices are resolved automatically from
/// blocks_library.tres by converting the enum member name to lowercase.
/// </summary>
public static class Blocks
{
    /// <summary>The loaded VoxelBlockyLibrary resource.</summary>
    public static VoxelBlockyLibrary Library { get; }

    /// <summary>Per-block gameplay metadata. One line per block type.</summary>
    public static readonly Dictionary<BlockType, BlockInfo> Registry = new()
    {
        [BlockType.Air]       = new("Air",       Hardness: 0f,   IsSolid: false, CanPlace: false),
        [BlockType.Dirt]      = new("Dirt",      Hardness: 0.5f, BiomeAffiliation: BiomeType.Forest),
        [BlockType.Grass]     = new("Grass",     Hardness: 0.6f, BiomeAffiliation: BiomeType.Forest),
        [BlockType.Stone]     = new("Stone",     Hardness: 2.0f),
        [BlockType.Sand]      = new("Sand",      Hardness: 0.3f, BiomeAffiliation: BiomeType.Desert),
        [BlockType.Sandstone] = new("Sandstone", Hardness: 1.5f, BiomeAffiliation: BiomeType.Desert),
        [BlockType.Snow]      = new("Snow",      Hardness: 0.2f, BiomeAffiliation: BiomeType.Snow),
        [BlockType.Ice]       = new("Ice",       Hardness: 1.0f, BiomeAffiliation: BiomeType.Snow),
        [BlockType.Mud]       = new("Mud",       Hardness: 0.5f, BiomeAffiliation: BiomeType.Jungle),
    };

    static Blocks()
    {
        Library = GD.Load<VoxelBlockyLibrary>("res://src/blocks_library.tres");

        foreach (var type in Registry.Keys)
        {
            var resourceName = type.ToString().ToLowerInvariant();
            if (Library.GetModelIndexFromResourceName(resourceName) < 0)
                GD.PushError(
                    $"Block model '{resourceName}' not found in blocks_library.tres. " +
                    "Add a model with a matching resource_name, or fix the enum/name spelling.");
        }
    }

    /// <summary>Get the voxel model index for a block type.</summary>
    public static int Model(BlockType type) =>
        Library.GetModelIndexFromResourceName(type.ToString().ToLowerInvariant());

    /// <summary>Get gameplay metadata for a block type.</summary>
    public static BlockInfo Info(BlockType type) => Registry[type];
}
