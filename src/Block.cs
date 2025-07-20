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

    static Godot.Collections.Array GetShape() => null;
}

public class BlockRegistry
{
    // 存储所有方块类型
    public static Dictionary<BlockId, (Type, Func<Color>)> BlockTypes { get; } = new();

    // 注册方块类型
    public static void RegisterBlock<T>() where T : IBlock
    {
        var getMeshMethod = typeof(T).GetMethod("GetShaderData");
        BlockTypes.Add(T.Id, (typeof(T), (Func<Color>)Delegate.CreateDelegate(typeof(Action), getMeshMethod)));
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
    public static Godot.Collections.Array GetShape(BlockId id)
    {
        if (BlockTypes.TryGetValue(id, out var blockType))
        {
            var getMeshMethod = blockType.Item1.GetMethod("GetShape");
            if (getMeshMethod != null) return (Godot.Collections.Array)getMeshMethod.Invoke(null, null);
        }
        return null;
    }
}