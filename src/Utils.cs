using Godot;

public class Utils
{
    public static Vector2 GetChunk(Vector3 pos)
    {
        return new Vector2((int)pos.X / 16, (int)pos.Z / 16);
    }
}
