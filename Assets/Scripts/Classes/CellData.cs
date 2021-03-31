using System;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;


[CreateAssetMenu(fileName = "Cell", menuName = "CellData", order = 1)]
[Serializable]
[DataContract]
public class CellData : ScriptableObject {

	[DataMember] public CellType type;
	public bool IsCapturable = true;
	public bool CanSpread = false;
	public bool IsStatic = false;
	public float SpreadSpeed = 1f;
	public Sprite sprite;

	public string Name => type.ToString();

	public CellData() {
			
	}
}