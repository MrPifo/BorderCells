using System;


public interface ICellEffect {

	public void OnCreate();
	public void ApplyEffect();
	public float ModifySpreadSpeed();
	public void OnDelete();

}
