using Godot;
using System;
using System.Collections.Generic;

public partial class ChunkMeshGenerator : Node
{
	public class Surface
	{
		public List<Vector3I> Vertices = new List<Vector3I>();
		public List<int> Indices = new List<int>();
		public Vector3 Normal;
		public int BlockId;

		public Surface(List<Vector3I> vertices, List<int> indices, Vector3 normal)
		{
			Vertices = vertices;
			Indices = indices;
			Normal = normal;
		}
	}

	public static readonly Vector3I[] Normals = new Vector3I[]
	{
		new Vector3I(1, 0, 0),
		new Vector3I(-1, 0, 0),
		new Vector3I(0, 1, 0),
		new Vector3I(0, -1, 0),
		new Vector3I(0, 0, 1),
		new Vector3I(0, 0, -1)
	};

	public static readonly Vector2[] UVs = new Vector2[]
	{
		new Vector2(0, 0),
		new Vector2(1, 0),
		new Vector2(1, 1),
		new Vector2(0, 1)
	};

	public static Dictionary<Vector3I, bool[]> FindBlockSurfaces(
		Chunk chunk,
		Chunk xPosChunk = null,
		Chunk xNegChunk = null,
		Chunk yPosChunk = null,
		Chunk yNegChunk = null,
		Chunk zPosChunk = null,
		Chunk zNegChunk = null)
	{
		var exposedBlocks = new Dictionary<Vector3I, bool[]>();
		var adjacentChunks = new Chunk[]
		{
			xPosChunk, xNegChunk, yPosChunk, yNegChunk, zPosChunk, zNegChunk
		};

		for (int z = 0; z < chunk.ChunkSize.Z; z++)
		{
			for (int y = 0; y < chunk.ChunkSize.Y; y++)
			{
				for (int x = 0; x < chunk.ChunkSize.X; x++)
				{
					Vector3I blockPos = new Vector3I(x, y, z);

					if (chunk.GetBlock(blockPos) == -1)
						continue;

					bool[] exposedSurfaces = new bool[Normals.Length];
					bool hasExposed = false;

					for (int i = 0; i < Normals.Length; i++)
					{
						Vector3I dir = Normals[i];
						Vector3I adjacentPos = blockPos + dir;
						int adjacentBlock;

						if (chunk.IsInChunk(adjacentPos))
						{
							adjacentBlock = chunk.GetBlock(adjacentPos);
						}
						else
						{
							Vector3I wrapped = Chunk.WrapToChunk(adjacentPos, chunk.ChunkSize);
							adjacentBlock = adjacentChunks[i]?.GetBlock(wrapped) ?? -1;
						}

						if (adjacentBlock == -1)
						{
							exposedSurfaces[i] = true;
							hasExposed = true;
						}
					}

					if (hasExposed)
						exposedBlocks[blockPos] = exposedSurfaces;
				}
			}
		}
		return exposedBlocks;
	}

