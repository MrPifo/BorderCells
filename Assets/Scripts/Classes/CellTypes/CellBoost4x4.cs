using Sperlich.Vectors;
using UnityEngine;
using System;
using System.Collections.Generic;

public class CellBoost4x4 : Cell, IEquatable<Cell> {

	public int Radius { get; set; } = 4;
	public float SpreadBoost { get; set; } = 2;
	public List<Int2> neighbours = new List<Int2>();

	public CellBoost4x4() : base() { }
	public CellBoost4x4(Int2 index) : base(index) { }
	public CellBoost4x4(Int2 index, CellType type) : base(index, type) { }

	public override void OnCreate() {
		foreach(Cell c in GetNeighbours(2)) {
			int dist = Int2.Distance(Index, c.Index);
			float b = (1f / Radius) * dist + 0.2f;
			if(dist != 0) {
				c.externSpreadMultiplier *= SpreadBoost;
				if(c.IsAffectedByOtherCell) {
					c.SetBrightness(c.brightness - b, 0.1f);
				} else {
					c.SetBrightness(b, 0.1f);
				}
			}
			c.IsAffectedByOtherCell = true;
		}
		SetBrightness(0.3f);
	}

	public override void OnDelete() {
		
	}

	public new bool Equals(Cell other) {
		if(other.Index == Index) return true;
		return false;
	}
}
