using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


public partial class ModuleMeshGenerator : Node
{
	public class Surface
	{
		public List<Vector3I> Vertices = new List<Vector3I>();
		public List<int> Indices = new List<int>();
		public Vector3I Normal;
		public Vector2 SurfaceBlockSpan;
		public Direction Dir;
		public int BlockId;

		public Surface(List<Vector3I> vertices, List<int> indices, Vector3I normal, Vector2 surfaceBlockSpan, Direction dir, int blockId)
		{
			Vertices = vertices;
			Indices = indices;
			Normal = normal;
			SurfaceBlockSpan = surfaceBlockSpan;
			Dir = dir;
			BlockId = blockId;
		}
	}

	public static readonly Vector2[] UVs = new Vector2[]
	{
		new Vector2(0, 0),
		new Vector2(1, 0),
		new Vector2(1, 1),
		new Vector2(0, 1)
	};

	public static List<Surface> GetSurfaceVectors(Module module, ExposedModuleSurfaceCache cache)
	{
		int moduleSize = module.ModuleSize;
		var surfaces = new List<Surface>();

		foreach (var kvp in cache.ExposedSurfaces)
		{
			Direction dir = kvp.Key;
			ICollection<ModuleGridPos> surfacePositions = kvp.Value.ToHashSet();
			List<Surface> newSurfaces = FindDirSurfaces(moduleSize, dir, module, surfacePositions);
			surfaces.AddRange(newSurfaces);
		}

		return surfaces;
	}

	private static List<Surface> FindDirSurfaces(int moduleSize, Direction dir, Module module, ICollection<ModuleGridPos> surfacePositions)
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
				else break;
			}


			int maxX = -1;
			int maxY = -1;
			int surfaceBlockId = -1;

			for (int y = 0; y <= moduleSize; y++)
			{
				bool fullRow = false;
				for (int x = 0; x <= moduleSize; x++)
				{
					Vector3I np = minPos + locXMove * x + locYMove * y;
					module.HasBlock(np, out int blockId);
					if (surfaceBlockId == -1) surfaceBlockId = blockId;

					bool hasSurface = remaining.Contains(np);
					bool sameSurface = blockId == surfaceBlockId;
					bool firstRow = maxX == -1;
					bool lastCol = x == maxX;

					if (hasSurface && !lastCol && sameSurface)
						continue;
					else if ((!hasSurface || !sameSurface) && firstRow)
					{
						maxX = x - 1;
						fullRow = true;
						break;
					}
					else if (hasSurface && lastCol && sameSurface)
					{
						fullRow = true;
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
					remaining.Remove(rp);
				}
			}
			Surface surface = CreateSurface(normal, dir, minPos, new Vector2I(maxX, maxY), surfaceBlockId);
			surfaces.Add(surface);
		}

		return surfaces;
	}

	public static Surface CreateSurface(Vector3I normal, Direction dir, Vector3I minPos, Vector2I maxSurface, int surfaceBlockId)
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

		return new Surface(verts, inds, normal, surfaceBlockSpan, dir, surfaceBlockId);
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

	public static Mesh BuildModuleMesh(ModuleMeshGenerateContext context)
	{
		var sw = Stopwatch.StartNew();
		Debug.WriteLine("Add Module Time: " + sw.Elapsed);

		sw.Restart();
		var surfaces = GetSurfaceVectors(context.Module, context.Module.SurfaceCache);
		Debug.WriteLine("Create Vectors Time: " + sw.Elapsed);

		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		st.SetMaterial(context.ModuleMaterial);
		int rotOffset = 0;

		for (int i = 0; i < surfaces.Count; i++)
		{
			Surface s = surfaces[i];
			BlockDefault blockDefault = context.BlockStore.blockDefaults[s.BlockId];
			Color surfaceColor = Color.Color8(0, 0, 0, 0);

			switch (s.Dir)
			{
				case Direction.UP:
					surfaceColor = Color.Color8((byte)blockDefault.TextureAtlasFaceUp.X, (byte)blockDefault.TextureAtlasFaceUp.Y, 0, 0);
					rotOffset = 0;
					break;
				case Direction.DOWN:
					surfaceColor = Color.Color8((byte)blockDefault.TextureAtlasFaceDown.X, (byte)blockDefault.TextureAtlasFaceDown.Y, 0, 0);
					rotOffset = 0;
					break;
				case Direction.LEFT:
					surfaceColor = Color.Color8((byte)blockDefault.TextureAtlasFaceLeft.X, (byte)blockDefault.TextureAtlasFaceLeft.Y, 0, 0);
					rotOffset = 3;
					break;
				case Direction.RIGHT:
					surfaceColor = Color.Color8((byte)blockDefault.TextureAtlasFaceRight.X, (byte)blockDefault.TextureAtlasFaceRight.Y, 0, 0);
					rotOffset = 1;
					break;
				case Direction.FORWARD:
					surfaceColor = Color.Color8((byte)blockDefault.TextureAtlasFaceForward.X, (byte)blockDefault.TextureAtlasFaceForward.Y, 0, 0);
					rotOffset = 2;
					break;
				case Direction.BACKWARD:
					surfaceColor = Color.Color8((byte)blockDefault.TextureAtlasFaceBackward.X, (byte)blockDefault.TextureAtlasFaceBackward.Y, 0, 0);
					rotOffset = 2;
					break;
			}

			st.SetNormal(s.Normal);

			for (int j = 0; j < 4; j++)
			{
				int uvId = (j + rotOffset) % 4;
				st.SetUV(UVs[uvId] * s.SurfaceBlockSpan);
				st.SetColor(surfaceColor);
				st.AddVertex(s.Vertices[j]);
			}

			foreach (int ind in s.Indices)
				st.AddIndex(i * 4 + ind);
		}

		return st.Commit();
	}
}
