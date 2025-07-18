using Godot;

public class Utils
{
    public static Vector2I GetChunk(Vector3 pos)
    {
        return new Vector2I((int)pos.X / 16, (int)pos.Z / 16);
    }
}
