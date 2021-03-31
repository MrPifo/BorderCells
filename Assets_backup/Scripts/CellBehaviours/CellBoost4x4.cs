using Sperlich.Vectors;
using UnityEngine;
using System;

public class CellBoost4x4 : Cell, IEquatable<Cell> {

	public int Radius { get; set; } = 4;

	public CellBoost4x4() : base() { }
	public CellBoost4x4(Int2 index) : base(index) { }
	public CellBoost4x4(Int2 index, CellType type) : base(index, type) { }

	public override void OnCreate() {
		Debug.Log("<color=blue>Boost</color>");
	}

	public override void ApplyEffect() {
		
	}

	public override void OnDelete() {
		
	}

	public override float ModifySpreadSpeed() => 1f;

	public new bool Equals(Cell other) {
		if(other.Index == Index) return true;
		return false;
	}
}
