using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sperlich.Vectors;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelEditor : MonoBehaviour {

	public Int2 paintIndex;
	public CellType paintCell;
	public Team.Teams team;
	public bool editTeam;
	public string levelName;
	public LevelMap map;
	public static bool AllowEditing = true;

	private void Update() {
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mousePos.y = mousePos.z;
		paintIndex = new Int2(mousePos);

		if(Input.GetKey(KeyCode.Mouse0) && AllowEditing) {
			if(Input.GetKey(KeyCode.LeftShift)) {
				map.Remove(paintIndex);
			} else {
				if(!editTeam) {
					map.Add(paintIndex, paintCell, true, team);
				} else {
					map.SetTeam(paintIndex, team);
				}
				if(paintCell == CellType.Core) {
					var t = map.teams.Find(t => t.team == team);
					if(t != null) {
						t.startIndex = paintIndex;
						map.SetTeam(paintIndex, t.team);
					}
				}
			}
		}
	}

	public void LoadLevel() {
		var core = FindObjectOfType<GameCore>();
		map = LevelMap.LoadLevel(levelName);
		GameCore.activeMap = map;
		map.RenderFullMap();
		if(map != null) {
			AllowEditing = true;
		}
	}

	public void SaveLevel() {
		map.SaveLevel();
	}
}
#if UNITY_EDITOR
[CustomEditor(typeof(LevelEditor))]
public class GridEditorInspector : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		LevelEditor builder = (LevelEditor)target;
		if(GUILayout.Button("Load Level")) {
			builder.LoadLevel();
		}
		if(GUILayout.Button("Save Level")) {
			builder.SaveLevel();
		}
	}
}
#endif