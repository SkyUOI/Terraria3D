using System;
using System.Collections.Generic;
using Godot;

public enum BlockId
{
    Nop = 0,
    Dirt = 2
}

public interface IBlock
{
    static abstract BlockId Id { get; }

    static abstract Mesh GetMesh();

    static Godot.Collections.Array GetShape() => null;
}

public class BlockRegistry
{
    // 存储所有方块类型
    public static Dictionary<BlockId, Type> BlockTypes { get; } = new();

    // 注册方块类型
    public static void RegisterBlock<T>() where T : IBlock
    {
        BlockTypes.Add(T.Id, typeof(T));
    }

    // 获取方块 Mesh
    public static Mesh GetMesh(BlockId id)
    {
        if (BlockTypes.TryGetValue(id, out var blockType))
        {
            var getMeshMethod = blockType.GetMethod("GetMesh");
            return (Mesh)getMeshMethod.Invoke(null, null);
        }
        return null;
    }

    // 获取方块 Shape
    public static Godot.Collections.Array GetShape(BlockId id)
    {
        if (BlockTypes.TryGetValue(id, out var blockType))
        {
            var getMeshMethod = blockType.GetMethod("GetShape");
            return (Godot.Collections.Array)getMeshMethod.Invoke(null, null);
        }
        return null;
    }
}
