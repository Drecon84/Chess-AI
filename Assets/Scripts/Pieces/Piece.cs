using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece {

	public int type;

	public Vector2Int posOnBoard;
	public bool colorIsWhite;

	public Piece (){

	}

	public Piece(int tp, bool color, Vector2Int pos){
		type = tp;
		posOnBoard = pos;
		colorIsWhite = color;
	}
}
