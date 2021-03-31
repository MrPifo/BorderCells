using System;
using System.Runtime.Serialization;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Sperlich.Vectors;
using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[DataContract]
public class LevelMap {

	[DataMember] public string levelName;
	[DataMember] public List<Cell> list = new List<Cell>();
	[DataMember] public bool[] completionStars = new bool[3] { false, false, false };
	[DataMember] public List<Team> teams = new List<Team>();
	[DataMember] public List<CellType> startDeck = new List<CellType>();
	public List<Cell> CapturableCells => list.Where(c => c.CanBeCaptured).ToList();
	public bool HasBeenCompleted => completionStars[0];

	public string LevelPath => FolderPath + levelName + ".json";
	public static string FolderPath => Application.dataPath + "/Resources/Levels/";

	public int Count => list.Count;

	public bool IsReadOnly => false;

	public Cell this[int index] { get => list[index]; set => list[index] = value; }

	public void SaveLevel() {
		string path = LevelPath;
		string json = JsonConvert.SerializeObject(this, new JsonSerializerSettings() {
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.Auto
		});
		File.WriteAllText(path, json);
#if UNITY_EDITOR
		AssetDatabase.Refresh();
#endif
	}

	public void RenderFullMap() {
		int count = 0;
		foreach(Cell c in list) {
			count++;
			var cell = c.RenderCell(count);
			cell.IsAnimating = true;
			cell.SetTeam(cell.ActiveTeam);
		}
	}

	public void SetTeam(Int2 index, Team.Teams team) {
		if(team != Team.Teams.Disabled && Contains(index)) {
			var cell = list.Find(c => c.Index == index);
			if(cell.CanBeCaptured || cell.Id == CellType.Core) {
				cell.SetTeam(team);
			}
		}
	}

	public static LevelMap LoadLevel(string name) {
		if(File.Exists(FolderPath + name + ".json")) {
			LevelMap map = JsonConvert.DeserializeObject<LevelMap>(File.ReadAllText(FolderPath + name + ".json"), new JsonSerializerSettings() {
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto
			});

			return map;
		} else {
			Debug.Log("Map doesn't exist.");
			return null;
		}
	}

	public Team GetPlayerTeam() => teams.Where(t => t.type == Team.TeamType.Player).FirstOrDefault();

	#region List Functions

	public Cell GetCell(Int2 index) => list.Find(c => c.Index == index);

	public int IndexOf(Cell item) => list.IndexOf(item);

	public void Insert(int index, Cell item) => list.Insert(index, item);

	public void RemoveAt(int index) => list.RemoveAt(index);

	public bool Add(Cell item) {
		if(Contains(item) == false) {
			list.Add(item);
			item.RenderCell(1);
			item.SetTeam(item.ActiveTeam);
			return true;
		} else {
			Debug.LogError($"Index: {item.Index} already exists.");
			return false;
		}
	}
	public void Add(Int2 index, CellType type, Team.Teams team = Team.Teams.None) {
		if(!Contains(index)) {
			var cell = new CellDefault(index, type);
			cell = (CellDefault)cell.RenderCell(list.Count);
			Add(cell);
			if(cell.CanBeCaptured) {
				cell.SetTeam(team);
			}
		}
	}

	public void SetCell(Cell cell) {
		if(Contains(cell)) {
			Remove(cell);
		}
		Add(cell);
	}

	public void Clear() => list.Clear();

	public bool Contains(Cell item) => Contains(item.Index);
	public bool Contains(Int2 index) => list.Exists(c => c.Index == index);

	public void CopyTo(Cell[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

	public void Remove(Int2 index) {
		var cell = GetCell(index);
		list.RemoveAll(c => c.Index == index);
		cell.Destroy();
	}
	public void Remove(Cell item) {
		list.RemoveAll(c => c.Index == item.Index);
		item.Destroy();
	}

	public IEnumerator<Cell> GetEnumerator() => list.GetEnumerator();
	#endregion
}
