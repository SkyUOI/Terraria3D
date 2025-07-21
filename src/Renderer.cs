using System.Collections.Generic;
using Godot;

namespace Terraria3D;

public partial class Renderer : Node3D
{
    [Export]
    Main _main { get; set; }

    Dictionary<Vector3I, MultiMeshInstance3D> RenderedChunks = new();

    public override void _Process(double delta)
    {
    }

    public void RenderChunk(Chunk chunk, bool force = true)
    {
        var hasAddedToSceneTree = RenderedChunks.ContainsKey(chunk.Pos);
        if (!force && hasAddedToSceneTree)
        {
            return;
        }
        if (hasAddedToSceneTree)
        {
            RenderedChunks[chunk.Pos].Multimesh = chunk.GenerateMultiMesh();
        }
        else
        {
            var meshInstance3D = chunk.GenerateMultiMeshInstance3D();
            AddChild(meshInstance3D);
            meshInstance3D.Position = chunk.GetStartPoint();
            // GD.Print($"Rendered Chunk: {meshInstance3D.GlobalPosition}");
            RenderedChunks.Add(chunk.Pos, meshInstance3D);
        }
    }

    public void UnrenderChunk(Vector3I chunkPos)
    {
        if (RenderedChunks.ContainsKey(chunkPos))
        {
            RemoveChild(RenderedChunks[chunkPos]);
            RenderedChunks.Remove(chunkPos);
        }
    }
}