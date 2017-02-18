using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BitmapMesh))]
public class BitmapMeshGUI : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();

		BitmapMesh bitmapMesh = (BitmapMesh) target;

		if (GUILayout.Button ("TraceEdges")) {
			bitmapMesh.GenerateEdgePoints ();
		}
	}

}
