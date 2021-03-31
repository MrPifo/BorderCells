using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

    public static Dictionary<string, GameObject> Menus { get; set; }
	public static Transform LevelMapsContainer { get; set; }
	public Sprite emptyStar;
	public Sprite filledStar;
	private LevelMap selectedLevelMap;

	private void Awake() {
		Menus = new Dictionary<string, GameObject>();
		LevelMapsContainer = GameObject.Find("Levels").transform;

		foreach(GameObject o in FindObjectsOfType<GameObject>().Where(o => o.CompareTag("Menu"))) {
			Menus.Add(o.name.ToLower(), o);
		}
		EnableMainMenu();

		// Setup Low Resolution Camera
		
	}

	public void LoadAndDisplayAllLevelMap() {
		foreach(Transform t in LevelMapsContainer) {
			Destroy(t.gameObject);
		}
		int count = 0;
		foreach(TextAsset file in Resources.LoadAll<TextAsset>("Levels/")) {
			var map = JsonConvert.DeserializeObject<LevelMap>(file.text, new JsonSerializerSettings() {
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto
			});
			var obj = Instantiate(Resources.Load<GameObject>("Prefabs/UI/LevelMap"), LevelMapsContainer).transform;

			// Set Level Information
			obj.name = $"Level_{map.levelName}";
			obj.GetChild(0).GetComponent<TMP_Text>().SetText(map.levelName);
			obj.GetChild(1).GetComponent<Image>().sprite = map.completionStars[0] ? filledStar : emptyStar;
			obj.GetChild(2).GetComponent<Image>().sprite = map.completionStars[1] ? filledStar : emptyStar;
			obj.GetChild(3).GetComponent<Image>().sprite = map.completionStars[2] ? filledStar : emptyStar;
			obj.GetChild(4).GetComponent<Image>().enabled = false;
			StartCoroutine(FillAnimation(obj.GetComponent<Image>(), 0, 1, 0.05f, count));

			// Add trigger
			var playBtnImg = obj.GetChild(5).GetComponent<Image>();
			playBtnImg.material = new Material(playBtnImg.material);
			var trigger = playBtnImg.gameObject.AddComponent<EventTrigger>();
			var enter = new EventTrigger.Entry {
				eventID = EventTriggerType.PointerEnter
			};
			var exit = new EventTrigger.Entry {
				eventID = EventTriggerType.PointerExit
			};
			enter.callback.AddListener((data) => {
				StartCoroutine(FillAnimation(playBtnImg, 0, 1, 0.1f));
				selectedLevelMap = map;
			});
			exit.callback.AddListener((data) => {
				StartCoroutine(FillAnimation(playBtnImg, 1, 0, -0.2f));
			});
			trigger.triggers.Add(enter);
			trigger.triggers.Add(exit);
			playBtnImg.GetComponent<Button>().onClick.AddListener(StartLevel);

			// Display Teams
			var teamContainer = obj.GetChild(6);
			foreach(Team team in map.teams) {
				var t = Instantiate(Resources.Load<GameObject>("Prefabs/UI/LevelTeam"), teamContainer).transform;
				t.GetComponent<Image>().color = team.GetColor();
			}

			count++;
		}
	}

	public void StartLevel() {
		DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += SwitchToGame;
		SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
	}

	public void SwitchToGame(Scene scene, LoadSceneMode mode) {
		if(scene.name == "Game") {
			FindObjectOfType<GameCore>().Initialize(selectedLevelMap);
			LevelMapsContainer = null;
			Menus = null;
			SceneManager.sceneLoaded -= SwitchToGame;
			Destroy(gameObject);
		}
	}

	public void EnableMainMenu() {
		DisableAllMenus();
		EnableMenu("MainMenu");
	}

	public void SwitchToArcadeMenu() {
		DisableAllMenus();
		LoadAndDisplayAllLevelMap();
		EnableMenu("ArcadeMenu");
		
	}

    public void SwitchBackToMenu() {
		EnableMainMenu();
	}

	public IEnumerator FillAnimation(Image img, float start, float targetValue = 1, float speed = 0.05f, int offset = 0) {
		var mat = new Material(img.material);
		img.material = mat;
		mat.SetFloat("_Fill", start);
		yield return new WaitForSeconds(offset / 10f);
		while((mat.GetFloat("_Fill") < targetValue && start < targetValue) || (mat.GetFloat("_Fill") > targetValue && start > targetValue)) {
			mat.SetFloat("_Fill", mat.GetFloat("_Fill") + speed);
			yield return new WaitForSeconds(Time.fixedDeltaTime);
		}
	}

	public void LevelMapPlayFillAnimation(Image img) => StartCoroutine(FillAnimation(img, 0));

	public void DisableAllMenus() => Menus.Values.ToList().ForEach(o => o.SetActive(false));
	public void EnableAllMenus() => Menus.Values.ToList().ForEach(o => o.SetActive(true));
	public GameObject GetMenu(string name) => Menus[name.ToLower()];
	public void EnableMenu(string name) => GetMenu(name.ToLower()).SetActive(true);
	public void DisableMenu(string name) => GetMenu(name.ToLower()).SetActive(false);
}
