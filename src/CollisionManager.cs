using Godot;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria3D;

public partial class CollisionManager : Node
{
    ConcurrentDictionary<Vector3I, StaticBody3D> LoadedChunks { get; set; } = new();

    static Vector3[] ShapeFaces = Chunk.UnitMesh.GetFaces();

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
        var visibleChunks = await chunk.FindVisibleBlocks();
        var allFaces = new List<Vector3>()
        {
            Capacity = visibleChunks.Count * ShapeFaces.Length
        };
        for (var i = 0; i < visibleChunks.Count; i++)
        {
            foreach (var j in ShapeFaces)
            {
                allFaces.Add(j + visibleChunks[i].Item1);
            }
        }
        if (allFaces.Count == 0)
        {
            return;
        }
        var chunkNode = new StaticBody3D();
        // AddChild(chunkNode);
        CallDeferred(Node.MethodName.AddChild, chunkNode);
        chunkNode.CallDeferred(Node3D.MethodName.SetPosition, chunk.GetStartPoint());

        var shape = new ConcavePolygonShape3D();
        // GD.Print($"Mesh Faces: {allFaces.Count}");
        shape.SetFaces(allFaces.ToArray());
        var collision = new CollisionShape3D();
        collision.Shape = shape;
        // chunkNode.AddChild(collision);
        chunkNode.CallDeferred(Node.MethodName.AddChild, collision);

        LoadedChunks.TryAdd(chunk.Pos, chunkNode);
    }

    public void RemoveCollision(Vector3I chunkPos)
    {
        if (LoadedChunks.TryRemove(chunkPos, out var chunk))
        {
            RemoveChild(chunk);
        }
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
