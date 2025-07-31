using Godot;
using System;
using Terraria3D;

public partial class OutlineBox : MeshInstance3D
{
    BoxMesh mesh;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mesh = Mesh as BoxMesh;
        mesh.Size = new Vector3(Consts.BlockSize, Consts.BlockSize, Consts.BlockSize);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void Line(Vector3 pos)
    {
        Position = pos;
    }
}
