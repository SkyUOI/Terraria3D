using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace Terraria3D;

public partial class Renderer : Node3D
{
    [Export]
    Main _main { get; set; }

    ConcurrentDictionary<Vector3I, MultiMeshInstance3D> RenderedChunks = new();

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
            renderedChunk.CallDeferred(MultiMeshInstance3D.MethodName.SetMultimesh, await chunk.GenerateMultiMesh());
        }
        else
        {
            var meshInstance3D = await chunk.GenerateMultiMeshInstance3D();
            CallDeferred(Node.MethodName.AddChild, meshInstance3D);
            meshInstance3D.CallDeferred(MultiMeshInstance3D.MethodName.SetPosition, chunk.GetStartPoint());
            // GD.Print($"Rendered Chunk: {meshInstance3D.GlobalPosition}");
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
    public static Shader LoadTexture { get; set; } = GD.Load<Shader>("res://src/ChunkMesh.gdshader");
}
