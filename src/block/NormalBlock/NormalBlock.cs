using Godot;

namespace Terraria3D.block.NormalBlock;

public class BlockUtil
{
    public static Mesh GetMesh(AtlasTexture texture)
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        var atlasSize = texture.Atlas.GetSize();
        // GD.Print(atlas_size);
        var regionPos = texture.Region.Position;
        // GD.Print(region_pos);
        var regionSize = texture.Region.Size;
        // GD.Print(region_size);
        var uvOffset = new Vector2(regionPos.X / atlasSize.X, regionPos.Y / atlasSize.Y);
        var uvScale = new Vector2(regionSize.X / atlasSize.X, regionSize.Y / atlasSize.Y);
        Vector3[,] vertices = {
            // 前面
            {new(-1,1,-1), new(1,1,-1), new(1,-1,-1), new(-1,-1,-1) },
            // 后面
            {new(-1,1,1), new(-1,-1,1), new(1,-1,1), new(1,1,1) },
            // 左面
            {new(-1,1,1), new(-1,1,-1), new(-1,-1,-1), new(-1,-1,1) },
            // 右面
            {new(1,1,1), new(1,-1,1), new(1,-1,-1), new(1,1,-1) },
            // 上面
            {new(-1,1,1), new(1,1,1), new(1,1,-1), new(-1,1,-1) },
            // 下面
            {new(-1,-1,1), new(-1,-1,-1), new(1,-1,-1), new(1,-1,1) },
        };
        Vector2[] uvs = [new(0, 0), new(1, 0), new(1, 1), new(0, 1)];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] *= uvScale;
            uvs[i] += uvOffset;
        }
        for (int i = 0; i < 6; ++i)
        {
            // 三角1
            st.SetUV(uvs[0]); st.AddVertex(vertices[i, 0]);
            st.SetUV(uvs[1]); st.AddVertex(vertices[i, 1]);
            st.SetUV(uvs[2]); st.AddVertex(vertices[i, 2]);
            // 三角2
            st.SetUV(uvs[0]); st.AddVertex(vertices[i, 0]);
            st.SetUV(uvs[2]); st.AddVertex(vertices[i, 2]);
            st.SetUV(uvs[3]); st.AddVertex(vertices[i, 3]);
        }
        var mesh = st.Commit();
        var mat = new StandardMaterial3D();
        mat.AlbedoTexture = texture.Atlas;
        mat.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
        mesh.SurfaceSetMaterial(0, mat);
        return mesh;
    }
}

public class Dirt : IBlock
{
    public static BlockId Id => BlockId.Dirt;

    [Export]
    public static AtlasTexture Texture = GD.Load<AtlasTexture>("res://resources/tiles/Tile_0.tres");

    public static Mesh GetMesh()
    {
        return BlockUtil.GetMesh(Texture);
    }

    public static Godot.Collections.Array GetShape()
    {
        var shape = new BoxShape3D();
        shape.Size = GetMesh().GetAabb().Size;
        Variant[] ls = [shape, Transform3D.Identity];
        var ret = new Godot.Collections.Array(ls);
        // GD.Print(ret);
        return ret;
    }
}