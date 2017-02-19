using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

public class BitmapMesh : MonoBehaviour {

	[SerializeField] private Texture2D bitmapTexture = null;
	[SerializeField] private Material poolMaterial = null;
	[SerializeField] private Material poolBorderMaterial = null;
	[SerializeField] private Material poolWallMaterial = null;

	[SerializeField] private float borderSize = 0.1f;
	[SerializeField] private float poolHeight = 1.0f;
	[SerializeField] private float minPointDistance = 0.2f;
	[SerializeField] private float scaleX = 0.01f;
	[SerializeField] private float scaleY = 0.01f;

	private Mesh mesh;
	private short[,] imageData;
	private List<List<Vector2>> outlines;
	private HashSet<int> visitedCells;
	private List<Vector3> dbg = new List<Vector3> ();

	List<LineSegment> t = new List<LineSegment>();

	public void GenerateOutlines() {
		if (bitmapTexture == null) {
			return;
		}

		imageData = ReadTextureToMemory (bitmapTexture);
		outlines = TraceEdge (imageData);
		GenerateMeshesFromOutlines (outlines);
	}

	private void GenerateMeshesFromOutlines(List<List<Vector2>> outlines) {
		for (int i = 0; i < transform.childCount; i++) {
			DestroyImmediate (transform.GetChild (i).gameObject, true);
		}

		for (int i = 0; i < outlines.Count; i++) {
			List<Vector2> outline = outlines [i];

			GameObject poolObject = new GameObject ("Pool" + i.ToString ());
			MeshFilter poolObjectMeshFilter = poolObject.AddComponent<MeshFilter> ();
			MeshRenderer poolObjectMeshRenderer = poolObject.AddComponent<MeshRenderer> ();

			GameObject poolBorderObject = new GameObject ("PoolBorder" + i.ToString ());
			MeshFilter poolBorderObjectMeshFilter = poolBorderObject.AddComponent<MeshFilter> ();
			MeshRenderer poolBorderObjectMeshRenderer = poolBorderObject.AddComponent<MeshRenderer> ();

			GameObject poolWallObject = new GameObject ("PoolWall" + i.ToString ());
			MeshFilter poolWallObjectMeshFilter = poolWallObject.AddComponent<MeshFilter> ();
			MeshRenderer poolWallObjectMeshRenderer = poolWallObject.AddComponent<MeshRenderer> ();

			poolObject.transform.SetParent (transform);
			poolBorderObject.transform.SetParent (transform);
			poolWallObject.transform.SetParent (transform);

			Mesh poolBorderMesh = new Mesh ();
			List<Vector3> poolBorderVertices = new List<Vector3> ();
			List<int> poolBorderTriangles = new List<int> ();

			Mesh poolWallMesh = new Mesh ();
			List<Vector3> poolWallVertices = new List<Vector3> ();
			List<int> poolWallTriangles = new List<int> ();

			int li0 = 0;
			int li1 = 0;

			int li2 = 0;
			int li3 = 0;

			for (int j = 0; j < outline.Count; j++) {
				int mid = j;
				int fw = (j + 1) % (outline.Count);
				int bw = (outline.Count - 1 + j) % (outline.Count);

				Vector3 pMid = outline[mid];
				Vector3 pFw = outline[fw];
				Vector3 pBw = outline [bw];

				Vector3 d0 = Vector3.Cross ((pFw - pMid).normalized, Vector3.forward);
				Vector3 d1 = Vector3.Cross ((pBw - pMid).normalized, Vector3.forward);

				Vector3 p0 = pMid - d0 * borderSize;
				Vector3 p1 = pMid + d1 * borderSize;
				Vector3 pm = p0 + ((p1 - p0) * 0.5f);

				poolBorderVertices.Add (pMid + Vector3.forward * - poolHeight);
				poolBorderVertices.Add (pm + Vector3.forward * - poolHeight);

				poolWallVertices.Add (pMid);
				poolWallVertices.Add (pMid + Vector3.forward * - poolHeight);

				if (j > 0) {
					poolBorderTriangles.Add (poolBorderVertices.Count - 2);
					poolBorderTriangles.Add (li0);
					poolBorderTriangles.Add (li1);

					poolBorderTriangles.Add (poolBorderVertices.Count - 2);
					poolBorderTriangles.Add (li1);
					poolBorderTriangles.Add (poolBorderVertices.Count - 1);

					poolWallTriangles.Add (poolWallVertices.Count - 2);
					poolWallTriangles.Add (li2);
					poolWallTriangles.Add (li3);

					poolWallTriangles.Add (poolWallVertices.Count - 2);
					poolWallTriangles.Add (li3);
					poolWallTriangles.Add (poolWallVertices.Count - 1);
				}

				li0 = poolBorderVertices.Count - 2;
				li1 = poolBorderVertices.Count - 1;

				li2 = poolWallVertices.Count - 2;
				li3 = poolWallVertices.Count - 1;
			}

			// connect last border triangles of the loop
			poolBorderTriangles.Add (0);
			poolBorderTriangles.Add (poolBorderVertices.Count - 1);
			poolBorderTriangles.Add (1);

			poolBorderTriangles.Add (0);
			poolBorderTriangles.Add (poolBorderVertices.Count - 2);
			poolBorderTriangles.Add (poolBorderVertices.Count - 1);

			// connect last wall triangles of the loop
			poolWallTriangles.Add (0);
			poolWallTriangles.Add (poolWallVertices.Count - 1);
			poolWallTriangles.Add (1);

			poolWallTriangles.Add (0);
			poolWallTriangles.Add (poolWallVertices.Count - 2);
			poolWallTriangles.Add (poolWallVertices.Count - 1);


			poolBorderMesh.SetVertices (poolBorderVertices);
			poolBorderMesh.SetTriangles (poolBorderTriangles, 0);
			poolBorderMesh.RecalculateBounds ();
			poolBorderMesh.RecalculateNormals ();

			poolWallMesh.SetVertices (poolWallVertices);
			poolWallMesh.SetTriangles (poolWallTriangles, 0);
			poolWallMesh.RecalculateBounds ();
			poolWallMesh.RecalculateNormals ();

			poolBorderObjectMeshRenderer.material = poolBorderMaterial;
			poolBorderObjectMeshFilter.mesh = poolBorderMesh;

			poolWallObjectMeshRenderer.material = poolWallMaterial;
			poolWallObjectMeshFilter.mesh = poolWallMesh;

			poolObjectMeshRenderer.material = poolMaterial;

			TriangulatorSimple ts = new TriangulatorSimple (outline.ToArray ());
			int[] triangles = ts.Triangulate ();

			List<Vector3> vertices = new List<Vector3>();
			for (int j = 0; j < outline.Count; j++) {
				vertices.Add (outline[j]);
			}

			Mesh poolMesh = new Mesh ();
			poolMesh.SetVertices (vertices);
			poolMesh.triangles = triangles;
			poolMesh.RecalculateBounds ();
			poolMesh.RecalculateNormals ();
	
			poolObjectMeshFilter.mesh = poolMesh;
		}
	}

