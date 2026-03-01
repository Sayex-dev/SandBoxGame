using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


public class ModuleMeshGenerator
{
	public class Surface
	{
		public List<Vector3I> Vertices = new List<Vector3I>();
		public List<int> Indices = new List<int>();
		public Vector3I Normal;
		public Vector2 SurfaceBlockSpan;
		public Direction Dir;
		public BlockFace BlockFace;

		public Surface(
			List<Vector3I> vertices,
			List<int> indices,
			Vector3I normal,
			Vector2 surfaceBlockSpan,
			Direction dir,
			BlockFace blockFace
		)
		{
			Vertices = vertices;
			Indices = indices;
			Normal = normal;
			SurfaceBlockSpan = surfaceBlockSpan;
			Dir = dir;
			BlockFace = blockFace;
		}
	}

	public static readonly Vector2[] UVs =
	{
		new Vector2(0, 0),
		new Vector2(1, 0),
		new Vector2(1, 1),
		new Vector2(0, 1)
	};

	public static List<Surface> GetSurfaceVectors(Module module, ExposedModuleSurfaceCache cache, BlockStore store)
	{
		int moduleSize = module.ModuleSize;
		var surfaces = new List<Surface>();

		foreach (var kvp in cache.ExposedSurfaces)
		{
			Direction dir = kvp.Key;
			ICollection<ModuleGridPos> surfacePositions = kvp.Value.ToHashSet();
			List<Surface> newSurfaces = FindDirSurfaces(moduleSize, dir, module, surfacePositions, store);
			surfaces.AddRange(newSurfaces);
		}

		return surfaces;
	}

	private static List<Surface> FindDirSurfaces(
		int moduleSize,
		Direction dir,
		Module module,
		ICollection<ModuleGridPos> surfacePositions,
		BlockStore store
	)
	{
		List<Surface> surfaces = [];
		HashSet<ModuleGridPos> remaining = surfacePositions.ToHashSet();

		Vector3I normal = (Vector3I)DirectionTools.GetWorldDirVec(dir);
		Vector3I locXMove = (Vector3I)Embed2DInPlane(new Vector2(1, 0), normal);
		Vector3I locYMove = (Vector3I)Embed2DInPlane(new Vector2(0, 1), normal);

		while (remaining.Count > 0)
		{
			ModuleGridPos minPos = remaining.First();

			bool moveX = true;
			bool failedLast = false;

			for (int i = 0; i < moduleSize * moduleSize; i++)
			{
				Vector3I newPos = moveX ? minPos - locXMove : minPos - locYMove;
				bool hasSurface = remaining.Contains(newPos);

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
				else
					break;
			}

			int maxX = -1;
			int maxY = -1;
			BlockFace surfaceFace = null;
			Block surfaceBlock = default;
			bool hasSurfaceFace = false;
			Vector3I np;

			for (int y = 0; y <= moduleSize; y++)
			{
				bool fullRow = false;
				for (int x = 0; x <= moduleSize; x++)
				{
					np = minPos + locXMove * x + locYMove * y;
					bool firstRow = maxX == -1;
					if (!remaining.Contains(np))
					{
						// Terminate first row
						if (firstRow)
						{
							maxX = x - 1;
							fullRow = true;
							break;
						}

						// Terminate other row
						fullRow = false;
						break;
					}

					module.HasBlock(np, out Block block);
					if (block.IsEmpty)
						throw new Exception("Block contained in surfaces even though it is empty!");


					if (!hasSurfaceFace)
					{
						surfaceFace = GetBlockFace(dir, block, store);
						surfaceBlock = block;
						hasSurfaceFace = true;
						continue;
					}

					bool sameSurface;
					bool sameBlock = surfaceBlock.Id == block.Id && surfaceBlock.Direction == block.Direction;
					if (sameBlock)
					{
						sameSurface = true;
					}
					else
					{
						sameSurface = surfaceFace == GetBlockFace(dir, block, store);
					}

					bool lastCol = x == maxX;

					// Continue expansion of surface
					if (!lastCol && sameSurface)
						continue;
					// Terminate expansion of surface on first row
					else if (!sameSurface && firstRow)
					{
						maxX = x - 1;
						fullRow = true;
						break;
					}
					// Terminate expansion of surface because is x max is reached
					else if (lastCol && sameSurface)
					{
						fullRow = true;
						break;
					}
					// Terminate unfinished row
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
					remaining.Remove(rp);
				}
			}
			Surface surface = CreateSurface(normal, dir, minPos, new Vector2I(maxX, maxY), surfaceFace);
			surfaces.Add(surface);
		}

		return surfaces;
	}

