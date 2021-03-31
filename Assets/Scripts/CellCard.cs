using Sperlich.Vectors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellCard : MonoBehaviour  {

	public bool isDragged;
	public CellType card;
	public Team.Teams team;
	public Image sprite;
	public Image border;
	private Transform menu;
	private Transform container;
	private Vector2 origin;
	private int childIndex;
	private Transform placeholder;
	private float scale;

	private void Awake() {
		var trigger = gameObject.AddComponent<EventTrigger>();
		var pointerDown = new EventTrigger.Entry {
			eventID = EventTriggerType.PointerDown
		};
		var pointerUp = new EventTrigger.Entry {
			eventID = EventTriggerType.PointerUp
		};
		pointerDown.callback.AddListener((data) => {
			isDragged = true;
			childIndex = transform.GetSiblingIndex();
			placeholder = new GameObject().transform;
			placeholder.SetParent(container);
			placeholder.SetSiblingIndex(childIndex);
			placeholder.name = name + "_placeholder";
			placeholder = placeholder.gameObject.AddComponent<RectTransform>();
			transform.SetParent(menu);
			border.enabled = false;
			scale = 0.5f;
		});
		pointerUp.callback.AddListener((data) => {
			Destroy(placeholder.gameObject);

			if(GameCore.activeMap.ReplaceableCells.Exists(c => c.Index == GameCore.CardPlaceIndex)) {
				var cell = Cell.CreateCell(card, GameCore.CardPlaceIndex);
				cell.ActiveTeam = team;

				GameCore.activeMap.SetCell(cell, false);
				cell.CaptureValue = 100;
				RemoveFromDeck();
			} else {
				isDragged = false;
				transform.SetParent(container);
				transform.position = origin;
				transform.SetSiblingIndex(childIndex);
				scale = 1f;
				border.enabled = true;
				transform.localScale = new Vector3(1f, 1f, 1f);
			}
		});
		trigger.triggers.Add(pointerDown);
		trigger.triggers.Add(pointerUp);
		container = transform.parent;
		menu = GameObject.FindGameObjectWithTag("Menu").transform;
		origin = transform.position;
	}

	private void Update() {
		if(isDragged) {
			if(!GameCore.activeMap.ReplaceableCells.Exists(c => c.Index == GameCore.CardPlaceIndex)) {
				transform.position = Input.mousePosition;
			} else {
				transform.position = Camera.main.WorldToScreenPoint(GameCore.CardPlaceIndex.Vector2);
			}
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scale, scale, scale), Time.deltaTime * 10);
		}
	}

	public void SetCard(CellType type, Team.Teams team) {
		this.team = team;
		card = type;
		sprite.sprite = GameCore.GetCellData(type).sprite;
	}

	public void RemoveFromDeck() {
		GameCore.activeMap.GetPlayerTeam().Cards.Remove(card);
		Destroy(gameObject);
	}

}