	public static List<Surface> GetSurfaceVectors(Chunk chunk, Dictionary<Vector3I, bool[]> exposed)
	{
		var blockSurfaces = new Dictionary<Vector3I, bool[]>();
		foreach (var kv in exposed)
			blockSurfaces[kv.Key] = (bool[])kv.Value.Clone();

		int maxDim = Math.Max(chunk.ChunkSize.X, Math.Max(chunk.ChunkSize.Y, chunk.ChunkSize.Z));
		var surfaces = new List<Surface>();

		while (blockSurfaces.Count > 0)
		{
			Vector3I startPos = new List<Vector3I>(blockSurfaces.Keys)[0];
			int dirIndex = 0;
			Vector3I normal = new Vector3I();

			for (int i = 0; i < Normals.Length; i++)
			{
				if (blockSurfaces[startPos][i])
				{
					dirIndex = i;
					normal = Normals[i];
					break;
				}
			}

			Vector3I locXMove = (Vector3I)Embed2DInPlane(new Vector2(1, 0), normal);
			Vector3I locYMove = (Vector3I)Embed2DInPlane(new Vector2(0, 1), normal);

			Vector3I minPos = startPos;
			bool moveX = true;
			bool failedLast = false;

			for (int i = 0; i < maxDim * maxDim; i++)
			{
				Vector3I newPos = moveX ? minPos - locXMove : minPos - locYMove;
				bool hasSurface = blockSurfaces.ContainsKey(newPos) && blockSurfaces[newPos][dirIndex];

				if (hasSurface)
				{
					minPos = newPos;
					failedLast = false;
				}
				else if (!failedLast)
				{
					failedLast = true;
					moveX = !moveX;
				}
				else break;
			}

			int maxX = -1;
			int maxY = -1;
			Vector3I maxPos = new Vector3I();

			for (int y = 0; y <= maxDim; y++)
			{
				bool fullRow = false;
				for (int x = 0; x <= maxDim; x++)
				{
					Vector3I np = minPos + locXMove * x + locYMove * y;
					bool hasSurface = blockSurfaces.ContainsKey(np) && blockSurfaces[np][dirIndex];

					bool firstRow = maxX == -1;
					bool lastCol = x == maxX;

					if (hasSurface && !lastCol)
						continue;
					else if (!hasSurface && firstRow)
					{
						maxX = x - 1;
						fullRow = true;
						maxPos = np - locXMove;
						break;
					}
					else if (hasSurface && lastCol)
					{
						fullRow = true;
						maxPos = np;
						break;
					}
					else
					{
						fullRow = false;
						break;
					}
				}
				if (!fullRow)
				{
					maxY = y - 1;
					break;
				}
			}

			for (int x = 0; x <= maxX; x++)
			{
				for (int y = 0; y <= maxY; y++)
				{
					Vector3I rp = minPos + locXMove * x + locYMove * y;
					blockSurfaces[rp][dirIndex] = false;
					bool stillHas = false;
					foreach (bool b in blockSurfaces[rp]) if (b) stillHas = true;
					if (!stillHas) blockSurfaces.Remove(rp);
				}
			}

			List<Vector3I> verts = new List<Vector3I>();
			List<int> inds = new List<int>();

			Vector3 disp = (Vector3)normal * 0.5f + new Vector3(Mathf.Abs(normal.X), Mathf.Abs(normal.Y), Mathf.Abs(normal.Z)) * 0.5f;
			Vector3I displacement = (Vector3I)disp;

			if (dirIndex == 0)
				displacement += new Vector3I(0, 1, 0);
			else if (dirIndex == 3 || dirIndex == 4)
				displacement += new Vector3I(1, 0, 0);

			Vector3I basePos = minPos + displacement;

			Vector3I c1 = (Vector3I)Embed2DInPlane(new Vector2I(0, 0), normal) + basePos;
			Vector3I c2 = (Vector3I)Embed2DInPlane(new Vector2I(0, maxY + 1), normal) + basePos;
			Vector3I c3 = (Vector3I)Embed2DInPlane(new Vector2I(maxX + 1, maxY + 1), normal) + basePos;
			Vector3I c4 = (Vector3I)Embed2DInPlane(new Vector2I(maxX + 1, 0), normal) + basePos;

			verts.Add(c1);
			verts.Add(c2);
			verts.Add(c3);
			verts.Add(c4);

			inds.AddRange(new int[] { 0, 1, 2, 2, 3, 0 });

			surfaces.Add(new Surface(verts, inds, normal));
		}

		return surfaces;
	}

	public static Vector3 Embed2DInPlane(Vector2 v, Vector3 n)
	{
		Vector3 a;
		n = n.Normalized();
		if (Mathf.Abs(n.Z) < 0.9f)
			a = new Vector3(0, 0, 1);
		else
			a = new Vector3(0, 1, 0);

		Vector3 t1 = (a - n.Dot(a) * n).Normalized();
		Vector3 t2 = n.Cross(t1);
		return v.X * t1 + v.Y * t2;
	}

	public static Mesh BuildChunkMesh(
		Chunk chunk,
		Material mat,
		Chunk xPos = null,
		Chunk xNeg = null,
		Chunk yPos = null,
		Chunk yNeg = null,
		Chunk zPos = null,
		Chunk zNeg = null)
	{
		var exposed = FindBlockSurfaces(chunk, xPos, xNeg, yPos, yNeg, zPos, zNeg);
		var surfaces = GetSurfaceVectors(chunk, exposed);

		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		st.SetMaterial(mat);

		for (int i = 0; i < surfaces.Count; i++)
		{
			var s = surfaces[i];
			st.SetNormal(s.Normal);

			for (int j = 0; j < 4; j++)
			{
				st.SetUV(UVs[j]);
				st.AddVertex(s.Vertices[j]);
			}

			foreach (int ind in s.Indices)
				st.AddIndex(i * 4 + ind);
		}

		return st.Commit();
	}
}
