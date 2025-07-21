using System;
using System.Collections.Generic;
using Godot;

namespace Terraria3D;

public enum BlockId
{
    Nop = 0,
    Dirt = 2
}

public interface IBlock
{
    static abstract BlockId Id { get; }

    static abstract Color GetShaderData();

    static BoxShape3D GetShape() => null;
}

public class BlockRegistry
{
    // 存储所有方块类型
    public static Dictionary<BlockId, (Type, Func<Color>, Func<BoxShape3D>)> BlockTypes { get; } = new();

    // 注册方块类型
    public static void RegisterBlock<T>() where T : IBlock
    {
        var getShaderDataMethod = typeof(T).GetMethod("GetShaderData");
        var getShapeMethod = typeof(T).GetMethod("GetShape");
        BlockTypes.Add(T.Id, (typeof(T), (Func<Color>)Delegate.CreateDelegate(typeof(Func<Color>), getShaderDataMethod), (Func<BoxShape3D>)Delegate.CreateDelegate(typeof(Func<BoxShape3D>), getShapeMethod)));
    }

    public static Color GetShaderData(BlockId id)
    {
        if (BlockTypes.TryGetValue(id, out var blockType))
        {
            return blockType.Item2();
        }
        throw new Exception($"Unknown block type: {id}");
    }

    // 获取方块 Shape
    public static BoxShape3D GetShape(BlockId id)
    {
        if (BlockTypes.TryGetValue(id, out var blockType))
        {
            return blockType.Item3();
        }
        return null;
    }

    public static Block NewBlock(BlockId id)
    {
        if (BlockTypes.TryGetValue(id, out var blockType))
        {
            return new Block(id, blockType.Item2, blockType.Item3);
        }
        throw new Exception($"Unknown block type: {id}");
    }
}

public class Block(BlockId blockId, Func<Color> getShaderDataDelegate, Func<BoxShape3D> getShapeDelegate)
{
    public BlockId BlockId = blockId;

    public Func<Color> GetShaderData = getShaderDataDelegate;
    public Func<BoxShape3D> GetShape = getShapeDelegate;
}