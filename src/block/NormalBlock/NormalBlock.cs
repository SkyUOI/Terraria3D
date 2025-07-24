using System.Linq;
using Godot;

namespace Terraria3D.block.NormalBlock;

public class Dirt : IBlockType
{
    public static BlockId Id => BlockId.Dirt;

    public static int NormalTextureId = 0;

    public static Color GetNormalFaceTexture()
    {
        return (Color)SharedData.AtlasData.Atlas[0].First();
    }
}