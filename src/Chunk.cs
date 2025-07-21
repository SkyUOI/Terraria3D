using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace Terraria3D;

public class Chunk(Vector3I pos)
{
    public const int X = 16;
    public const int Z = 16;
    public const int Y = 16;
    public Vector3I Pos = pos;
    public Block[,,] Blocks = new Block[X, Y, Z];

    static Vector3I[] directs =
    [
        new Vector3I(0, 0, 1),
        new Vector3I(0, 0, -1),
        new Vector3I(1, 0, 0),
        new Vector3I(-1, 0, 0),
        new Vector3I(0, 1, 0),
        new Vector3I(0, -1, 0)
    ];

    public static bool LocalPosInChunk(Vector3 pos)
    {
        return pos.X >= 0 && pos.X < X && pos.Y >= 0 && pos.Y < Y && pos.Z >= 0 && pos.Z < Z;
    }

    public Vector3 GetGlobalPos(Vector3I inChunkPos)
    {
        return new Vector3(Pos.X * X + inChunkPos.X, Pos.Y * Y + inChunkPos.Y, Pos.Z * Z + inChunkPos.Z);
    }

    public Vector3 GetLocalPos(Vector3 pos)
    {
        return new Vector3((int)(pos.X - Pos.X * X), (int)(pos.Y - Pos.Y * Y), (int)(pos.Z - Pos.Z * Z));
    }

    public (int, int) HeightRange()
    {
        return (Pos.Y, Pos.Y + Y - 1);
    }

    public MultiMesh GenerateMultiMesh()
    {
        var multiMesh = new MultiMesh();
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.Mesh = new BoxMesh
        {
            Size = new(2, 2, 2)
        };
        var transforms = new List<(Vector3, Block)>();
        for (int i = 0; i < X; ++i)
        {
            for (int j = 0; j < Y; ++j)
            {
                for (int k = 0; k < Z; ++k)
                {
                    var block = Blocks[i, j, k];
                    if (block == null)
                    {
                        continue;
                    }
                    var blockPos = new Vector3I(i, j, k);
                    bool renderFlag = false;
                    for (int l = 0; l < 6; ++l)
                    {
                        var direct = directs[l];
                        var blockPos2 = blockPos + direct;
                        if (blockPos2.X < 0 || blockPos2.X >= X || blockPos2.Y < 0 || blockPos2.Y >= Y || blockPos2.Z < 0 || blockPos2.Z >= Z)
                        {
                            continue;
                        }
                        if (Blocks[blockPos2.X, blockPos2.Y, blockPos2.Z] == null)
                        {
                            renderFlag = true;
                            break;
                        }
                    }
                    if (renderFlag)
                    {
                        transforms.Add((blockPos, block));
                    }
                }
            }
        }
        multiMesh.UseCustomData = true;
        multiMesh.InstanceCount = transforms.Count;
        for (int i = 0; i < transforms.Count; ++i)
        {
            multiMesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, transforms[i].Item1));
            multiMesh.SetInstanceCustomData(i, BlockRegistry.GetShaderData(transforms[i].Item2.BlockId));
        }
        return multiMesh;
    }

    public MultiMeshInstance3D GenerateMultiMeshInstance3D()
    {
        var multiMeshInstance3D = new MultiMeshInstance3D();
        multiMeshInstance3D.Multimesh = GenerateMultiMesh();
        var mat = new ShaderMaterial();
        mat.Shader = GD.Load<Shader>("res://src/ChunkMesh.gdshader");
        mat.SetShaderParameter("atlas", GD.Load<Texture2D>("res://resources/tiles/Atlas.png"));
        // TODO: read dynamically
        mat.SetShaderParameter("atlas_size", new Vector2(1024, 1024));
        multiMeshInstance3D.MaterialOverride = mat;
        return multiMeshInstance3D;
    }

    public Vector3 GetStartPoint()
    {
        return new Vector3(Pos.X * X, Pos.Y * Y, Pos.Z * Z);
    }
}

public class ChunksManager
{
    public ConcurrentDictionary<Vector3I, Chunk> Chunks { get; set; } = new();

    public void CheckAndLoadChunk(string worldPath, Vector3 pos)
    {
        var chunkPos = Utils.GetChunk(pos);
        if (Chunks.ContainsKey(chunkPos))
        {
            return;
        }
        LoadChunk(worldPath, chunkPos);
        // GD.Print($"Loaded Chunk: {dir}");
    }

    public Chunk LoadChunk(string worldPath, Vector3I chunkPos)
    {
        var chunkPath = WorldFile.GetChunksPath(worldPath).PathJoin(WorldFile.GetChunkFIleName(chunkPos));
        if (File.Exists(chunkPath))
        {
            using var f = File.OpenRead(chunkPath);
            var chunkData = JsonSerializer.Deserialize<Chunk>(f);
            Chunks.TryAdd(chunkPos, chunkData);
            return chunkData;
        }
        var chunk = new Chunk(chunkPos);
        WorldGeneration.GenerateChunk(chunk);
        Chunks.TryAdd(chunkPos, chunk);
        return chunk;
    }

    public void UnloadChunk(Vector3I chunkPos)
    {
        Chunks.TryRemove(chunkPos, out _);
    }

    public bool BlockExists(Vector3 pos)
    {
        var chunkPos = Utils.GetChunk(pos);
        if (!Chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            return false;
        }
        var blockPos = chunk.GetLocalPos(pos);
        if (!Chunk.LocalPosInChunk(blockPos))
        {
            return false;
        }
        return chunk.Blocks[(int)blockPos.X, (int)blockPos.Y, (int)blockPos.Z] != null;
    }
}
