using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;


namespace Terraria3D;

record Triangle(int[] Vertex, int[] UVs);

record Face(params Triangle[] Triangles);

public class Chunk(Vector3I pos)
{
    public const int X = 16;
    public const int Z = 16;
    public const int Y = 16;
    public Vector3I Pos = pos;
    public Block[,,] Blocks = new Block[X, Y, Z];

    static Vector3I[] _directs =
    [
        Vector3I.Up,
        Vector3I.Down,
        Vector3I.Right,
        Vector3I.Left,
        Vector3I.Back,
        Vector3I.Forward,
    ];

    static Vector3[] _vertex =
    [
        new (0, 0, 0),
        new (1, 0, 0),
        new (1, 1, 0),
        new (0, 1, 0),
        new (0, 0, 1),
        new (1, 0, 1),
        new (1, 1, 1),
        new (0, 1, 1)
    ];

    static Vector2[] _uvs =
    [
        new (0, 0),
        new (1, 0),
        new (1, 1),
        new (0, 1)
    ];

    static Face[] _facesPosition =
    [
        new (
            new([3,2,7], [0, 3, 1]),
            new([2,6,7], [3, 2, 1])
        ),
        new(
            new ([4,5,0], [0, 3, 1]),
            new ([5, 1, 0], [3, 2, 1])
        ),
        new(
            new ([6, 2, 5], [0,3,1]),
            new ([2,1,5], [3, 2, 1])
        ),
        new(
            new ([3,7,0], [0, 3, 1]),
            new ([7,4,0], [3, 2, 1])
        ),
        new(
            new ([7,6,4], [0, 3, 1]),
            new ([6,5,4], [3, 2, 1])
        ),
        new(
            new ([2, 3, 1], [0,3,1]),
            new ([3, 0, 1], [3, 2, 1])
        ),
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

    public Vector3I GetGlobalChunkPosFromLocalChunkPos(Vector3I inChunkPos)
    {
        return new Vector3I(Pos.X * X + inChunkPos.X, Pos.Y * Y + inChunkPos.Y, Pos.Z * Z + inChunkPos.Z);
    }

    public Vector3 ConvertLocalChunkPosToLocalRealPos(Vector3I pos)
    {
        return new Vector3(pos.X * Consts.BlockSize, pos.Y * Consts.BlockSize, pos.Z * Consts.BlockSize);
    }

    public Vector3 ConvertGlobalChunkPosToGlobalRealPos(Vector3I pos)
    {
        return new Vector3(pos.X * Consts.BlockSize, pos.Y * Consts.BlockSize, pos.Z * Consts.BlockSize);
    }

    public Vector3 ConvertLocalChunkPosToGlobalRealPos(Vector3I pos)
    {
        return new Vector3((Pos.X * X + pos.X) * Consts.BlockSize, (Pos.Y * Y + pos.Y) * Consts.BlockSize, (Pos.Z * Z + pos.Z) * Consts.BlockSize);
    }

    public Vector3I GetLocalChunkPosFromGlobalRealPos(Vector3 pos)
    {
        return new Vector3I((int)(pos.X / Consts.BlockSize - Pos.X * X), (int)(pos.Y / Consts.BlockSize - Pos.Y * Y), (int)(pos.Z / Consts.BlockSize - Pos.Z * Z));
    }

    public Vector3I GetLocalChunkPosFromGlobalChunkPos(Vector3 pos)
    {
        return new Vector3I((int)(pos.X - Pos.X * X), (int)(pos.Y - Pos.Y * Y), (int)(pos.Z - Pos.Z * Z));
    }

    public (int, int) HeightRange()
    {
        return (Pos.Y * Y, (Pos.Y + 1) * Y - 1);
    }

    public Block GetBlock(Vector3I pos)
    {
        return Blocks[pos.X, pos.Y, pos.Z];
    }

    public async Task<Mesh> GenerateMesh()
    {
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        var faces = await FindVisibleFaces();
        foreach (var face in faces)
        {
            for (int i = 0; i < face.Item1.Triangles.Length; ++i)
            {
                var triangle = face.Item1.Triangles[i];
                for (int j = 0; j < 3; ++j)
                {
                    surfaceTool.SetUV(_uvs[triangle.UVs[j]]);
                    // surfaceTool.SetColor(face.Item2.GetShaderData());
                    surfaceTool.AddVertex(_vertex[triangle.Vertex[j]] * Consts.BlockSize + face.Item3);
                }
            }
        }
        surfaceTool.GenerateNormals();
        var mesh = surfaceTool.Commit();
        // GD.Print($"mesh: {mesh} faces: {faces.Count}");
        return mesh;
    }

    private async Task<List<(Face, Block, Vector3)>> FindVisibleFaces()
    {
        var transforms = await Task.Run(() =>
                {
                    var transforms = new List<(Face, Block, Vector3)>()
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
                                var realPos = ConvertLocalChunkPosToLocalRealPos(blockPos);
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
                                        var nearBlockPos = nearChunk.GetLocalChunkPosFromGlobalChunkPos(GetGlobalChunkPosFromLocalChunkPos(blockPos2));
                                        // GD.Print($"{nearBlockPos}");
                                        if (nearChunk.Blocks[nearBlockPos.X, nearBlockPos.Y, nearBlockPos.Z] == null)
                                        {
                                            transforms.Add((_facesPosition[l], block, realPos));
                                            // GD.Print("fk");
                                        }
                                    }
                                    else if (Blocks[blockPos2.X, blockPos2.Y, blockPos2.Z] == null)
                                    {
                                        transforms.Add((_facesPosition[l], block, realPos));
                                        // GD.Print("fk");
                                    }
                                }
                            }
                        }
                    }
                    return transforms;
                });
        // GD.Print("Found all faces");
        return transforms;
    }

    public async Task<MeshInstance3D> GenerateMeshInstance3D()
    {
        var MeshInstance3D = new MeshInstance3D();
        MeshInstance3D.Mesh = await GenerateMesh();
        MeshInstance3D.MaterialOverride = RenderShaderResources.Material;
        return MeshInstance3D;
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
        return GetBlock(pos) != null;
    }

    public static (Chunk, Vector3I) LocateBlock(Vector3 pos)
    {
        var chunkPos = Utils.GetChunk(pos);
        if (!Chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            return (null, Vector3I.Zero);
        }
        var blockPos = chunk.GetLocalChunkPosFromGlobalRealPos(pos);
        if (!Chunk.InLocalChunkPos(blockPos))
        {
            return (null, Vector3I.Zero);
        }
        return (chunk, blockPos);
    }

    public static Block GetBlock(Vector3 pos)
    {
        var (chunk, blockPos) = LocateBlock(pos);
        if (chunk == null)
        {
            return null;
        }
        return chunk.GetBlock(blockPos);
    }

    /// <summary>
    /// Retrieves the real-world position of a block based on its given position.
    /// </summary>
    /// <param name="pos">The position of the block to locate.</param>
    /// <returns>The real-world position of the block if found; otherwise, null.</returns>
    public static Vector3? GetBlockRealPos(Vector3 pos)
    {
        var (chunk, blockPos) = LocateBlock(pos);
        if (chunk == null)
        {
            return null;
        }
        return chunk.ConvertLocalChunkPosToGlobalRealPos(blockPos);
    }
}
