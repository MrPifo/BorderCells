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
	[DataMember] public	StarCondition[] completionStars = new StarCondition[3];
	[DataMember] public List<Team> teams = new List<Team>();
	[DataMember] public List<CellType> startDeck = new List<CellType>();
	public Dictionary<Int2, GameObject> render = new Dictionary<Int2, GameObject>();
	public Dictionary<Int2, Cell> GameCells { get; set; } = new Dictionary<Int2, Cell>();
	public List<Cell> SpreaderCells { get; set; }
	public List<Cell> ReplaceableCells { get; set; }
	public bool HasBeenCompleted => completionStars[0].completed;

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
		foreach(Cell c in list) {
			c.RenderCell();
			c.SetTeam(c.ActiveTeam);
			c.map = this;

			render.Add(c.Index, c.Rend.gameObject);
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

	public void UpdateHelperLists() {
		SpreaderCells = GameCells.Values.Where(c => c.HasBeenCaptured).ToList();
		ReplaceableCells = GameCells.Values.Where(c => c.HasBeenCaptured == false).ToList();
		foreach(Team t in teams) {
			t.capturedCells = SpreaderCells.Where(c => c.ActiveTeam == t.team).ToList();
		}
	}

	public void UpdateGameCells() {
		GameCells = new Dictionary<Int2, Cell>();
		foreach(Cell c in list.Where(c => c.GetData().IsStatic == false)) {
			GameCells.Add(c.Index, c);
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

	public Cell GetGameCell(Int2 index) {
		GameCells.TryGetValue(index, out Cell c);
		return c;
	}

	public int IndexOf(Cell item) => list.IndexOf(item);

	public void Insert(int index, Cell item) => list.Insert(index, item);

	public void RemoveAt(int index) => list.RemoveAt(index);

	public bool Add(Cell item, bool noRender) {
		if(Contains(item) == false) {
			list.Add(item);
			if(noRender == false) {
				item.RenderCell();
				render.Add(item.Index, item.Rend.gameObject);
			}
			item.SetTeam(item.ActiveTeam);
			item.map = this;
			UpdateHelperLists();
			return true;
		} else {
			Debug.LogError($"Index: {item.Index} already exists.");
			return false;
		}
	}
	public void Add(Int2 index, CellType type, bool noRender, Team.Teams team = Team.Teams.None) {
		if(!Contains(index)) {
			var cell = new CellDefault(index, type);
			Add(cell, noRender);
			if(cell.CanBeCaptured) {
				cell.SetTeam(team);
			}
		}
	}

	public void SetCell(Cell cell, bool noRender) {
		if(Contains(cell)) {
			Remove(cell);
		}
		Add(cell, noRender);
	}

	public void Clear() => list.Clear();

	public bool Contains(Cell item) => Contains(item.Index);
	public bool Contains(Int2 index) => list.Exists(c => c.Index == index);

	public void CopyTo(Cell[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

	public void Remove(Int2 index) {
		list.RemoveAll(c => c.Index == index);
		GetGameCell(index)?.Destroy();

		if(render.ContainsKey(index)) {
			UnityEngine.Object.Destroy(render[index]);
			render.Remove(index);
		}
	}
	public void Remove(Cell item) => Remove(item.Index);

	public IEnumerator<Cell> GetEnumerator() => list.GetEnumerator();
	#endregion
}
