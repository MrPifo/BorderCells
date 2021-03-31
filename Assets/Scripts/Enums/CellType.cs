
public enum CellType : int {
	Disabled,	// Cannot be captured from a team
	Empty,		// Can be captured from a team
	Core,		// A teams core
	Border,		// Border, cannot be taken from a team
	Boost4x4,	// Boosts block spreading in a 4x4 radius
}
