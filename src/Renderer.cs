using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Resolvers;
using Godot;

namespace Terraria3D;

public partial class Renderer : Node3D
{
    [Export]
    Main _main { get; set; }

    ConcurrentDictionary<Vector3I, MeshInstance3D> RenderedChunks = new();

    public override void _Process(double delta)
    {
    }

    public async Task RenderChunk(Chunk chunk, bool force = true)
    {
        // GD.Print($"Rendering Chunk: {chunk.Pos}");
        var hasAddedToSceneTree = RenderedChunks.TryGetValue(chunk.Pos, out var renderedChunk);
        if (!force && hasAddedToSceneTree)
        {
            return;
        }
        if (hasAddedToSceneTree)
        {
            renderedChunk.CallDeferred(MeshInstance3D.MethodName.SetMesh, await chunk.GenerateMesh());
        }
        else
        {
            // GD.Print("Rendering Chunk");
            var meshInstance3D = await chunk.GenerateMeshInstance3D();
            CallDeferred(Node.MethodName.AddChild, meshInstance3D);
            meshInstance3D.CallDeferred(Node3D.MethodName.SetPosition, chunk.GetRealStartPoint());
            // GD.Print($"Rendered Chunk: {chunk.Pos}");
            RenderedChunks.TryAdd(chunk.Pos, meshInstance3D);
        }
    }

    public void UnrenderChunk(Vector3I chunkPos)
    {
        if (RenderedChunks.TryRemove(chunkPos, out var child))
        {
            RemoveChild(child);
        }
    }
}

public class RenderShaderResources
{
    public static Shader LoadTexture { get; set; }
    public static Texture2D Atlas { get; set; }

    public static ShaderMaterial Material { get; set; } = new();

    static RenderShaderResources()
    {
        LoadTexture ??= GD.Load<Shader>("res://src/ChunkMesh.gdshader");
        Atlas ??= GD.Load<Texture2D>("res://resources/tiles/Atlas.png");
        Material.Shader = LoadTexture;
        Material.SetShaderParameter("atlas", Atlas);
    }

    public static void Preload()
    { }
}
