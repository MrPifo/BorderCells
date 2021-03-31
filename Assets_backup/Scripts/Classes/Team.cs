using Sperlich.Vectors;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
[DataContract]
public class Team {
	[DataMember] public Teams team;
	[DataMember] public TeamType type;
	[DataMember] public Int2 startIndex;    // Where the team starts on the map
	[DataMember] public float spreadSpeed = 1f;
	[DataMember] public List<CellType> Cards = new List<CellType>();

	public Color GetColor() => TeamColors[(int)team];

	public static Color GetColor(Teams team) => TeamColors[(int)team];

	public static Color[] TeamColors = new Color[] {
		GameCore.BackgroundColor,
		GameCore.BackgroundColor,
		new Color32(255, 85, 70, 255),
		new Color32(75, 200, 255, 255),
		new Color32(235, 199, 40, 255),
		new Color32(125, 235, 40, 255)
	};

	public enum Teams : int {
		Disabled = 0,
		None = 1,
		Red = 2,
		Blue = 3,
		Yellow = 4,
		Green = 5,
	}

	public enum TeamType : int {
		Player,
		Bot,
	}
}
