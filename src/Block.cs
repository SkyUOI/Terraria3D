using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using Terraria3D.block.NormalBlock;

namespace Terraria3D;

public enum BlockId
{
    Nop = 0,
    Dirt = 2,
    Water = 3
}

public interface IBlockType
{
    static abstract BlockId Id { get; }

    public static Color GetShaderData() => new Color(0, 0, 0);

    public static BoxShape3D GetShape()
    {
        var shape = new BoxShape3D();
        shape.Size = new Vector3(Consts.BlockSize, Consts.BlockSize, Consts.BlockSize);
        // GD.Print(ret);
        return shape;
    }
}

public class BlockRegistry
{
    // 存储所有方块类型
    public static Dictionary<BlockId, (Type, Func<Color>, Func<BoxShape3D>)> BlockTypes { get; } = new();
    public static MethodInfo DefaultGetShaderDataMethod = typeof(IBlockType).GetMethod("GetShaderData");
    public static MethodInfo DefaultGetShapeMethod = typeof(IBlockType).GetMethod("GetShape");

    static BlockRegistry()
    {
        // RegisterBlock<Dirt>();
        var blockTypes = Assembly.GetExecutingAssembly()
                                         .GetTypes()
                                         .Where(t => t.IsClass && !t.IsAbstract && typeof(IBlockType).IsAssignableFrom(t));

        foreach (var type in blockTypes)
        {
            // 构造 RegisterBlock<具体类型>() 的泛型方法并调用
            var method = typeof(BlockRegistry)
                         .GetMethod(nameof(RegisterBlock), BindingFlags.Public | BindingFlags.Static)
                         .MakeGenericMethod(type);
            GD.Print($"Registering block type: {type.Name}");
            method.Invoke(null, null);   // 静态方法，实例参数传 null
        }
    }

    // 注册方块类型
    public static void RegisterBlock<T>() where T : IBlockType
    {
        var getShaderDataMethod = typeof(T).GetMethod("GetShaderData");
        var getShapeMethod = typeof(T).GetMethod("GetShape");
        if (getShaderDataMethod != null && getShapeMethod != null)
        {
            BlockTypes.Add(T.Id,
                (typeof(T), (Func<Color>)Delegate.CreateDelegate(typeof(Func<Color>), getShaderDataMethod),
                    (Func<BoxShape3D>)Delegate.CreateDelegate(typeof(Func<BoxShape3D>), getShapeMethod)));
        }
        else
        {
            BlockTypes.Add(T.Id,
                            (typeof(T), (Func<Color>)Delegate.CreateDelegate(typeof(Func<Color>), DefaultGetShaderDataMethod),
                                (Func<BoxShape3D>)Delegate.CreateDelegate(typeof(Func<BoxShape3D>), DefaultGetShapeMethod)));
        }
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

    // liquid volume
    public float Volume = 0;

    public (int, int)[] FaceTextures = new (int, int)[6];

    public Func<Color> GetShaderData = getShaderDataDelegate;
    public Func<BoxShape3D> GetShape = getShapeDelegate;
}