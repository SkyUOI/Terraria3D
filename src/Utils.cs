using Godot;

public class Utils
{
    public static Vector3I GetChunk(Vector3 pos)
    {
        return new Vector3I((int)pos.X / Chunk.X, (int)pos.Y / Chunk.Y, (int)pos.Z / Chunk.Z);
    }
}
