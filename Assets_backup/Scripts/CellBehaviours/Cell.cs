using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sperlich.Vectors;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[Serializable]
[DataContract]
public abstract class Cell : IEquatable<Cell>, ICellEffect {

	public bool IsAnimating { get; set; }
	[DataMember] public CellType Id { get; set; }
	[DataMember] public Int2 Index { get; set; }
	[DataMember] public Team.Teams ActiveTeam { get; set; }
	[DataMember] public float CaptureValue { get; set; }
	public bool HasBeenCaptured => CaptureValue >= 100;
	public bool CanBeCaptured => GetData().IsCapturable;
	public bool CanSpread => GetData().CanSpread;
	// Debug purpose
	public TMP_Text debugShowValue;

	public Cell() { }

	public Cell(Int2 index) => Index = index;

	public Cell(Int2 index, CellType type) {
		Index = index;
		Id = type;
	}

	public CellData GetData() => GameCore.GetCellData(Id);

	public Cell RenderCell(int num) {
		/*EntityArchetype archType = GameCore.EntityManager.CreateArchetype(
			typeof(Translation),
			typeof(LocalToWorld),
			typeof(Scale),
			typeof(RenderMesh),
			typeof(BuiltinMaterialPropertyUnity_LightData),
			typeof(Rotation),
			typeof(RenderBounds));
		//Entity = GameCore.EntityManager.CreateEntity(archType);*/
		if(EntityFactory.PrefabExists("Sprite")) {
			var entity = GameCore.EntityManager.Instantiate(EntityFactory.GetPrefab("Sprite"));
			GameCore.EntityManager.SetName(entity, $"Cell_{Id}_{Index}");

			var mat = new Material(GameCore.SpriteMaterial) {
				mainTexture = GetData().sprite.texture,
				color = Color.red
			};
			
			RenderMesh rendMesh = new RenderMesh() {
				castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
				receiveShadows = false,
				mesh = GameCore.SpriteMesh,
				material = mat,
			};
			//bool isAnimating = Id != CellType.Empty;
			bool isAnimating = false;
			GameCore.EntityManager.AddComponentData(entity, new ICell() { isAnimating = isAnimating, number = num == 0 ? 1 : num });
			GameCore.EntityManager.AddComponentData(entity, new Scale() { Value = isAnimating ? 0 : 1 });
			GameCore.EntityManager.AddComponentData(entity, new Translation() { Value = new float3(Index.x, 0, Index.y) });
			GameCore.EntityManager.AddSharedComponentData(entity, rendMesh);
			GameCore.EntityManager.AddComponentData(entity, new Rotation() {
				Value = Quaternion.Euler(0, 0, 0)
			});
			GameCore.EntityManager.RemoveComponent<Disabled>(entity);
			OnCreate();
			return this;
		}
		return null;
	}

	public void SetTeam(Team.Teams team) {
		ActiveTeam = team;
		var rend = GameCore.EntityManager.GetSharedComponentData<RenderMesh>(Entity);
		rend.material.SetColor("_BaseColor", Team.GetColor(team));
		GameCore.EntityManager.SetSharedComponentData(Entity, rend);
	}

	public void CaptureCell(float value, Team.Teams fromTeam) {
		var team = GameCore.GetTeam(fromTeam);
		var mult = ModifySpreadSpeed();
		if(ActiveTeam == fromTeam) {
			CaptureValue += mult * value * team.spreadSpeed * Time.deltaTime;
			if(CaptureValue >= 100) {
				CaptureValue = 100;
			}
		} else if(ActiveTeam == Team.Teams.None) {
			CaptureValue += mult * value * team.spreadSpeed * Time.deltaTime;
			if(CaptureValue >= 100) {
				CaptureValue = 100;
				SetTeam(fromTeam);
			}
		} else {
			CaptureValue -= mult * value * team.spreadSpeed * Time.deltaTime;
			if(CaptureValue <= 0) {
				SetTeam(fromTeam);
			}
		}
		if(debugShowValue != null) {
			debugShowValue.SetText(Mathf.Round(CaptureValue).ToString());
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

	public List<Cell> GetNeighbours() {
		var neighs = new List<Cell>();

		// Upper neighbour
		var up = GameCore.activeMap.GetCell(Index + new Int2(0, 1));
		if(up != null && up.CanBeCaptured) neighs.Add(up);

		// Below neighbour
		var below = GameCore.activeMap.GetCell(Index + new Int2(0, -1));
		if(below != null && below.CanBeCaptured) neighs.Add(below);

		// Right neighbour
		var right = GameCore.activeMap.GetCell(Index + new Int2(1, 0));
		if(right != null && right.CanBeCaptured) neighs.Add(right);

		// Left neighbour
		var left = GameCore.activeMap.GetCell(Index + new Int2(-1, 0));
		if(left != null && left.CanBeCaptured) neighs.Add(left);

		return neighs;
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
		try {
			GameCore.EntityManager.AddComponent<Disabled>(Entity);
			Debug.LogWarning("Disabled " + Index);
		} catch {

		}
	}

	public bool Equals(Cell other) {
		if(other.Index == Index) return true;
		return false;
	}

	public static Cell CreateCell(CellType type, Int2 index = new Int2()) {
		Cell cell = null;
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
	public abstract void ApplyEffect();
	public abstract void OnDelete();
	public abstract float ModifySpreadSpeed();
}