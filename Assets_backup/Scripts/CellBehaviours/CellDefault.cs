using Sperlich.Vectors;
using System;
using UnityEngine;

[Serializable]
public class CellDefault : Cell, IEquatable<Cell> {

	public CellDefault() : base() { }

	public CellDefault(Int2 index) : base(index) { }

	public CellDefault(Int2 index, CellType type) : base(index, type) {

	}

	public override void ApplyEffect() {
		
	}

	public override float ModifySpreadSpeed() => 1f;

	public override void OnCreate() {
		Debug.Log("<color=green>Default</color>");
	}

	public override void OnDelete() {
		
	}

	public new bool Equals(Cell other) {
		if(other.Index == Index) return true;
		return false;
	}
}
