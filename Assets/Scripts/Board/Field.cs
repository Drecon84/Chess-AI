using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field {

	// Should this just be a pair of ints rather than a Vector2???
	public Vector2Int position;
	public Piece piece;

	public Field(int x, int y){
		position = new Vector2Int(x,y);
	}

}
