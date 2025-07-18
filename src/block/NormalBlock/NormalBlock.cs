using System;
using Godot;

public class Dirt : IBlock
{
    public static BlockId Id => BlockId.Dirt;

    [Export]
    public static AtlasTexture texture = GD.Load<AtlasTexture>("res://resources/tiles/Tile_0.tres");

    public static Mesh GetMesh()
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        var atlas_size = texture.Atlas.GetSize();
        GD.Print(atlas_size);
        var region_pos = texture.Region.Position;
        GD.Print(region_pos);
        var region_size = texture.Region.Size;
        GD.Print(region_size);
        var uv_offset = new Vector2(region_pos.X / atlas_size.X, region_pos.Y / atlas_size.Y);
        var uv_scale = new Vector2(region_size.X / atlas_size.X, region_size.Y / atlas_size.Y);
        Vector3[,] vertices = {
        // 前面
        {new Vector3(-1,1,-1), new Vector3(1,1,-1), new Vector3(1,-1,-1), new Vector3(-1,-1,-1) },
        // 后面
        {new Vector3(-1,1,1), new Vector3(1,1,1), new Vector3(1,-1,1), new Vector3(-1,-1,1) },
        // 左面
        {new Vector3(-1,1,1), new Vector3(-1,1,-1), new Vector3(-1,-1,-1), new Vector3(-1,-1,1) },
        // 右面
        {new Vector3(1,1,1), new Vector3(1,1,-1), new Vector3(1,-1,-1), new Vector3(1,-1,1) },
        // 上面
        {new Vector3(-1,1,1), new Vector3(1,1,1), new Vector3(1,1,-1), new Vector3(-1,1,-1) },
        // 下面
        {new Vector3(-1,-1,1), new Vector3(1,-1,1), new Vector3(1,-1,-1), new Vector3(-1,-1,-1) },
        };
        Vector2[] uvs = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] *= uv_scale;
            uvs[i] += uv_offset;
        }
        for (int i = 0; i < 6; ++i)
        {
            // 三角1
            st.SetUV(uvs[0]); st.AddVertex(vertices[i, 0]);
            st.SetUV(uvs[1]); st.AddVertex(vertices[i, 1]);
            st.SetUV(uvs[2]); st.AddVertex(vertices[i, 2]);
            // 三角2
            st.SetUV(uvs[2]); st.AddVertex(vertices[i, 2]);
            st.SetUV(uvs[3]); st.AddVertex(vertices[i, 3]);
            st.SetUV(uvs[0]); st.AddVertex(vertices[i, 0]);
        }
        var mesh = st.Commit();
        var mat = new StandardMaterial3D();
        mat.AlbedoTexture = texture.Atlas;
        mesh.SurfaceSetMaterial(0, mat);
        return mesh;
    }
}