	public static BlockFace GetBlockFace(Direction dir, Block block, BlockStore store)
	{
		BlockFace face;
		BlockDefault blockDefault = store.GetBlockDefault(block);
		if (!blockDefault.Faces.TryGetValue(dir, out face))
			face = blockDefault.DefaultFace;
		return face;
	}

	public static Surface CreateSurface(
		Vector3I normal,
		Direction dir,
		Vector3I minPos,
		Vector2I maxSurface,
		BlockFace blockFace
	)
	{
		List<Vector3I> verts = new List<Vector3I>();
		List<int> inds = new List<int>();

		Vector3 disp = (Vector3)normal * 0.5f + new Vector3(Mathf.Abs(normal.X), Mathf.Abs(normal.Y), Mathf.Abs(normal.Z)) * 0.5f;
		Vector3I displacement = (Vector3I)disp;

		if (dir == Direction.RIGHT)
			displacement += new Vector3I(0, 1, 0);
		else if (dir == Direction.DOWN || dir == Direction.BACKWARD)
			displacement += new Vector3I(1, 0, 0);

		Vector3I basePos = minPos + displacement;

		Vector3I c1 = (Vector3I)Embed2DInPlane(new Vector2I(0, 0), normal) + basePos;
		Vector3I c2 = (Vector3I)Embed2DInPlane(new Vector2I(0, maxSurface.Y + 1), normal) + basePos;
		Vector3I c3 = (Vector3I)Embed2DInPlane(new Vector2I(maxSurface.X + 1, maxSurface.Y + 1), normal) + basePos;
		Vector3I c4 = (Vector3I)Embed2DInPlane(new Vector2I(maxSurface.X + 1, 0), normal) + basePos;

		verts.Add(c1);
		verts.Add(c2);
		verts.Add(c3);
		verts.Add(c4);

		inds.AddRange([0, 1, 2, 2, 3, 0]);

		Vector2 surfaceBlockSpan;
		if (dir == Direction.LEFT || dir == Direction.RIGHT)
		{
			surfaceBlockSpan = new Vector2(maxSurface.X + 1, maxSurface.Y + 1);
		}
		else
		{
			surfaceBlockSpan = new Vector2(maxSurface.Y + 1, maxSurface.X + 1);
		}

		return new Surface(verts, inds, normal, surfaceBlockSpan, dir, blockFace);
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

	public static Mesh BuildModuleMesh(ModuleMeshGenerateContext context, BlockStore store)
	{
		var surfaces = GetSurfaceVectors(context.Module, context.Module.SurfaceCache, store);

		if (surfaces.Count == 0)
			return null;

		int vertexCount = surfaces.Count * 4;
		int indexCount = surfaces.Count * 6;

		// Pre-allocate flat arrays for all mesh data
		var vertices = new Vector3[vertexCount];
		var normals = new Vector3[vertexCount];
		var uvs = new Vector2[vertexCount];
		var colors = new Color[vertexCount];
		var indices = new int[indexCount];

		for (int i = 0; i < surfaces.Count; i++)
		{
			Surface s = surfaces[i];

			BlockFace blockFace = s.BlockFace;
			Color surfaceColor = Color.Color8((byte)blockFace.TextureAtlasPos.X, (byte)blockFace.TextureAtlasPos.Y, 0, 0);
			int rotOffset = Mathf.PosMod(-(int)blockFace.FaceOrientation, 4);
			int vBase = i * 4;
			Vector3 normal = s.Normal;

			for (int j = 0; j < 4; j++)
			{
				int idx = vBase + j;
				int uvId = (j + rotOffset) % 4;

				vertices[idx] = s.Vertices[j];
				normals[idx] = normal;
				uvs[idx] = UVs[uvId] * s.SurfaceBlockSpan;
				colors[idx] = surfaceColor;
			}

			int iBase = i * 6;
			indices[iBase] = vBase;
			indices[iBase + 1] = vBase + 1;
			indices[iBase + 2] = vBase + 2;
			indices[iBase + 3] = vBase + 2;
			indices[iBase + 4] = vBase + 3;
			indices[iBase + 5] = vBase;
		}

		// Build the ArrayMesh directly from arrays
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Mesh.ArrayType.Normal] = normals;
		arrays[(int)Mesh.ArrayType.TexUV] = uvs;
		arrays[(int)Mesh.ArrayType.Color] = colors;
		arrays[(int)Mesh.ArrayType.Index] = indices;

		var arrayMesh = new ArrayMesh();
		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		arrayMesh.SurfaceSetMaterial(0, context.ModuleMaterial);

		return arrayMesh;
	}
}
