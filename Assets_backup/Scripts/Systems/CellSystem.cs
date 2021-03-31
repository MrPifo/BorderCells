using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Sperlich.Vectors;
using Unity.Rendering;
using Unity.Mathematics;

public class CellSystem : ComponentSystem {

	public static LevelMap activeMap;

	protected override void OnUpdate() {
		if(!GameCore.Initialized) {
			//PostUpdateCommands.des
			/*Entities.ForEach((ref ICell cell, ref Scale scale) => {
				if(activeMap != null && cell.isAnimating) {
					cell.scale += Mathf.Clamp(Time.DeltaTime * cell.number / activeMap.Count * 5f, 0.005f, 1f);
					scale.Value = GameCore.SpriteSpawnAnimationCurve.Evaluate(cell.scale);
					if(cell.scale >= 1) {
						cell.isAnimating = false;
						scale.Value = 1f;
						cell.scale = 1f;
					}
				}
			});*/
		}
	}

	public void DisableEntity(Int2 index) {
		Entities.WithAll<ICell>().ForEach((Entity e, ref ICell cell) => {
			if(cell.index == index) {
				GameCore.EntityManager.AddComponent<Disabled>(e);
			}
		});
	}

	public void SetRenderMesh(RenderMesh rend, Int2 index) {
		Entities.WithAll<ICell, RenderMesh>().ForEach((Entity e, ref ICell cell) => {
			if(cell.index == index) {
				GameCore.EntityManager.AddComponent<Disabled>(e);
			}
		});
		
	}
}
