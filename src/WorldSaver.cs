using Godot;

partial class WorldSaver: VoxelStreamScript
{
    public override int _LoadVoxelBlock(VoxelBuffer outBuffer, Vector3I positionInBlocks, int lod)
    {
        return base._LoadVoxelBlock(outBuffer, positionInBlocks, lod);
    }

    public override void _SaveVoxelBlock(VoxelBuffer buffer, Vector3I positionInBlocks, int lod)
    {
        base._SaveVoxelBlock(buffer, positionInBlocks, lod);
    }
}
