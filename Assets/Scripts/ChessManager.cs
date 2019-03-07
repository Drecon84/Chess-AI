using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessManager : MonoBehaviour {

	// TODO: player loses the game when time runs out

	public Board board;
	public BoardManager boardManager;

	public TournamentManager tournamentManager;

	public bool whiteTurn;

	public bool pieceSelected;
	public Field selectPiece;

	public UIField highlightedField;

	public Text player1Name;
	public Text player2Name;

	public Text player1Timer;

	public Text player2Timer;

	public int timerP1;
	public int timerP2;
	public bool timerOn = false;

	public bool gameActive;

	public Button promote1Button, promote2Button, promote3Button, promote4Button;
	public int promote;

	public bool tryMove;

	public bool movingPiece = false;


	void Start () {
		//StartNewGame(60*5);
	}

	public void StartNewGame(int playerTime){
		whiteTurn = true;
		board = new Board();
		board.BuildBoard();
		SetUpBoard();
		boardManager.board = board;
		boardManager.SetupBoard();
		player1Name.color = Color.blue;
		player2Name.color = Color.black;
		timerP1 = playerTime;
		timerP2 = playerTime;
		DisplayTime();
		gameActive = true;
	}

	void Update(){
		if(!timerOn && gameActive){
			StartCoroutine("CountSecond");
			timerOn = true;
		}
		if(timerP1 == 0 && gameActive){
			player1Name.color = Color.red;
			tournamentManager.PlayerLostGame(true);
			gameActive = false;
		}
		if(timerP2 == 0 && gameActive){
			player2Name.color = Color.red;
			tournamentManager.PlayerLostGame(false);
			gameActive = false;
		}
	}

	public void DisplayTime(){
		int tempTime = timerP1;
		int temp2 = 0;
		while(tempTime >= 60){
			tempTime -= 60;
			temp2++;
		}
		string tempStr;
		if(tempTime < 10){
			tempStr = "0" + tempTime.ToString();
		}
		else{
			tempStr = tempTime.ToString();
		}
		string toText = temp2.ToString() + ":" + tempStr;
		player1Timer.text = toText;
		tempTime = timerP2;
		temp2 = 0;
		while(tempTime >= 60){
			tempTime -= 60;
			temp2++;
		}
		if(tempTime < 10){
			tempStr = "0" + tempTime.ToString();
		}
		else{
			tempStr = tempTime.ToString();
		}
		toText = temp2.ToString() + ":" + tempStr;
		player2Timer.text = toText;
	}
	
	public void SetUpBoard(){
		board.gameBoard[0,0].piece = new Rook(true, new Vector2Int(0,0));
		board.gameBoard[1,0].piece = new Knight(true, new Vector2Int(1,0));
		board.gameBoard[2,0].piece = new Bishop(true,new Vector2Int(2,0));
		board.gameBoard[3,0].piece = new Queen(true, new Vector2Int(3,0));
		board.gameBoard[4,0].piece = new King(true, new Vector2Int(4,0));
		board.gameBoard[5,0].piece = new Bishop(true,new Vector2Int(5,0));
		board.gameBoard[6,0].piece = new Knight(true, new Vector2Int(6,0));
		board.gameBoard[7,0].piece = new Rook(true, new Vector2Int(7,0));
		board.gameBoard[0,1].piece = new Pawn(true, new Vector2Int(0,1));
		board.gameBoard[1,1].piece = new Pawn(true, new Vector2Int(1,1));
		board.gameBoard[2,1].piece = new Pawn(true, new Vector2Int(2,1));
		board.gameBoard[3,1].piece = new Pawn(true, new Vector2Int(3,1));
		board.gameBoard[4,1].piece = new Pawn(true, new Vector2Int(4,1));
		board.gameBoard[5,1].piece = new Pawn(true, new Vector2Int(5,1));
		board.gameBoard[6,1].piece = new Pawn(true, new Vector2Int(6,1));
		board.gameBoard[7,1].piece = new Pawn(true, new Vector2Int(7,1));
		board.gameBoard[0,6].piece = new Pawn(false, new Vector2Int(0,6));
		board.gameBoard[1,6].piece = new Pawn(false, new Vector2Int(1,6));
		board.gameBoard[2,6].piece = new Pawn(false, new Vector2Int(2,6));
		board.gameBoard[3,6].piece = new Pawn(false, new Vector2Int(3,6));
		board.gameBoard[4,6].piece = new Pawn(false, new Vector2Int(4,6));
		board.gameBoard[5,6].piece = new Pawn(false, new Vector2Int(5,6));
		board.gameBoard[6,6].piece = new Pawn(false, new Vector2Int(6,6));
		board.gameBoard[7,6].piece = new Pawn(false, new Vector2Int(7,6));
		board.gameBoard[0,7].piece = new Rook(false, new Vector2Int(0,7));
		board.gameBoard[1,7].piece = new Knight(false, new Vector2Int(1,7));
		board.gameBoard[2,7].piece = new Bishop(false, new Vector2Int(2,7));
		board.gameBoard[3,7].piece = new Queen(false, new Vector2Int(3,7));
		board.gameBoard[4,7].piece = new King(false, new Vector2Int(4,7));
		board.gameBoard[5,7].piece = new Bishop(false, new Vector2Int(5,7));
		board.gameBoard[6,7].piece = new Knight(false, new Vector2Int(6,7));
		board.gameBoard[7,7].piece = new Rook(false, new Vector2Int(7,7));

		promote1Button.GetComponent<Image>().color = Color.gray;
		promote2Button.GetComponent<Image>().color = Color.gray;
		promote3Button.GetComponent<Image>().color = Color.gray;
		promote4Button.GetComponent<Image>().color = Color.white;
	}

	public void FieldClicked(UIField uiField, Vector2Int position){
		if(gameActive){
			if(board.gameBoard[position.x, position.y].piece != null){
				// If no piece has been selected, select this piece
				if(!pieceSelected){
					pieceSelected = true;
					selectPiece = board.gameBoard[position.x, position.y];
					highlightedField = uiField;
					highlightedField.selectField();
					return;
				}
				else{
					// undo selecting the piece
					if(uiField.fieldPosition == selectPiece.position){
						pieceSelected = false;
						selectPiece = null;
						highlightedField.unSelectField();
						return;
					}
				}
			}
			// Try to move the piece to the selected field
			if(!movingPiece){
				movingPiece = true;
				StartCoroutine(MakeMove(whiteTurn, selectPiece.position, uiField.fieldPosition));
			}
			//bool tryMove = board.MakeMove(whiteTurn, selectPiece.position, uiField.fieldPosition);
			// Probably end the game when doing a game with AI?
			if(tryMove){
				PassTurn();
			}
			pieceSelected = false;
			selectPiece = null;
			highlightedField.unSelectField();
			boardManager.UpdateBoard();
			if(board.CheckIfCheckMate(whiteTurn)){
				print("Checkmate Atheists!");
				board.moveList.Add(new Move(board.moveNum, whiteTurn, true));
				if(whiteTurn){
					player1Name.color = Color.red;
					tournamentManager.PlayerLostGame(true);
					gameActive = false;
				}
				else{
					player2Name.color = Color.red;
					tournamentManager.PlayerLostGame(false);
					gameActive = false;
				}
			}
		}
	}

	public void SetPlayerNames(string p1Name, string p2Name){
		player1Name.text = p1Name;
		player2Name.text = p2Name;
	}

	public void MoveHelper(bool whiteTurn, Vector2Int startPos, Vector2Int endPos){
		if(!movingPiece){
			movingPiece = true;
			StartCoroutine(MakeMove(whiteTurn, startPos, endPos));
		}
	}

	public IEnumerator MakeMove(bool whiteTurn, Vector2Int startPos, Vector2Int endPos){
		Debug.Log("Moving from " + startPos + " to " + endPos);
		tryMove = board.MakeMove(whiteTurn, startPos, endPos);
		yield return new WaitForEndOfFrame();
		if(tryMove){
			tournamentManager.PlayerLostGame(whiteTurn);
			PassTurn();
		}
		else {
			tournamentManager.PlayerLostGame(whiteTurn);
			gameActive = false;
		}
		movingPiece = false;
		yield return null;

	}

	public void PassTurn(){
		whiteTurn = !whiteTurn;
		if(whiteTurn){
			player1Name.color = Color.blue;
			player2Name.color = Color.black;
		}
		else{
			player1Name.color = Color.black;
			player2Name.color = Color.blue;
		}
		StopAllCoroutines();
		timerOn = false;
	}

	public void PromoteTarget(int type){
		promote = type;
		board.promotePawnTo(type);
		if(type == 1){
			promote1Button.GetComponent<Image>().color = Color.white;
			promote2Button.GetComponent<Image>().color = Color.gray;
			promote3Button.GetComponent<Image>().color = Color.gray;
			promote4Button.GetComponent<Image>().color = Color.gray;
		}
		else if(type == 2){
			promote1Button.GetComponent<Image>().color = Color.gray;
			promote2Button.GetComponent<Image>().color = Color.white;
			promote3Button.GetComponent<Image>().color = Color.gray;
			promote4Button.GetComponent<Image>().color = Color.gray;
		}
		else if(type == 3){
			promote1Button.GetComponent<Image>().color = Color.gray;
			promote2Button.GetComponent<Image>().color = Color.gray;
			promote3Button.GetComponent<Image>().color = Color.white;
			promote4Button.GetComponent<Image>().color = Color.gray;
		}
		else if(type == 4){
			promote1Button.GetComponent<Image>().color = Color.gray;
			promote2Button.GetComponent<Image>().color = Color.gray;
			promote3Button.GetComponent<Image>().color = Color.gray;
			promote4Button.GetComponent<Image>().color = Color.white;
		}
	}

	public IEnumerator CountSecond(){
		yield return new WaitForSeconds(1);
		if(whiteTurn){
			timerP1--;
		}
		else{
			timerP2--;
		}
		timerOn = false;
		DisplayTime();
	}

}