	private short[,] ReadTextureToMemory(Texture2D t) {
		short[,] data = new short[t.width, t.height];
		int width = bitmapTexture.width;
		int height = bitmapTexture.height;

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				data [x, y] = (short) bitmapTexture.GetPixel (x, y).r;
			}
		}

		return data;
	}

	private List<List<Vector2>> TraceEdge(short[,] imgData) {
		List<List<Vector2>> outlines = new List<List<Vector2>> ();
		visitedCells = new HashSet<int> ();

		for (int y = 0; y < imgData.GetLength(1); y++) {
			for (int x = 0; x < imgData.GetLength(0); x++) {
				Cell cell = new Cell (x, y, imgData [x, y]);

				if (cell.value == 0) {
					List<Cell> numberOfBlackCells = GetNeighbourCellsByValue (cell, 0);

					if (numberOfBlackCells.Count < 8 && !visitedCells.Contains(cell.GetCellIndex())) {
						List<Vector2> outline = new List<Vector2> ();
						FollowEdge (outline, cell);
						ApplyMinPointDistanceToOutline (ref outline, minPointDistance);
						outlines.Add (outline);
					}
				}
			}
		}

		return outlines;
	}

	private void FollowEdge(List<Vector2> outline, Cell cell) {
		visitedCells.Add (cell.GetCellIndex());
		outline.Add (new Vector2 (cell.x * scaleX, cell.y * scaleY));

		Cell edgeCell = GetNextEdgeCell (cell);

		if (edgeCell != null) {
			FollowEdge (outline, edgeCell);
		}
	}

	private Cell GetNextEdgeCell(Cell cell) {
		List<Cell> neighbourBlackCells = GetNeighbourCellsByValue (cell, 0);

		for (int i = 0; i < neighbourBlackCells.Count; i++) {
			if (!visitedCells.Contains (neighbourBlackCells[i].GetCellIndex())) {
				if (HasEmptyNeighbour (neighbourBlackCells [i])) {
					return neighbourBlackCells [i];
				}
			}
		}

		return null;
	}

	private List<Cell> GetNeighbourCells(Cell cell) {
		List<Cell> cells = new List<Cell> ();

		Vector2[] positionsToCheck = new Vector2[] {
			new Vector2(0, 1),		new Vector2(1, 0),
			new Vector2(0, -1),		new Vector2(-1, 0),
			new Vector2(1, 1),		new Vector2(1, -1),
			new Vector2(-1, -1),	new Vector2(-1, 1),
		};

		for (int i = 0; i < positionsToCheck.Length; i++) {
			int ix = cell.x + (int) positionsToCheck[i].x;
			int iy = cell. y + (int) positionsToCheck [i].y;

			if (IsInBounds (ix, iy)) {
				cells.Add (new Cell (ix, iy, imageData [ix, iy]));
			}
		}

		return cells;
	}

	private List<Cell> GetNeighbourCellsByValue(Cell cell, short value) {
		List<Cell> cells = new List<Cell> ();

		Vector2[] positionsToCheck = new Vector2[] {
			new Vector2(0, 1),		new Vector2(1, 0),
			new Vector2(0, -1),		new Vector2(-1, 0),
			new Vector2(1, 1),		new Vector2(1, -1),
			new Vector2(-1, -1),	new Vector2(-1, 1),
		};

		for (int i = 0; i < positionsToCheck.Length; i++) {
			int ix = cell.x + (int) positionsToCheck[i].x;
			int iy = cell.y + (int) positionsToCheck [i].y;

			if (IsInBounds (ix, iy) && imageData[ix, iy] == value) {
				cells.Add (new Cell (ix, iy, imageData [ix, iy]));
			}
		}

		return cells;
	}

	private bool HasEmptyNeighbour(Cell cell) {
		List<Cell> cells = GetNeighbourCells (cell);

		for (int i = 0; i < cells.Count; i++) {
			if (cells [i].value > 0) {
				return true;
			}
		}

		return false;
	}

	private bool IsInBounds(int x, int y) {
		return x >= 0 && x < imageData.GetLength(0) && y >= 0 && y < imageData.GetLength(1);
	}

	private void ApplyMinPointDistanceToOutline(ref List<Vector2> outline, float minDistance) {
		List<Vector2> filteredOutline = new List<Vector2> ();
		int nextIndex = 0;

		while (nextIndex != -1) {
			filteredOutline.Add (outline [nextIndex]);
			nextIndex = GetNextMinDistancePointIndex (outline, minDistance, nextIndex);
		}

		outline = filteredOutline;
	}

	private int GetNextMinDistancePointIndex(List<Vector2> o, float minDistance, int index) {
		Vector2 p = o [index];
		for (int i = index; i < o.Count; i++) {
			float distance = Vector2.Distance (p, o [i]);
			if (distance >= minDistance) {
				return i;
			}
		}

		return -1;
	}

	private void DrawPointSequenceDebug() {
//		for(int i=0;i<filteredOutline.Count;i++) {
//			TextMesh tm = Instantiate(textMesh, filteredOutline[i], Quaternion.identity) as TextMesh;
//			tm.text = i.ToString();
//		}
	}

	private void OnDrawGizmos() {
//		for (int i = 0; i < dbg.Count; i++) {
//			Gizmos.color = Color.black;
//			Gizmos.DrawCube (dbg [i], Vector3.one * 0.05f);	
//		}
//		if (outlines == null) {
//			return;
//		}
//
//		for (int i = 0; i < outlines.Count; i++) {
//			for (int j = 0; j < outlines [i].Count; j++) {
//				Gizmos.color = Color.black;
//				Gizmos.DrawCube (outlines [i][j], Vector3.one * 0.03f);	
//			}
//		}
	}

}
