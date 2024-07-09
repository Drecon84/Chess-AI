using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BoardManager : MonoBehaviour {

	public Board board;

	public UIField[,] uiBoard;

	public Sprite pawn0;
	public Sprite pawn1;
	public Sprite knight0;
	public Sprite knight1;
	public Sprite bishop0;
	public Sprite bishop1;
	public Sprite rook0;
	public Sprite rook1;
	public Sprite queen0;
	public Sprite queen1;
	public Sprite king0;
	public Sprite king1;

	public UIField A1, A2, A3, A4, A5, A6, A7, A8;
	public UIField B1, B2, B3, B4, B5, B6, B7, B8;
	public UIField C1, C2, C3, C4, C5, C6, C7, C8;
	public UIField D1, D2, D3, D4, D5, D6, D7, D8;
	public UIField E1, E2, E3, E4, E5, E6, E7, E8;
	public UIField F1, F2, F3, F4, F5, F6, F7, F8;
	public UIField G1, G2, G3, G4, G5, G6, G7, G8;
	public UIField H1, H2, H3, H4, H5, H6, H7, H8;


	// Use this for initialization
	public void SetupBoard () {
		uiBoard = new UIField[8,8];
		uiBoard[0,0] = A1;
		uiBoard[0,1] = A2;
		uiBoard[0,2] = A3;
		uiBoard[0,3] = A4;
		uiBoard[0,4] = A5;
		uiBoard[0,5] = A6;
		uiBoard[0,6] = A7;
		uiBoard[0,7] = A8;
		uiBoard[1,0] = B1;
		uiBoard[1,1] = B2;
		uiBoard[1,2] = B3;
		uiBoard[1,3] = B4;
		uiBoard[1,4] = B5;
		uiBoard[1,5] = B6;
		uiBoard[1,6] = B7;
		uiBoard[1,7] = B8;
		uiBoard[2,0] = C1;
		uiBoard[2,1] = C2;
		uiBoard[2,2] = C3;
		uiBoard[2,3] = C4;
		uiBoard[2,4] = C5;
		uiBoard[2,5] = C6;
		uiBoard[2,6] = C7;
		uiBoard[2,7] = C8;
		uiBoard[3,0] = D1;
		uiBoard[3,1] = D2;
		uiBoard[3,2] = D3;
		uiBoard[3,3] = D4;
		uiBoard[3,4] = D5;
		uiBoard[3,5] = D6;
		uiBoard[3,6] = D7;
		uiBoard[3,7] = D8;
		uiBoard[4,0] = E1;
		uiBoard[4,1] = E2;
		uiBoard[4,2] = E3;
		uiBoard[4,3] = E4;
		uiBoard[4,4] = E5;
		uiBoard[4,5] = E6;
		uiBoard[4,6] = E7;
		uiBoard[4,7] = E8;
		uiBoard[5,0] = F1;
		uiBoard[5,1] = F2;
		uiBoard[5,2] = F3;
		uiBoard[5,3] = F4;
		uiBoard[5,4] = F5;
		uiBoard[5,5] = F6;
		uiBoard[5,6] = F7;
		uiBoard[5,7] = F8;
		uiBoard[6,0] = G1;
		uiBoard[6,1] = G2;
		uiBoard[6,2] = G3;
		uiBoard[6,3] = G4;
		uiBoard[6,4] = G5;
		uiBoard[6,5] = G6;
		uiBoard[6,6] = G7;
		uiBoard[6,7] = G8;
		uiBoard[7,0] = H1;
		uiBoard[7,1] = H2;
		uiBoard[7,2] = H3;
		uiBoard[7,3] = H4;
		uiBoard[7,4] = H5;
		uiBoard[7,5] = H6;
		uiBoard[7,6] = H7;
		uiBoard[7,7] = H8;

		UpdateBoard();
	}
	
	public void UpdateBoard(){
		foreach(Field f in board.gameBoard){
			Image img = uiBoard[f.position.x, f.position.y].transform.GetChild(0).GetComponent<Image>();
			Color clr = new Color (1,1,1,1);
			if(f.piece != null){
				switch (f.piece.type){
					case 0: 
						if(f.piece.colorIsWhite){
							img.sprite = pawn0;
						}
						else{
							img.sprite = pawn1;
						}
						img.color = clr;
						break;
					case 1:
						if(f.piece.colorIsWhite){
							img.sprite = knight0;
						}
						else{
							img.sprite = knight1;
						}
						img.color = clr;
						break;
					case 2:
						if(f.piece.colorIsWhite){
							img.sprite = bishop0;
						}
						else{
							img.sprite = bishop1;
						}
						img.color = clr;
						break;
					case 3:
						if(f.piece.colorIsWhite){
							img.sprite = rook0;
						}
						else{
							img.sprite = rook1;
						}
						img.color = clr;
						break;
					case 4:
						if(f.piece.colorIsWhite){
							img.sprite = queen0;
						}
						else{
							img.sprite = queen1;
						}
						img.color = clr;
						break;
					case 5:
						if(f.piece.colorIsWhite){
							img.sprite = king0;
						}
						else{
							img.sprite = king1;
						}
						img.color = clr;
						break;
				}
			}
			else{
				img.sprite = null;
				img.color = new Color(1,1,1,0);
			}
		}
	}
}
