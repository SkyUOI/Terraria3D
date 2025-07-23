using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;


namespace Terraria3D;

public class Chunk(Vector3I pos)
{
    public const int X = 16;
    public const int Z = 16;
    public const int Y = 16;
    public Vector3I Pos = pos;
    public Block[,,] Blocks = new Block[X, Y, Z];

    static Vector3I[] _directs =
    [
        new (0, 0, 1),
        new (0, 0, -1),
        new (1, 0, 0),
        new (-1, 0, 0),
        new (0, 1, 0),
        new (0, -1, 0)
    ];

    public static Mesh UnitMesh = new BoxMesh
    {
        Size = new Vector3(Consts.BlockSize, Consts.BlockSize, Consts.BlockSize)
    };

    public static Transform3D FromBlockPos(Vector3I pos)
    {
        return new Transform3D(Basis.Identity, pos);
    }

    public static bool InLocalChunkPos(Vector3 pos)
    {
        return pos.X >= 0 && pos is { X: < X, Y: >= 0 } and { Y: < Y, Z: >= 0 and < Z };
    }

    public Vector3I GetGlobalChunkPos(Vector3I inChunkPos)
    {
        return new Vector3I(Pos.X * X + inChunkPos.X, Pos.Y * Y + inChunkPos.Y, Pos.Z * Z + inChunkPos.Z);
    }

    public Vector3 ConvertToLocalRealPos(Vector3I pos)
    {
        return new Vector3(pos.X * Consts.BlockSize, pos.Y * Consts.BlockSize, pos.Z * Consts.BlockSize);
    }

    public Vector3 GetLocalPosFromGlobalRealPos(Vector3 pos)
    {
        return new Vector3((int)(pos.X / Consts.BlockSize - Pos.X * X), (int)(pos.Y / Consts.BlockSize - Pos.Y * Y), (int)(pos.Z / Consts.BlockSize - Pos.Z * Z));
    }

    public Vector3I GetLocalPosFromGlobalChunkPos(Vector3 pos)
    {
        return new Vector3I((int)(pos.X - Pos.X * X), (int)(pos.Y - Pos.Y * Y), (int)(pos.Z - Pos.Z * Z));
    }

    public (int, int) HeightRange()
    {
        return (Pos.Y * Y, (Pos.Y + 1) * Y - 1);
    }

    public async Task<MultiMesh> GenerateMultiMesh()
    {
        var multiMesh = new MultiMesh()
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseCustomData = true
        };
        multiMesh.Mesh = UnitMesh;

        var transforms = await FindVisibleBlocks();
        multiMesh.InstanceCount = transforms.Count;
        for (int i = 0; i < transforms.Count; ++i)
        {
            multiMesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, transforms[i].Item1));
            multiMesh.SetInstanceCustomData(i, transforms[i].Item2.GetShaderData());
        }
        return multiMesh;
    }

    public async Task<List<(Vector3, Block)>> FindVisibleBlocks()
    {
        var transforms = await Task.Run(() =>
                {
                    var transforms = new List<(Vector3, Block)>()
                    {
                        Capacity = X * Y * Z
                    };
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
                                    var direct = _directs[l];
                                    var blockPos2 = blockPos + direct;
                                    if (blockPos2.X < 0 || blockPos2.X >= X || blockPos2.Y < 0 || blockPos2.Y >= Y || blockPos2.Z < 0 || blockPos2.Z >= Z)
                                    {
                                        // GD.Print("rendered side block");
                                        // renderFlag = true;
                                        // break;
                                        var nearChunkPos = Pos + direct;
                                        if (!ChunksManager.Chunks.TryGetValue(nearChunkPos, out var nearChunk))
                                        {
                                            continue;
                                        }
                                        var nearBlockPos = nearChunk.GetLocalPosFromGlobalChunkPos(GetGlobalChunkPos(blockPos2));
                                        // GD.Print($"{nearBlockPos}");
                                        if (nearChunk.Blocks[nearBlockPos.X, nearBlockPos.Y, nearBlockPos.Z] == null)
                                        {
                                            renderFlag = true;
                                            break;
                                        }
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
                                    transforms.Add((ConvertToLocalRealPos(blockPos), block));
                                }
                            }
                        }
                    }
                    return transforms;
                });
        return transforms;
    }

    public async Task<MultiMeshInstance3D> GenerateMultiMeshInstance3D()
    {
        var multiMeshInstance3D = new MultiMeshInstance3D();
        multiMeshInstance3D.Multimesh = await GenerateMultiMesh();
        multiMeshInstance3D.MaterialOverride = RenderShaderResources.Material;
        return multiMeshInstance3D;
    }

    public Vector3 GetRealStartPoint()
    {
        return new Vector3(Pos.X * X * Consts.BlockSize, Pos.Y * Y * Consts.BlockSize, Pos.Z * Z * Consts.BlockSize);
    }
}

public class ChunksManager
{
    public static ConcurrentDictionary<Vector3I, Chunk> Chunks { get; set; } = new();

    public static async Task CheckAndLoadChunk(string worldPath, Vector3 pos)
    {
        var chunkPos = Utils.GetChunk(pos);
        if (Chunks.ContainsKey(chunkPos))
        {
            return;
        }
        await LoadChunk(worldPath, chunkPos);
        // GD.Print($"Loaded Chunk: {dir}");
    }

    public static async Task<Chunk> LoadChunk(string worldPath, Vector3I chunkPos)
    {
        var chunkPath = WorldFile.GetChunksPath(worldPath).PathJoin(WorldFile.GetChunkFileName(chunkPos));
        if (File.Exists(chunkPath))
        {
            await using var f = File.OpenRead(chunkPath);
            var chunkData = JsonSerializer.Deserialize<Chunk>(f);
            Chunks.TryAdd(chunkPos, chunkData);
            return chunkData;
        }
        var chunk = new Chunk(chunkPos);
        await WorldGeneration.GenerateChunk(chunk);
        Chunks.TryAdd(chunkPos, chunk);
        return chunk;
    }

    public static void UnloadChunk(Vector3I chunkPos)
    {
        Chunks.TryRemove(chunkPos, out _);
    }

    public static bool BlockExists(Vector3 pos)
    {
        var chunkPos = Utils.GetChunk(pos);
        if (!Chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            return false;
        }
        var blockPos = chunk.GetLocalPosFromGlobalRealPos(pos);
        if (!Chunk.InLocalChunkPos(blockPos))
        {
            return false;
        }
        return chunk.Blocks[(int)blockPos.X, (int)blockPos.Y, (int)blockPos.Z] != null;
    }
}
