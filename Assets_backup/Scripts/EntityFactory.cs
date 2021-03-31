using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;

public class EntityFactory : MonoBehaviour {

	public static Dictionary<string, Entity> EntityPrefabs { get; set; }

	public void Awake() {
		var objects = new List<GameObject>();
		EntityPrefabs = new Dictionary<string, Entity>();
		foreach(GameObject o in FindObjectsOfType<GameObject>().Where(o => o.CompareTag("EntityPrefab"))) {
			if(!EntityPrefabs.ContainsKey(o.name.ToLower())) {
				o.transform.position = new Vector3();
				Entity e = GameObjectConversionUtility.ConvertGameObjectHierarchy(o, new GameObjectConversionSettings(World.DefaultGameObjectInjectionWorld, GameObjectConversionUtility.ConversionFlags.AddEntityGUID));
				GameCore.EntityManager.AddComponentData(e, new Disabled());
				EntityPrefabs.Add(o.name.ToLower(), e);
				objects.Add(o);
				//Debug.Log("Added Entity Prefab: " + o.name.ToLower());
			}
		}
		while(objects.Count > 0) {
			Destroy(objects[0]);
			objects.RemoveAt(0);
		}
		Destroy(gameObject);
	}

	public static Entity GetPrefab(string name) => EntityPrefabs[name.ToLower()];

	public static bool PrefabExists(string name) => EntityPrefabs.ContainsKey(name.ToLower());
}
