using System.Linq;
using Godot;

namespace Terraria3D.block.NormalBlock;

public class Dirt : IBlock
{
    public static BlockId Id => BlockId.Dirt;

    public static Color GetShaderData()
    {
        // GD.Print(JsonSerializer.Serialize(SharedData.AtlasData, new JsonSerializerOptions { WriteIndented = true }));
        // GD.Print(JsonSerializer.Serialize(SharedData.AtlasData.Atlas, new JsonSerializerOptions { WriteIndented = true }));
        // GD.Print(File.ReadAllText(ProjectSettings.GlobalizePath("res://resources/tiles/atlas_tiles.json")));
        return (Color)SharedData.AtlasData.Atlas["Tiles_0"].First();
    }

    public static BoxShape3D GetShape()
    {
        var shape = new BoxShape3D();
        shape.Size = new Vector3(Consts.BlockSize, Consts.BlockSize, Consts.BlockSize);
        // GD.Print(ret);
        return shape;
    }
}