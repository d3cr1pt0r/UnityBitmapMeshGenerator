using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

public class BitmapMesh : MonoBehaviour {

	[SerializeField] private Texture2D bitmapTexture = null;
	[SerializeField] private MeshFilter meshFilter = null;
	[SerializeField] private TextMesh textMesh = null;

	[SerializeField] private float minPointDistance = 0.2f;
	[SerializeField] private float scaleX = 0.01f;
	[SerializeField] private float scaleY = 0.01f;

	private Mesh mesh;

	private short[,] imageData;
	private List<Vector2> outline;
	private List<Vector2> filteredOutline;
	private HashSet<int> visitedCells;

	List<LineSegment> t = new List<LineSegment>();

	public void GenerateEdgePoints() {
		if (bitmapTexture == null) {
			return;
		}
		outline = new List<Vector2> ();
		visitedCells = new HashSet<int> ();

		imageData = ReadTextureToMemory (bitmapTexture);
		TraceEdge (imageData);
		filteredOutline = ApplyMinPointDistanceToOutline (outline, minPointDistance);

		TriangulatorSimple ts = new TriangulatorSimple (filteredOutline.ToArray ());

		int[] indices = ts.Triangulate ();

		List<Vector3> vertices = new List<Vector3>();
		for (int i = 0; i < filteredOutline.Count; i++) {
			vertices.Add (filteredOutline [i]);
		}

		mesh = new Mesh ();
		mesh.SetVertices (vertices);
		mesh.triangles = indices;
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		meshFilter.mesh = mesh;

//		DrawPointSequenceDebug ();
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

	private void TraceEdge(short[,] imgData) {
		bool shouldStop = false;
		for (int y = 0; y < imgData.GetLength(1); y++) {
			if (shouldStop) break;

			for (int x = 0; x < imgData.GetLength(0); x++) {
				Cell cell = new Cell (x, y, imgData [x, y]);

				if (cell.value == 0) {
					List<Cell> numberOfBlackCells = GetNeighbourCellsByValue (cell, 0);

					if (numberOfBlackCells.Count < 8) {
						FollowEdge (cell);
						shouldStop = true;
						break;
					}
				}
			}
		}
	}

	private void FollowEdge(Cell cell) {
		visitedCells.Add (cell.GetCellIndex());
		outline.Add (new Vector2 (cell.x * scaleX, cell.y * scaleY));

		Cell edgeCell = GetNextEdgeCell (cell);

		if (edgeCell != null) {
			FollowEdge (edgeCell);
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

	private List<Vector2> ApplyMinPointDistanceToOutline(List<Vector2> o, float minDistance) {
		List<Vector2> filteredOutline = new List<Vector2> ();
		int nextIndex = 0;

		while (nextIndex != -1) {
			filteredOutline.Add (o [nextIndex]);
			nextIndex = GetNextMinDistancePointIndex (o, minDistance, nextIndex);
		}

		return filteredOutline;
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
		for(int i=0;i<filteredOutline.Count;i++) {
			TextMesh tm = Instantiate(textMesh, filteredOutline[i], Quaternion.identity) as TextMesh;
			tm.text = i.ToString();
		}
	}

	private void OnDrawGizmos() {
		if (outline == null) {
			return;
		}

		for (int i = 0; i < filteredOutline.Count; i++) {
			Gizmos.color = Color.black;
			Gizmos.DrawCube (filteredOutline[i], Vector3.one * 0.03f);
		}
	}

}
