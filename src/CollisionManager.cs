using Godot;
using System.Collections.Generic;
using Terraria3D;

public partial class CollisionManager : Node
{
    Dictionary<Vector3I, Node3D> LoadedChunks { get; set; } = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void AddCollision(Chunk chunk)
    {
        if (LoadedChunks.ContainsKey(chunk.Pos))
        {
            return;
        }
        var chunkNode = new Node3D();
        AddChild(chunkNode);
        chunkNode.Position = chunk.GetStartPoint();
        LoadedChunks.Add(chunk.Pos, chunkNode);
        for (int i = 0; i < Chunk.X; ++i)
        {
            for (int j = 0; j < Chunk.Y; ++j)
            {
                for (int k = 0; k < Chunk.Z; ++k)
                {
                    if (chunk.Blocks[i, j, k] != null)
                    {
                        var id = chunk.Blocks[i, j, k].BlockId;
                        AddBlockCollision(chunk.Blocks[i, j, k].GetShape(), chunkNode, new Vector3I(i, j, k));
                    }
                }
            }
        }
    }

    public void RemoveCollision(Vector3I chunkPos)
    {
        if (LoadedChunks.ContainsKey(chunkPos))
        {
            RemoveChild(LoadedChunks[chunkPos]);
            LoadedChunks.Remove(chunkPos);
        }
    }

    public void AddBlockCollision(Shape3D shape, Node3D chunkNode, Vector3 localPos)
    {
        var staticbody = new StaticBody3D();
        var collision = new CollisionShape3D();
        collision.Shape = shape;
        staticbody.AddChild(collision);
        chunkNode.AddChild(staticbody);
        staticbody.Position = localPos;
    }

    public void AddBlockCollision(Shape3D shape, Vector3 pos)
    {
        var chunkPos = Utils.GetChunk(pos);
        if (!LoadedChunks.TryGetValue(chunkPos, out var node))
        {
            return;
        }
        var staticbody = new StaticBody3D();
        var collision = new CollisionShape3D();
        collision.Shape = shape;
        staticbody.AddChild(collision);
        staticbody.GlobalPosition = pos;
        node.AddChild(staticbody);
    }
}
