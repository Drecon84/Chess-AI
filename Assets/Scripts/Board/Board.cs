using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board {

	public int promoteTo = 4;

	public Field[,] gameBoard;

	private Piece checkingPiece;

	public List<Move> moveList;

	public int moveNum = 0;

	int[] whitePieces;
	int[] blackPieces;

	public void BuildBoard(){
		gameBoard = new Field[8,8];
		for(int i = 0; i < 8; i++){
			for(int j = 0; j < 8; j++){
				gameBoard[i,j] = new Field(i,j);
			}
		}
		moveList = new List<Move>();
		whitePieces = new int[] {8,2,2,2,1,1};
		blackPieces = new int[] {8,2,2,2,1,1};
	}

	public void promotePawnTo(int pro){
		promoteTo = pro;
	}

	public bool MakeMove(bool whiteTurn, Vector2Int startPos, Vector2Int endPos){
		Field startField = gameBoard[startPos.x, startPos.y];
		Field endField = gameBoard[endPos.x, endPos.y];
		if(startField.piece == null){
			return false;
		}
		if(startField.piece.colorIsWhite != whiteTurn){
			//Debug.Log("Illegal move: Trying to move opponent's piece!");
			return false;
		}
		if(!CheckLegalMove(whiteTurn, startPos, endPos)){
			return false;
		}
		bool takePiece = endField.piece != null;
		// Check castling first
		if(startField.piece.type == 5 && Math.Abs(startField.position.x - endField.position.x) == 2){
			if(startField.position.x < endField.position.x){
				endField.piece = startField.piece;
				startField.piece = null;
				gameBoard[endPos.x - 1, endPos.y].piece = gameBoard[endPos.x + 1, endPos.y].piece;
				gameBoard[endPos.x + 1, endPos.y].piece = null;
				moveList.Add(new Move(moveNum, whiteTurn, 5, true, endPos.x, endPos.y));
			}
			else {
				endField.piece = startField.piece;
				startField.piece = null;
				gameBoard[endPos.x + 1, endPos.y].piece = gameBoard[endPos.x - 2, endPos.y].piece;
				gameBoard[endPos.x - 2, endPos.y].piece = null;
				moveList.Add(new Move(moveNum, whiteTurn, 5, true, endPos.x, endPos.y));
			}
		}
		// execute en passant
		else if(startField.piece.type == 0 && (startPos.x == endPos.x-1 || startPos.x == endPos.x+1) && endField.piece == null){
			endField.piece = startField.piece;
			startField.piece = null;
			if(whiteTurn){
				blackPieces[0]--;
				gameBoard[endField.position.x, endField.position.y-1].piece = null;
				moveList.Add(new Move(moveNum, whiteTurn, 0, startField.position.x, startField.position.y, endField.position.x, endField.position.y, true, true, CheckIfKingInCheck(whiteTurn)));
			}
			else{
				whitePieces[0]--;
				gameBoard[endField.position.x, endField.position.y+1].piece = null;
				moveList.Add(new Move(moveNum, whiteTurn, 0,  startField.position.x, startField.position.y, endField.position.x, endField.position.y, true, true, CheckIfKingInCheck(whiteTurn)));
			}
		}
		else if(startField.piece.type == 0 && endField.position.y == 7 && whiteTurn || 
				startField.piece.type == 0 && endField.position.y == 0 && !whiteTurn){
			ExecutePawnPromote(whiteTurn, takePiece, startField, endField);
		}
		else{
			ExecuteMove(whiteTurn, takePiece, startField, endField);
		}
		if(!whiteTurn){
			moveNum++;
		}

		return true;
	}

	public bool CheckLegalMove(bool whiteTurn, Vector2Int startPos, Vector2Int endPos){
		// for each different piece type: check if it's one of the possible moves

		int pieceType = gameBoard[startPos.x, startPos.y].piece.type;
		Field startField = gameBoard[startPos.x, startPos.y];
		Field endField = gameBoard[endPos.x, endPos.y];
		Piece tempPiece;

		// Pawn move
		switch (pieceType){
			// Piece is a pawn.
			case 0: 
				// white moves
				if(whiteTurn){
					// check if we're taking a piece 
					if((startPos.x == endPos.x-1 || startPos.x == endPos.x+1) && startPos.y+1 == endPos.y){
						if(endField.piece == null){
							// check en passant
							if(gameBoard[endPos.x, endPos.y-1].piece.type == 0){
								if(moveList.Count > 3){
									if(moveList[moveList.Count-1].endPosX == gameBoard[endPos.x, endPos.y-1].position.x
										&& moveList[moveList.Count-1].endPosY == gameBoard[endPos.x, endPos.y-1].position.y
										&& moveList[moveList.Count-1].startPosX == gameBoard[endPos.x, endPos.y+1].position.x	
										&& moveList[moveList.Count-1].startPosY == gameBoard[endPos.x, endPos.y+1].position.y){
									return true;
								}
								}
							}
							return false;
						}
						if(endField.piece.colorIsWhite){
							return false;
						}
						// Check if promoting pawn
						if(endField.position.y == 7){
							return PromotePawnTakePiece(whiteTurn, startField, endField);
						}
						return TryTakePiece(whiteTurn, startField, endField);
					}
					// check if on second rank (may move 2 if not obstructed)
					if(startPos.y == 1 && endPos.y == 3 && gameBoard[startPos.x, startPos.y+1].piece == null && 
															gameBoard[startPos.x, startPos.y+2].piece == null &&
															 startPos.x == endPos.x){
						return TryMovePiece(whiteTurn, startField, endField);
					}
					// check normal move
					if(endPos.y == startPos.y + 1 && endPos.x == startPos.x && gameBoard[startPos.x, startPos.y+1].piece == null){
						// Check if promoting pawn
						if(endField.position.y == 7){
							return PromotePawnNoTakePiece(whiteTurn, startField, endField);
						}
						return TryMovePiece(whiteTurn, startField, endField);
					}
				}
				// black moves
				else{
					// check if we're taking a piece 
					if((startPos.x == endPos.x-1 || startPos.x == endPos.x+1) && startPos.y-1 == endPos.y){
						if(endField.piece == null){
							// check en passant
							if(gameBoard[endPos.x, endPos.y+1].piece.type == 0){
								if(moveList.Count > 3){
									if(moveList[moveList.Count-1].endPosX == gameBoard[endPos.x, endPos.y+1].position.x
										&& moveList[moveList.Count-1].endPosY == gameBoard[endPos.x, endPos.y+1].position.y
										&& moveList[moveList.Count-1].startPosX == gameBoard[endPos.x, endPos.y-1].position.x
										&& moveList[moveList.Count-1].startPosY == gameBoard[endPos.x, endPos.y-1].position.y){
									return true;
								}
								}
							}
							return false;
						}
						if(!endField.piece.colorIsWhite){
							return false;
						}
						if(endField.position.y == 0){
							return PromotePawnTakePiece(whiteTurn, startField, endField);
						}
						return TryTakePiece(whiteTurn, startField, endField);
					}
					// check if on second rank (may move 2 if not obstructed)
					if(startPos.y == 6 && endPos.y == 4 && gameBoard[startPos.x, startPos.y-1].piece == null && 
															gameBoard[startPos.x, startPos.y-2].piece == null &&
															 startPos.x == endPos.x){
						return TryMovePiece(whiteTurn, startField, endField);
					}
					// check normal move
					if(endPos.y == startPos.y - 1 && endPos.x == startPos.x && gameBoard[startPos.x, startPos.y-1].piece == null){
						return TryMovePiece(whiteTurn, startField, endField);
					}
				}
				break;

			// Piece is a Knight
			case 1:
				// check if we're taking an enemy piece
				// check if target square is empty
				// check if king would be in check after move
				if(endField.piece != null){
					if(endField.piece.colorIsWhite == whiteTurn){
						break;
					}
					if(Math.Abs(endField.position.x - startField.position.x) == 2 && Math.Abs(endField.position.y - startField.position.y) == 1){
						return TryTakePiece(whiteTurn, startField, endField);
					}
					if(Math.Abs(endField.position.x - startField.position.x) == 1 && Math.Abs(endField.position.y - startField.position.y) == 2){
						return TryTakePiece(whiteTurn, startField, endField);
					}
				}
				if(Math.Abs(endField.position.x - startField.position.x) == 2 && Math.Abs(endField.position.y - startField.position.y) == 1){
					return TryMovePiece(whiteTurn, startField, endField);
				}
				if(Math.Abs(endField.position.x - startField.position.x) == 1 && Math.Abs(endField.position.y - startField.position.y) == 2){
					return TryMovePiece(whiteTurn, startField, endField);
				}
				break;

			// Piece is a Bishop
			case 2:
				// Check all 4 directions
				// for each step, check if square is occupied and by what piece
				if(startField.position.x > endField.position.x && startField.position.y > endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i > -1; i--){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j--;
						if(j < 0){
							break;
						}
					}
				}

				if(startField.position.x > endField.position.x && startField.position.y < endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i > -1; i--){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j++;
						if(j > 7){
							break;
						}
					}
				}

				if(startField.position.x < endField.position.x && startField.position.y > endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i < 8; i++){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j--;
						if(j < 0){
							break;
						}
					}
				}

				if(startField.position.x < endField.position.x && startField.position.y < endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i < 8; i++){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j++;
						if(j > 7){
							break;
						}
					}
				}

				break;

			// Piece is a Rook
			case 3: 
				// Check all directions
				if(endField.position.x == startField.position.x && endField.position.y > startField.position.y){
					for(int i = startField.position.y; i < 8; i++){
						if(gameBoard[startField.position.x, i] != startField){
							if(gameBoard[startField.position.x, i] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[startField.position.x, i].piece != null){
								return false;
							}
						}
					}
				}

				if(endField.position.x == startField.position.x && endField.position.y < startField.position.y){
					for(int i = startField.position.y; i > -1; i--){
						if(gameBoard[startField.position.x, i] != startField){
							if(gameBoard[startField.position.x, i] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[startField.position.x, i].piece != null){
								return false;
							}
						}
					}
				}
				
				if(endField.position.x > startField.position.x && endField.position.y == startField.position.y){
					for(int i = startField.position.x; i < 8; i++){
						if(gameBoard[i, startField.position.y] != startField){
							if(gameBoard[i, startField.position.y] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i, startField.position.y].piece != null){
								return false;
							}
						}
					}
				}

				if(endField.position.x < startField.position.x && endField.position.y == startField.position.y){
					for(int i = startField.position.x; i > -1; i--){
						if(gameBoard[i, startField.position.y] != startField){
							if(gameBoard[i, startField.position.y] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i, startField.position.y].piece != null){
								return false;
							}
						}
					}
				}

				break;

			// Piece is a Queen (Queen combines Bishop and Rook)
			case 4: 
				if(startField.position.x > endField.position.x && startField.position.y > endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i > -1; i--){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j--;
						if(j < 0){
							break;
						}
					}
				}

				if(startField.position.x > endField.position.x && startField.position.y < endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i > -1; i--){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j++;
						if(j > 7){
							break;
						}
					}
				}

				if(startField.position.x < endField.position.x && startField.position.y > endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i < 8; i++){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j--;
						if(j < 0){
							break;
						}
					}
				}

				if(startField.position.x < endField.position.x && startField.position.y < endField.position.y){
					int j = startField.position.y;
					for(int i = startField.position.x; i < 8; i++){
						if(gameBoard[i,j] != startField){
							if(gameBoard[i,j].piece != null){
								// If we're not at the target field and a piece is blocking our path, abort. 
								if(gameBoard[i,j] != endField){
									return false;
								}
								if(endField.piece.colorIsWhite == whiteTurn){
									return false;
								}
								return TryTakePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i,j] == endField){
								return TryMovePiece(whiteTurn, startField, endField);	
							}
						}
						j++;
						if(j > 7){
							break;
						}
					}
				}

				if(endField.position.x == startField.position.x && endField.position.y > startField.position.y){
					for(int i = startField.position.y; i < 8; i++){
						if(gameBoard[startField.position.x, i] != startField){
							if(gameBoard[startField.position.x, i] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[startField.position.x, i].piece != null){
								return false;
							}
						}
					}
				}

				if(endField.position.x == startField.position.x && endField.position.y < startField.position.y){
					for(int i = startField.position.y; i > -1; i--){
						if(gameBoard[startField.position.x, i] != startField){
							if(gameBoard[startField.position.x, i] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[startField.position.x, i].piece != null){
								return false;
							}
						}
					}
				}
				
				if(endField.position.x > startField.position.x && endField.position.y == startField.position.y){
					for(int i = startField.position.x; i < 8; i++){
						if(gameBoard[i, startField.position.y] != startField){
							if(gameBoard[i, startField.position.y] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i, startField.position.y].piece != null){
								return false;
							}
						}
					}
				}

				if(endField.position.x < startField.position.x && endField.position.y == startField.position.y){
					for(int i = startField.position.x; i > -1; i--){
						if(gameBoard[i, startField.position.y] != startField){
							if(gameBoard[i, startField.position.y] == endField){
								if(endField.piece != null){
									if(endField.piece.colorIsWhite == whiteTurn){
										return false;
									}
									return TryTakePiece(whiteTurn, startField, endField);
								}
								return TryMovePiece(whiteTurn, startField, endField);
							}
							if(gameBoard[i, startField.position.y].piece != null){
								return false;
							}
						}
					}
				}

				break;

			// Piece is a King
			case 5: 
				if(Math.Abs( startPos.x - endPos.x) <= 1 && Math.Abs( startPos.y - endPos.y) <= 1){
					if(endField.piece != null){
						if(endField.piece.colorIsWhite == whiteTurn){
							return false;
						}
						else{
							return TryTakePiece(whiteTurn, startField, endField);
						}
					}
					else{
						return TryMovePiece(whiteTurn, startField, endField);
					}
				}
				if(Math.Abs(endField.position.x - startField.position.x) == 2 
						&& endField.position.x > startField.position.x
						&& gameBoard[startPos.x+1, startPos.y].piece == null
						&& gameBoard[startPos.x+2, startPos.y].piece == null 
						&& gameBoard[startPos.x+3, startPos.y].piece != null){
					if(gameBoard[startPos.x+3, startPos.y].piece.type == 3){
						return CheckIfCastlingPossible(whiteTurn, gameBoard[startPos.x+3, startPos.y]);
					}
				}
				if(Math.Abs(endField.position.x - startField.position.x) == 2 
						&& endField.position.x < startField.position.x
						&& gameBoard[startPos.x-1, startPos.y].piece == null
						&& gameBoard[startPos.x-2, startPos.y].piece == null 
						&& gameBoard[startPos.x-3, startPos.y].piece == null
						&& gameBoard[startPos.x-4, startPos.y].piece != null){
					if(gameBoard[startPos.x-4, startPos.y].piece.type == 3){
						return CheckIfCastlingPossible(whiteTurn, gameBoard[startPos.x-4, startPos.y]);
					}
				}
				break;
		}

		return false;
	}

	private bool PromotePawnTakePiece(bool whiteTurn, Field startField, Field endField){
		Piece tempPiece;
		if(promoteTo == 1){
			tempPiece = endField.piece;
			endField.piece = new Knight(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece = tempPiece;
				return false;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = tempPiece;
		}
		if(promoteTo == 2){
			tempPiece = endField.piece;
			endField.piece = new Bishop(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece = tempPiece;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = tempPiece;
		}
		if(promoteTo == 3){
			tempPiece = endField.piece;
			endField.piece = new Rook(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece = tempPiece;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = tempPiece;
		}
		if(promoteTo == 4){
			tempPiece = endField.piece;
			endField.piece = new Queen(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece = tempPiece;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = tempPiece;
		}
		return true;
	}

		private bool PromotePawnNoTakePiece(bool whiteTurn, Field startField, Field endField){
		if(promoteTo == 1){
			endField.piece = new Knight(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece = null;
				return false;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = null;
		}
		if(promoteTo == 2){
			endField.piece = new Bishop(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece = null;
				return false;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = null;
		}
		if(promoteTo == 3){
			endField.piece = new Rook(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece =null;
				return false;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = null;
		}
		if(promoteTo == 4){
			endField.piece = new Queen(whiteTurn, endField.position);
			startField.piece = null;
			if(CheckIfKingInCheck(whiteTurn)){
				startField.piece = new Pawn(whiteTurn, startField.position);
				endField.piece = null;
				return false;
			}
			startField.piece = new Pawn(whiteTurn, startField.position);
			endField.piece = null;
		}
		return true;
	}

	private bool TryTakePiece(bool whiteTurn, Field startField, Field endField){
		Piece tempPiece = endField.piece;
		endField.piece = startField.piece; 
		startField.piece = null;
		// Check if king in check
		if(CheckIfKingInCheck(whiteTurn)){
			startField.piece = endField.piece;
			endField.piece = tempPiece;
			return false;
		}
		startField.piece = endField.piece;
		endField.piece = tempPiece; 
		return true;	
	}

	private bool TryMovePiece(bool whiteTurn, Field startField, Field endField){
		endField.piece = startField.piece; 
		startField.piece = null;
		if(CheckIfKingInCheck(whiteTurn)){
			startField.piece = endField.piece;
			endField.piece = null;
			return false;
		}
		startField.piece = endField.piece;
		endField.piece = null;
		return true;
	}

	private bool CheckIfCastlingPossible(bool colorIsWhite, Field rookField){
		foreach(Move m in moveList){
			if(m.whiteMove == colorIsWhite){
				if(m.type == 5){
					return false;
				}
				if(m.startPosX == rookField.position.x && m.startPosY == rookField.position.y){
					return false;
				}
			}
		}
		return true;
	}






	public bool CheckIfKingInCheck(bool colorIsWhite){
		Field kingLocation = FindKingPosition(colorIsWhite);
		Field enemyField;

		// Check pawns separately
		if(colorIsWhite && kingLocation.position.y < 7){
			if(kingLocation.position.x < 7){
				enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y+1];
				if(enemyField.piece != null){
					if(!enemyField.piece.colorIsWhite && enemyField.piece.type == 0){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
				
			}
			if(kingLocation.position.x > 0){
				enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y+1];
				if(enemyField.piece != null){
					if(!enemyField.piece.colorIsWhite && enemyField.piece.type == 0){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
				
			}
		}
		else if(kingLocation.position.y > 0){
			if(kingLocation.position.x < 7){
				enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y-1];
				if(enemyField.piece != null){
					if(enemyField.piece.colorIsWhite && enemyField.piece.type == 0){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
				
			}
			if(kingLocation.position.y > 0){
				enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y-1];
				if(enemyField.piece != null){
					if(enemyField.piece.colorIsWhite && enemyField.piece.type == 0){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
				
			}
		}

		// Check if kings are next to each other?
		if(kingLocation.position.y < 7){
			enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y+1];
			if(enemyField.piece != null){
				if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
					checkingPiece = enemyField.piece;
					return true;
				}
			}
			if(kingLocation.position.x < 7){
				enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y+1];
				if(enemyField.piece != null){
					if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
			}
			if(kingLocation.position.x > 0){
				enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y+1];
				if(enemyField.piece != null){
					if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
			}
		}	

		if(kingLocation.position.y > 0){
			enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y-1];
			if(enemyField.piece != null){
				if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
					checkingPiece = enemyField.piece;
					return true;
				}
			}
			if(kingLocation.position.x < 7){
				enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y-1];
				if(enemyField.piece != null){
					if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
			}
			if(kingLocation.position.x > 0){
				enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y-1];
				if(enemyField.piece != null){
					if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
						checkingPiece = enemyField.piece;
						return true;
					}
				}
			}
		}	
		if(kingLocation.position.x > 0){
			enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y];
			if(enemyField.piece != null){
				if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
					checkingPiece = enemyField.piece;
					return true;
				}
			}
		}
		if(kingLocation.position.x < 7){
			enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y];
			if(enemyField.piece != null){
				if(enemyField.piece.colorIsWhite != colorIsWhite && enemyField.piece.type == 5){
					checkingPiece = enemyField.piece;
					return true;
				}
			}
		}	
			

		// check knight spaces and all around the king what the first piece is (if any). 
		// if the piece is an enemy piece that checks the king, return true, otherwise false
		if(kingLocation.position.x < 6 && kingLocation.position.y < 7){
			enemyField = gameBoard[kingLocation.position.x+2, kingLocation.position.y+1];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}
		if(kingLocation.position.x < 7 && kingLocation.position.y < 6){
			enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y+2];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}		
		if(kingLocation.position.x > 0 && kingLocation.position.y < 6){
			enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y+2];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}
		if(kingLocation.position.x > 1 && kingLocation.position.y < 7){
			enemyField = gameBoard[kingLocation.position.x-2, kingLocation.position.y+1];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}
		if(kingLocation.position.x > 1 && kingLocation.position.y > 0){
			enemyField = gameBoard[kingLocation.position.x-2, kingLocation.position.y-1];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}
		if(kingLocation.position.x > 0 && kingLocation.position.y > 1){
			enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y-2];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}
		if(kingLocation.position.x < 7 && kingLocation.position.y > 1){
			enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y-2];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}
		if(kingLocation.position.x  < 6 && kingLocation.position.y > 0){
			enemyField = gameBoard[kingLocation.position.x+2, kingLocation.position.y-1];
			if(CheckIfKnightChecksKing(colorIsWhite, kingLocation, enemyField)){
				checkingPiece = enemyField.piece;
				return true;
			}
		}

		// Check y+
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		for(int i = kingLocation.position.y; i < 8; i++){
			enemyField = gameBoard[enemyField.position.x, i];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 3 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
		}		
		
		// Check x+
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		for(int i = kingLocation.position.x; i < 8; i++){
			enemyField = gameBoard[i, enemyField.position.y];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 3 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
		}	

		// Check x-
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		for(int i = kingLocation.position.x; i > -1; i--){
			enemyField = gameBoard[i, enemyField.position.y];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 3 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
		}	

		// Check y-
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		for(int i = kingLocation.position.y; i > -1; i--){
			enemyField = gameBoard[enemyField.position.x, i];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 3 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
		}	

		// Check x+ y+
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		int j = kingLocation.position.y;
		for(int i = kingLocation.position.x; i < 8; i++){
			enemyField = gameBoard[i, j];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 2 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
			j++;
			if(j > 7){
				break;
			}

		}

		// Check x+ y-
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		j = kingLocation.position.y;
		for(int i = kingLocation.position.x; i < 8; i++){
			enemyField = gameBoard[i, j];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 2 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
			j--;
			if(j < 0){
				break;
			}

		}

		// Check x- y+
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		j = kingLocation.position.y;
		for(int i = kingLocation.position.x; i > -1; i--){
			enemyField = gameBoard[i, j];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 2 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
			j++;
			if(j > 7){
				break;
			}
		}

		// Check x- y-
		enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y];
		j = kingLocation.position.y;
		for(int i = kingLocation.position.x; i > -1; i--){
			enemyField = gameBoard[i, j];
			if(enemyField.piece != null && enemyField != kingLocation){
				if(enemyField.piece.colorIsWhite != colorIsWhite){
					if(enemyField.piece.type == 2 || enemyField.piece.type == 4){
						checkingPiece = enemyField.piece;
						return true;
					}
					else{
						break;
					}
				}
				else{
					break;
				}
			}
			j--;
			if(j < 0){
				break;
			}
		}

		return false;
	}

	private bool CheckIfKnightChecksKing(bool colorIsWhite, Field kingLocation, Field enemyField){
		if(enemyField != null){
			if(enemyField.piece != null){
				if(enemyField.piece.type == 1 && enemyField.piece.colorIsWhite != colorIsWhite){
					checkingPiece = enemyField.piece;
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckIfCheckMate(bool colorIsWhite){
		Field kingLocation = FindKingPosition(colorIsWhite);
		Field enemyField;
		if(!CheckIfKingInCheck(colorIsWhite)){
			return false;
		}
		// move king to each adjacent square
		if(kingLocation.position.y < 7){
			enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y+1];
			if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
				return false;
			}
			if(kingLocation.position.x < 7){
				enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y+1];
				if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
					return false;
				}
			}
			if(kingLocation.position.x > 0){
				enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y+1];
				if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
					return false;
				}
			}
		}	

		if(kingLocation.position.y > 0){
			enemyField = gameBoard[kingLocation.position.x, kingLocation.position.y-1];
			if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
				return false;
			}
			if(kingLocation.position.x < 7){
				enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y-1];
				if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
					return false;
				}
			}
			if(kingLocation.position.x > 0){
				enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y-1];
				if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
					return false;
				}
			}
		}	
		if(kingLocation.position.x > 0){
			enemyField = gameBoard[kingLocation.position.x-1, kingLocation.position.y];
			if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
				return false;
			}
		}
		if(kingLocation.position.x < 7){
			enemyField = gameBoard[kingLocation.position.x+1, kingLocation.position.y];
			if(!CheckMateHelper(colorIsWhite, enemyField, kingLocation)){
				return false;
			}
		}	
		// Get a list of all our non-king pieces
		List<Piece> pieceList = GetNonKingPieces(colorIsWhite);

		// Check if we can take the offending piece (and after that check if we're not still in check)
		foreach(Piece p in pieceList){
			if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
				return false;
			}
		}
		
		// Check if we can put a piece in between the attacker and king
		if(checkingPiece.type == 2 || checkingPiece.type == 4){
			// Get all squares inbetween the king and the checking piece and for all our pieces try to move them to that square
			if(checkingPiece.posOnBoard.x > kingLocation.position.x && checkingPiece.posOnBoard.y > kingLocation.position.y){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.x; i > kingLocation.position.x; i--){
					enemyField = gameBoard[enemyField.position.x-1, enemyField.position.y-1];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}
			if(checkingPiece.posOnBoard.x > kingLocation.position.x && checkingPiece.posOnBoard.y < kingLocation.position.y){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.x; i > kingLocation.position.x; i--){
					enemyField = gameBoard[enemyField.position.x-1, enemyField.position.y+1];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}

			if(checkingPiece.posOnBoard.x < kingLocation.position.x && checkingPiece.posOnBoard.y > kingLocation.position.y){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.x; i < kingLocation.position.x; i++){
					enemyField = gameBoard[enemyField.position.x+1, enemyField.position.y-1];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}
			if(checkingPiece.posOnBoard.x < kingLocation.position.x && checkingPiece.posOnBoard.y < kingLocation.position.y){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.x; i > kingLocation.position.x; i++){
					enemyField = gameBoard[enemyField.position.x+1, enemyField.position.y+1];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}
		}
		if(checkingPiece.type == 3 || checkingPiece.type == 4){
			if(checkingPiece.posOnBoard.x > kingLocation.position.x){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.x; i > kingLocation.position.x; i--){
					enemyField = gameBoard[enemyField.position.x-1, enemyField.position.y];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}
			if(checkingPiece.posOnBoard.x < kingLocation.position.x){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.x; i < kingLocation.position.x; i++){
					enemyField = gameBoard[enemyField.position.x+1, enemyField.position.y];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}
			if(checkingPiece.posOnBoard.y > kingLocation.position.y){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.y; i > kingLocation.position.y; i--){
					enemyField = gameBoard[enemyField.position.x, enemyField.position.y-1];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}
			if(checkingPiece.posOnBoard.y < kingLocation.position.y){
				enemyField = gameBoard[checkingPiece.posOnBoard.x, checkingPiece.posOnBoard.y];
				for(int i = checkingPiece.posOnBoard.y; i < kingLocation.position.y; i++){
					enemyField = gameBoard[enemyField.position.x, enemyField.position.y+1];
					foreach(Piece p in pieceList){
						if(CheckLegalMove(colorIsWhite, p.posOnBoard, checkingPiece.posOnBoard)){
							return false;
						}
					}
				}
			}
		}

		return true;
	}

	private bool CheckMateHelper(bool colorIsWhite, Field enemyField, Field kingLocation){
		if(enemyField.piece == null){
			return !TryMovePiece(colorIsWhite, kingLocation, enemyField);
		}
		else if(enemyField.piece.colorIsWhite == !colorIsWhite) {
			return !TryTakePiece(colorIsWhite, kingLocation, enemyField);
		}
		return true;
	}

	private Field FindKingPosition(bool colorIsWhite){
		Field kingLocation = gameBoard[0,0];
		foreach(Field f in gameBoard){
			if(f.piece != null){
				if(f.piece.type == 5 && f.piece.colorIsWhite == colorIsWhite){
					return f;
				}
			}
		}
		// Could throw Exception because no king on board?
		return null;
	}

	private List<Piece> GetNonKingPieces(bool colorIsWhite){
		List<Piece> pieceList = new List<Piece>();

		foreach(Field f in gameBoard){
			if(f.piece != null){
				if(f.piece.colorIsWhite == colorIsWhite && f.piece.type != 5){
					pieceList.Add(f.piece);
				}
			}
		}

		return pieceList;
	}

	private void ExecutePawnPromote(bool whiteMove, bool takePiece, Field startField, Field endField){
		if(promoteTo == 1){
			endField.piece = new Knight(true, endField.position);
			startField.piece = null;
			if(whiteMove){
				whitePieces[0]--;
				whitePieces[1]++;
			}
			else{
				blackPieces[0]--;
				blackPieces[1]++;
			}
		}
		if(promoteTo == 2){
			endField.piece = new Bishop(true, endField.position);
			startField.piece = null;
			if(whiteMove){
				whitePieces[0]--;
				whitePieces[2]++;
			}
			else{
				blackPieces[0]--;
				blackPieces[2]++;
			}
		}
		if(promoteTo == 3){
			endField.piece = new Rook(true, endField.position);
			startField.piece = null;
			if(whiteMove){
				whitePieces[0]--;
				whitePieces[3]++;
			}
			else{
				blackPieces[0]--;
				blackPieces[3]++;
			}
		}
		if(promoteTo == 4){
			endField.piece = new Queen(true, endField.position);
			startField.piece = null;
			if(whiteMove){
				whitePieces[0]--;
				whitePieces[4]++;
			}
			else{
				blackPieces[0]--;
				blackPieces[4]++;
			}
		}
		bool checkCheck = CheckIfKingInCheck(whiteMove);
		moveList.Add(new Move(moveNum, whiteMove, 0, startField.position.x, startField.position.y, endField.position.x, endField.position.y, takePiece, promoteTo, checkCheck));
	}

	private void ExecuteMove(bool whiteMove, bool takePiece, Field startField, Field endField){
		if(takePiece){
			if(whiteMove){
				blackPieces[endField.piece.type]--;
			}
			else{
				whitePieces[endField.piece.type]--;
			}
		}
		endField.piece = startField.piece;
		endField.piece.posOnBoard = endField.position;
		startField.piece = null;
		bool checkCheck = CheckIfKingInCheck(whiteMove);
		moveList.Add(new Move(moveNum, whiteMove, endField.piece.type, startField.position.x, startField.position.y, endField.position.x, endField.position.y, takePiece, checkCheck));
	}

	public bool CheckDraw(bool whiteMove){
		// possibly allow AIs to offer a draw? Probably not. 
		// Stalemate: active player has no legal moves
		if(!CheckIfLegalMovesAvailable(whiteMove)){
			Debug.Log("Game is a draw: no legal moves available");
			return true;
		}
		// Thr(positionold repetition
		if(moveList.Count > 5){
			if(CheckIfMovesAreTheSame(moveList[moveList.Count-1], moveList[moveList.Count-3], moveList[moveList.Count-5]) 
				&& CheckIfMovesAreTheSame(moveList[moveList.Count-2], moveList[moveList.Count-4], moveList[moveList.Count-6])){
					Debug.Log("Game is a draw: repeated same position three times");
					return true;
			}
		}
		// 50-move rule (no piece taken or pawn moves for 50 moves by each player)
		if(moveList.Count > 100){
			if(FiftyMoveCheck()){
				Debug.Log("Game is a draw: no progress last 50 moves");
				return true;
			}
		}
		// Insufficient material: king with 1 bishop or 1 knight
		if(whitePieces[0] == 0 && whitePieces[3] == 0 && whitePieces[4] == 0
			&& blackPieces[0] == 0 && blackPieces[3] == 0 && blackPieces[4] == 0){
				if(whitePieces[1] + whitePieces[2] <= 1 && blackPieces[1] + blackPieces[2] <= 1){
					Debug.Log("Game is a draw: no player has enough material left to win");
					return true;
				}
			}

		return false;
	}

	private bool CheckIfMovesAreTheSame(Move m1, Move m2, Move m3){
		if(m1.type == m2.type && m1.type == m3.type){
			if(m1.startPosX == m2.startPosX  && m2.startPosX == m3.startPosX){
				if(m1.startPosY == m2.startPosY && m2.startPosY == m3.startPosY){
					if(m1.endPosX == m2.endPosX && m2.endPosX == m3.endPosX){
						if(m1.endPosY == m2.endPosY && m2.endPosY == m3.endPosY){
							if(m1.takePiece == m2.takePiece && m2.takePiece == m3.takePiece){
								return true;
							}
						}
					}
				}
			}
		}

		return false;
	}

	private bool FiftyMoveCheck(){
		for(int i = moveList.Count-100; i < moveList.Count; i++){
			if(moveList[i].takePiece || moveList[i].type == 0){
				return false;
			}
		}
		return true;
	}

	
	private bool CheckIfLegalMovesAvailable(bool whiteMove){
		// for each field on the board, check if there is a piece of the current player's color on that field. 
		// Then, suggest possible moves for that piece, based on its type
		for(int x = 0; x < 8; x++){
			for(int y = 0; y < 8; y++){
				Vector2Int startPos = new Vector2Int(x,y);
				if(gameBoard[x,y].piece != null){
					if(gameBoard[x,y].piece.colorIsWhite == whiteMove){
						int pieceType = gameBoard[x,y].piece.type;
						switch(pieceType){
							// Check pawn moves
							case 0:
								List<Vector2Int> possibleMoves = new List<Vector2Int>();
								possibleMoves.Add(new Vector2Int(x, y - 2));
								possibleMoves.Add(new Vector2Int(x, y + 2));
								possibleMoves.Add(new Vector2Int(x, y - 1));
								possibleMoves.Add(new Vector2Int(x, y + 1));
								possibleMoves.Add(new Vector2Int(x + 1, y + 1));
								possibleMoves.Add(new Vector2Int(x + 1, y - 1));
								possibleMoves.Add(new Vector2Int(x - 1, y - 1));
								possibleMoves.Add(new Vector2Int(x - 1, y + 1));

								foreach(Vector2Int position in possibleMoves){
									if(CheckIfPieceOnBoard(position)){
										if(CheckLegalMove(whiteMove, startPos, position)){
											return true;
										}
									}
								}
								break;
								// }



								// if(x == 1 && whiteMove){
								// 	// white pawn in starting position
								// 	if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, y+2))){
								// 		return true;
								// 	}
								// }
								// else if(x == 6 && !whiteMove){
								// 	if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, y-2))){
								// 		return true;
								// 	}
								// }
								// else if(x > 0){
								// 	if(whiteMove){
								// 		if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, y+1))){
								// 			return true;
								// 		}
								// 		if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x+1,y+1))){
								// 			return true;
								// 		}
								// 	}
								// 	else{
								// 		if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, y-1))){
								// 			return true;
								// 		}
								// 		if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x+1,y-1))){
								// 			return true;
								// 		}
								// 	}
								// }
								// else if(x < 7){
								// 	if(whiteMove){
								// 		if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x-1, y+1))){
								// 			return true;
								// 		}
								// 	}
								// 	else{
								// 		if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x-1, y-1))){
								// 			return true;
								// 		}
								// 	}
								// }
								break;
							// Check knight moves
							case 1:
								List<Vector2Int> endFields = new List<Vector2Int>();
								endFields.Add(new Vector2Int(x-2, y-1));
								endFields.Add(new Vector2Int(x+2, y-1));
								endFields.Add(new Vector2Int(x-2, y+1));
								endFields.Add(new Vector2Int(x+2, y+1));
								endFields.Add(new Vector2Int(x-1, y-2));
								endFields.Add(new Vector2Int(x+1, y-2));
								endFields.Add(new Vector2Int(x-1, y+2));
								endFields.Add(new Vector2Int(x+1, y+2));
								
								foreach(Vector2Int position in endFields){
									if(CheckIfPieceOnBoard(position)){
										if(CheckLegalMove(whiteMove, startPos, position)){
											return true;
										}
									}
								}
								break;
							// Check bishop moves
							case 2: 
								for(int i = x + 1; i < 8; i++){
									for(int j = y + 1; j < 8; j++){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								for(int i = x + 1; i < 8; i++){
									for(int j = y - 1; j > -1; j--){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								for(int i = x - 1; i > -1; i--){
									for(int j = y + 1; j < 8; j++){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								for(int i = x - 1; i > -1; i--){
									for(int j = y - 1; j > -1; j--){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								break;
							// Check rook moves
							case 3: 
								for(int i = x + 1; i < 8; i++){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i, y))){
										return true;
									}
								}
								for(int i = x - 1; i > -1; i--){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i, y))){
										return true;
									}
								}
								for(int i = y + 1; i < 8; i++){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, i))){
										return true;
									}
								}
								for(int i = y - 1; i > -1; i--){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, i))){
										return true;
									}
								}
								break;
							// Check queen moves
							case 4: 
								for(int i = x + 1; i < 8; i++){
									for(int j = y + 1; j < 8; j++){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								for(int i = x + 1; i < 8; i++){
									for(int j = y - 1; j > -1; j--){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								for(int i = x - 1; i > -1; i--){
									for(int j = y + 1; j < 8; j++){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								for(int i = x - 1; i > -1; i--){
									for(int j = y - 1; j > -1; j--){
										if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
											return true;
										}
									}
								}
								for(int i = x + 1; i < 8; i++){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i, y))){
										return true;
									}
								}
								for(int i = x - 1; i > -1; i--){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i, y))){
										return true;
									}
								}
								for(int i = y + 1; i < 8; i++){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, i))){
										return true;
									}
								}
								for(int i = y - 1; i > -1; i--){
									if(CheckLegalMove(whiteMove, startPos, new Vector2Int(x, i))){
										return true;
									}
								}
								break;
							// Check king moves
							case 5: 
								for(int i = x - 1 ; i < x + 2; i++){
									for(int j = y - 1; j < y + 2; j++){
										if(CheckIfPieceOnBoard(new Vector2Int(i,j))){
											if(CheckLegalMove(whiteMove, startPos, new Vector2Int(i,j))){
												return true;
											}
										}
									}
								}
								break;
						}
					}
				}
			}
		}
		return false;
	}

	private bool CheckIfPieceOnBoard(Vector2Int position){
		if(position.x >= 0 &&(position.x <= 7 &&(position.y >= 0 &&(position.y <= 7)))){
			return true;
		}
		return false;
	}
}
