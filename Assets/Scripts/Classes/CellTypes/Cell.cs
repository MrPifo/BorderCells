using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sperlich.Vectors;
using TMPro;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[Serializable]
[DataContract]
public abstract class Cell : IEquatable<Cell>, ICellEffect {

	public bool IsAnimating { get; set; }
	[DataMember] public CellType Id { get; set; }
	[DataMember] public Int2 Index { get; set; }
	[DataMember] public Team.Teams ActiveTeam { get; set; }
	[DataMember] public float CaptureValue { get; set; }
	public CellData Data { get; set; }
	public (List<Cell>, int) Neighbours { get; set; }
	public bool HasBeenCaptured { get; set; }
	public bool CanBeCaptured => GetData().IsCapturable;
	public bool CanSpread => GetData().CanSpread;
	public bool IsAffectedByOtherCell { get; set; }
	public float brightness;
	public float externSpreadMultiplier = 1f;
	public SpriteRenderer Rend { get; set; }
	public LevelMap map;
	// Debug purposes
	public TMP_Text debugShowValue;

	public Cell() { }

	public Cell(Int2 index) => Index = index;

	public Cell(Int2 index, CellType type) {
		Index = index;
		Id = type;
	}

	public CellData GetData() {
		if(Data == null) {
			Data = GameCore.GetCellData(Id);
		}
		return Data;
	}

	public void RenderCell() {
		Rend = new GameObject().AddComponent<SpriteRenderer>();
		Rend.name = $"Cell_{Id}_{Index}";
		Rend.transform.SetParent(UnityEngine.Object.FindObjectOfType<GameCore>().transform);

		Rend.material = GameCore.SpriteMaterial;
		Rend.sprite = GetData().sprite;
		Rend.transform.position = Index.Vector2;
		SetBrightness(1f);

		OnCreate();
	}

	public void SetTeam(Team.Teams team) {
		ActiveTeam = team;
		SetColor(Team.GetColor(team));
	}

	public void CaptureCell(float value, Team.Teams fromTeam) {
		if(HasBeenCaptured == false) {
			var team = GameCore.GetTeam(fromTeam);
			float mult = externSpreadMultiplier;
			if(ActiveTeam == fromTeam) {
				CaptureValue += mult * value * team.spreadSpeed * Time.deltaTime;
			} else if(ActiveTeam == Team.Teams.None) {
				CaptureValue += mult * value * team.spreadSpeed * Time.deltaTime;
			} else {
				CaptureValue -= mult * value * team.spreadSpeed * Time.deltaTime;
				if(CaptureValue <= 0) {
					SetTeam(fromTeam);
				}
			}
			if(CaptureValue >= 100 && HasBeenCaptured == false) {
				CaptureValue = 100;
				SetTeam(fromTeam);
				HasBeenCaptured = true;
				map.UpdateHelperLists();
			}

			if(debugShowValue != null) {
				debugShowValue.SetText(Mathf.Round(CaptureValue).ToString());
			}
		}
	}

	public void Spread(float multiplier) {
		if(CanSpread == false) return;
		foreach(Cell c in GetNeighbours()) {
			// Continue capture if same team
			if(c.IsTeam(c) && c.HasBeenCaptured == false) {
				// Continue capturing if same team
				c.CaptureCell(GetTeam().spreadSpeed * multiplier * GetData().SpreadSpeed, ActiveTeam);
			} else if(c.IsTeam(c) == false) {
				// Capture if from other team
				c.CaptureCell(GetTeam().spreadSpeed * multiplier * GetData().SpreadSpeed, ActiveTeam);
			} else if(c.IsTeam(c)) {
				// If same team do nothing
			}
		}
	}

	public List<Cell> GetNeighbours(int radius = 1) {
		if(radius != Neighbours.Item2) {
			var neighs = new List<Cell>();

			for(int x = -radius; x <= radius; x++) {
				for(int y = -radius; y <= radius; y++) {
					var c = GameCore.activeMap.GetGameCell(Index + new Int2(x, y));
					if(c != null && c.CanBeCaptured && c.Index != Index) neighs.Add(c);
				}
			}
			Neighbours = (neighs, radius);
			return neighs;
		}
		return Neighbours.Item1;
	}

	public void UpdateNeighbours(int radius) {
		Neighbours = (GetNeighbours(radius), radius);
	}

	public bool IsTeam(Cell cell) => ActiveTeam == cell.ActiveTeam;
	public bool IsTeam(Team.Teams team) => ActiveTeam == team;

	public Team GetTeam() => GameCore.GetTeam(ActiveTeam);

	public bool IsAssignedToTeam() {
		if(ActiveTeam == Team.Teams.Disabled || ActiveTeam == Team.Teams.None || GetTeam() == null) return false;
		return true;
	}

	public void Destroy() {
		OnDelete();
	}

	public bool Equals(Cell other) {
		if(other.Index == Index) return true;
		return false;
	}

	public void SetBrightness(float value, float min = 0) {
		value = Mathf.Clamp01(value);
		if(value < min) value = min;
		Color.RGBToHSV(Rend.color, out float H, out float S, out float V);
		V = value;

		Rend.color = Color.HSVToRGB(H, S, V);
		brightness = value;
	}

	public void SetColor(Color col, float brightness = -1) {
		if(Rend != null) {
			Rend.color = col;
			if(brightness >= 0) {
				SetBrightness(brightness);
			} else {
				SetBrightness(this.brightness);
			}
		}
	}

	public static Cell CreateCell(CellType type, Int2 index = new Int2()) {
		Cell cell;
		switch(type) {
			case CellType.Disabled:
				cell = new CellDefault(index, type) {
					ActiveTeam = Team.Teams.None
				};
				break;
			case CellType.Boost4x4:
				cell = new CellBoost4x4(index, type);
				break;
			default:
				cell = new CellDefault(index, type);
				break;
		}
		return cell;
	}

	public abstract void OnCreate();
	public abstract void OnDelete();
}