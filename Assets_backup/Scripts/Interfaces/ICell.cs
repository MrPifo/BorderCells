using System;
using Unity.Entities;
using Sperlich.Vectors;

[Serializable]
public struct ICell : IComponentData {

	public Int2 index;
	public bool isAnimating;
	public float scale;
	public int number;
	public Team.TeamType team;

}
