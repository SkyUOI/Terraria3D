using Godot;

namespace Terraria3D;

public class Utils
{
    public static Vector3I GetChunk(Vector3 pos)
    {
        return new Vector3I((int)(pos.X / Chunk.X / Consts.BlockSize), (int)(pos.Y / Chunk.Y / Consts.BlockSize), (int)(pos.Z / Chunk.Z / Consts.BlockSize));
    }
}