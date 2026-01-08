using Godot;
using System;
using System.Collections.Generic;

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

	public static Dictionary<Vector3I, bool[]> FindBlockSurfaces(Module module)
	{
		var exposedBlocks = new Dictionary<Vector3I, bool[]>();
		for (int z = 0; z < module.ModuleSize; z++)
		{
			for (int y = 0; y < module.ModuleSize; y++)
			{
				for (int x = 0; x < module.ModuleSize; x++)
				{
					Vector3I blockPos = new Vector3I(x, y, z);

					if (module.GetBlock(blockPos) == -1)
						continue;

					bool[] exposedSurfaces = new bool[Normals.Length];
					bool hasExposed = false;

					for (int i = 0; i < Normals.Length; i++)
					{
						Vector3I dir = Normals[i];
						Vector3I adjacentPos = blockPos + dir;
						int adjacentBlock;

						if (module.IsInModule(adjacentPos))
						{
							adjacentBlock = module.GetBlock(adjacentPos);
						}
						else
						{
							adjacentBlock = -1;
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

	public static List<Surface> GetSurfaceVectors(Module module, Dictionary<Vector3I, bool[]> exposed)
	{
		var blockSurfaces = new Dictionary<Vector3I, bool[]>();
		foreach (var kv in exposed)
			blockSurfaces[kv.Key] = (bool[])kv.Value.Clone();

		int moduleSize = module.ModuleSize;
		var surfaces = new List<Surface>();

		while (blockSurfaces.Count > 0)
		{
			Vector3I startPos = new List<Vector3I>(blockSurfaces.Keys)[0];
			Direction dirIndex = 0;
			Vector3I normal = new Vector3I();

			for (int i = 0; i < Normals.Length; i++)
			{
				if (blockSurfaces[startPos][i])
				{
					dirIndex = (Direction)i;
					normal = Normals[i];
					break;
				}
			}

			Vector3I locXMove = (Vector3I)Embed2DInPlane(new Vector2(1, 0), normal);
			Vector3I locYMove = (Vector3I)Embed2DInPlane(new Vector2(0, 1), normal);

			Vector3I minPos = startPos;
			bool moveX = true;
			bool failedLast = false;

			for (int i = 0; i < moduleSize * moduleSize; i++)
			{
				Vector3I newPos = moveX ? minPos - locXMove : minPos - locYMove;
				bool hasSurface = blockSurfaces.ContainsKey(newPos) && blockSurfaces[newPos][(int)dirIndex];

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
					int blockId = module.IsInModule(np) ? module.GetBlock(np) : -1;

					if (surfaceBlockId == -1)
					{
						surfaceBlockId = blockId;
					}

					bool hasSurface = blockSurfaces.ContainsKey(np) && blockSurfaces[np][(int)dirIndex];
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
					blockSurfaces[rp][(int)dirIndex] = false;
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
			else if ((int)dirIndex == 3 || (int)dirIndex == 4)
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

			inds.AddRange([0, 1, 2, 2, 3, 0]);

			Vector2 surfaceBlockSpan;
			if (dirIndex == Direction.LEFT || dirIndex == Direction.RIGHT)
			{
				surfaceBlockSpan = new Vector2(maxX + 1, maxY + 1);
			}
			else
			{
				surfaceBlockSpan = new Vector2(maxY + 1, maxX + 1);
			}

			surfaces.Add(new Surface(verts, inds, normal, surfaceBlockSpan, dirIndex, surfaceBlockId));
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

	public static Mesh BuildModuleMesh(
		Module module,
		Material mat,
		BlockStore blockStore)
	{
		var exposed = FindBlockSurfaces(module);
		var surfaces = GetSurfaceVectors(module, exposed);

		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		st.SetMaterial(mat);
		int rotOffset = 0;

		for (int i = 0; i < surfaces.Count; i++)
		{
			Surface s = surfaces[i];
			BlockDefault blockDefault = blockStore.blockDefaults[s.BlockId];
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
