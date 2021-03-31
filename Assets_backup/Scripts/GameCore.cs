using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Sperlich.Vectors;

public class GameCore : MonoBehaviour {

	public bool loadDebug;
	public bool showCellDebug;
	public float spreadMultiplier;
	public float totalCapturableCount;
	private Dictionary<Team.Teams, (Slider, TMP_Text)> teamCompletionSliders;
	private GameObject deckCardPrefab;
	public Canvas gameUI;
	private Transform deckContainer;
	public static LevelMap activeMap;
	public static Int2 CardPlaceIndex;
	[SerializeField] private Material _spriteMat;
    [SerializeField] private List<CellData> _cellsDatas = new List<CellData>();
	[SerializeField] private Mesh _spriteMesh;
	[SerializeField] private AnimationCurve _spriteSpawnAnimationCurve;
	public static List<Team> Teams => activeMap.teams;
	public static AnimationCurve SpriteSpawnAnimationCurve { get; set; }
	public static Material SpriteMaterial { get; set; }
	public static EntityFactory Factory { get; set; }
	public static Mesh SpriteMesh { get; set; }
	public static List<CellData> Cells { get; set; }
	public static bool Initialized { get; set; }
	public static bool IsPaused { get; set; } = false;
	public static Color32 BackgroundColor => new Color32(207, 198, 181, 255);
	public static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

	private void Awake() {
		Cells = _cellsDatas;
		SpriteMaterial = _spriteMat;
		SpriteMesh = _spriteMesh;
		SpriteSpawnAnimationCurve = _spriteSpawnAnimationCurve;
		Initialized = false;
		IsPaused = false;
		Factory = new GameObject().AddComponent<EntityFactory>();
		teamCompletionSliders = new Dictionary<Team.Teams, (Slider, TMP_Text)>();
		deckContainer = GameObject.FindGameObjectWithTag("CellDeck").transform;
		deckCardPrefab = Resources.Load<GameObject>("Prefabs/UI/Card");
		if(loadDebug) {
			FindObjectOfType<LevelEditor>().LoadLevel();
		}
	}

	public void Initialize(LevelMap map) {
		activeMap = map;
		CellSystem.activeMap = map;
		FindObjectOfType<LevelEditor>().map = activeMap;
		LevelEditor.AllowEditing = false;
		StartCoroutine(IInitialize());
	}

	private IEnumerator IInitialize() {
		foreach(Transform t in deckContainer) {
			Destroy(t.gameObject);
		}
		yield return new WaitForSeconds(0.1f);
		var slider = Resources.Load<GameObject>("Prefabs/TeamProgress");
		foreach(Team t in Teams) {
			activeMap.SetCell(new CellDefault() {
				ActiveTeam = t.team,
				CaptureValue = 100f,
				IsAnimating = true,
				Id = CellType.Core,
				Index = t.startIndex
			});

			var teamSlider = Instantiate(slider, GameObject.FindGameObjectWithTag("TeamProgressBar").transform);
			teamSlider.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().color = t.GetColor();
			teamCompletionSliders.Add(t.team, (teamSlider.GetComponentInChildren<Slider>(), teamSlider.GetComponentInChildren<TMP_Text>()));
		}
		activeMap.RenderFullMap();
		if(showCellDebug) {
			var prefab = Resources.Load<GameObject>("Prefabs/CellDebug");
			foreach(Cell c in activeMap.list.Where(c => c.CanBeCaptured)) {
				TMP_Text t = Instantiate(prefab, transform).GetComponentInChildren<TMP_Text>();
				t.SetText(c.CaptureValue + "");
				t.transform.position = new Vector3(c.Index.x, 1, c.Index.y);
				c.debugShowValue = t;
			}
		}
		totalCapturableCount = activeMap.list.Where(c => c.CanBeCaptured).ToList().Count;
		yield return new WaitForSeconds(1f);
		// Give Cards to players
		foreach(Team t in Teams) {
			t.Cards = new List<CellType>(activeMap.startDeck);
		}
		foreach(CellType card in activeMap.startDeck) {
			GiveCard(card, activeMap.GetPlayerTeam().team);
		}
		Initialized = true;
	}

	public void GiveCard(CellType type, Team.Teams team) {
		var card = Instantiate(deckCardPrefab, deckContainer).GetComponent<CellCard>();
		card.SetCard(type, team);
	}

	public void BackToMenu() {
		EntityManager.GetAllEntities().ToList().ForEach(e => {
			if(EntityManager.HasComponent<ICell>(e)) {
				EntityManager.DestroyEntity(e);
			}
		});
		SceneManager.LoadScene("Menu", LoadSceneMode.Single);
	}

	private void Update() {
		if(Initialized && IsPaused == false) {
			foreach(Cell cell in activeMap) {
				if(cell.IsAssignedToTeam()) {
					cell.Spread(spreadMultiplier);
				}
			}
			foreach(Team t in Teams) {
				float teamCount = activeMap.list.Where(c => c.ActiveTeam == t.team).ToList().Count;
				float percent = Mathf.Round(teamCount / totalCapturableCount * 1000f) / 10;
				teamCompletionSliders[t.team].Item1.value = percent;
				teamCompletionSliders[t.team].Item2.SetText($"{percent}%");
			}

			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePos.y = mousePos.z;
			CardPlaceIndex = new Int2(mousePos);
		}
	}

	public static Team GetTeam(Team.Teams team) => Teams.Find(t => t.team == team);

	public static CellData GetCellData(CellType type) => Cells.Find(c => c.type == type);
}
