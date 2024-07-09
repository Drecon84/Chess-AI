using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIField : MonoBehaviour, IPointerClickHandler {

	public Vector2Int fieldPosition;
	public ChessManager chessManager;

	Color oldColor;

	public void OnPointerClick(PointerEventData data){
		chessManager.FieldClicked(this, fieldPosition);
	}

	public void selectField(){
		oldColor = gameObject.GetComponent<Image>().color;
		gameObject.GetComponent<Image>().color = Color.red;
	}

	public void unSelectField(){
		gameObject.GetComponent<Image>().color = oldColor;
	}
}
