using Godot;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria3D;

public partial class CollisionManager : Node
{
    ConcurrentDictionary<Vector3I, Node3D> LoadedChunks { get; set; } = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public async Task AddCollision(Chunk chunk)
    {
        if (LoadedChunks.ContainsKey(chunk.Pos))
        {
            return;
        }
        var chunkNode = new Node3D();
        // AddChild(chunkNode);
        CallDeferred(Node.MethodName.AddChild, chunkNode);
        CallDeferred(Node3D.MethodName.SetPosition, chunk.GetStartPoint());
        LoadedChunks.TryAdd(chunk.Pos, chunkNode);
        foreach (var i in await chunk.FindVisibleBlocks())
        {
            AddBlockCollision(i.Item2.GetShape(), chunkNode, i.Item1);
        }
    }

    public void RemoveCollision(Vector3I chunkPos)
    {
        if (LoadedChunks.TryRemove(chunkPos, out var chunk))
        {
            RemoveChild(chunk);
        }
    }

    public void AddBlockCollision(Shape3D shape, Node3D chunkNode, Vector3 localPos)
    {
        var staticbody = new StaticBody3D();
        var collision = new CollisionShape3D();
        collision.Shape = shape;
        staticbody.AddChild(collision);
        // chunkNode.AddChild(staticbody);
        chunkNode.CallDeferred(Node.MethodName.AddChild, staticbody);
        // staticbody.Position = localPos;
        staticbody.CallDeferred(Node3D.MethodName.SetPosition, localPos);
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
