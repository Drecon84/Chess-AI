using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class Move
{
    [XmlElement("MoveNumber")]
    public int moveNumber;

    [XmlElement("WhiteMove")]
    public bool whiteMove;

    [XmlElement("Type")]
    public int type;

    [XmlElement("StartPosX")]
    public int startPosX;

    [XmlElement("StartPosY")]
    public int startPosY;

    [XmlElement("EndPosx")]
    public int endPosX;

    [XmlElement("EndPosY")]
    public int endPosY;

    [XmlElement("TakePiece")]
    public bool takePiece;

    [XmlElement("PromotePawnTo")]
    public int promotePawnTo;

    [XmlElement("Check")]
    public bool check;

    [XmlElement("CheckMate")]
    public bool checkMate;

    [XmlElement("EnPassant")]
    public bool enPassant;

    [XmlElement("Castle")]
    public bool castle;

    [XmlElement("Draw")]
    public bool draw;

    public Move(){
        
    }

    public Move(int num, bool wm, bool cm){
        moveNumber = num;
        whiteMove = wm;
        checkMate = cm;
    }

    public Move(int num, bool wm, int pieceType, bool castling, int endX, int endY){
        moveNumber = num;
        whiteMove = wm;
        type = pieceType;
        castle = castling;
        endPosX = endX;
        endPosY = endY;
    }

    public Move(int num, bool wm, int pieceType, int startX, int startY, int endX, int endY, bool tp, bool checkCheck){
        moveNumber = num;
        whiteMove = wm;
        type = pieceType; 
        startPosX = startX;
        startPosY = startY;
        endPosX = endX;
        endPosY = endY;
        takePiece = tp;
        check = checkCheck;
    }

    public Move(int num, bool wm, int pieceType, int startX, int startY, int endX, int endY, bool tp, int promote, bool checkCheck){
        moveNumber = num;
        whiteMove = wm;
        type = pieceType; 
        startPosX = startX;
        startPosY = startY;
        endPosX = endX;
        endPosY = endY;
        takePiece = tp;
        promotePawnTo = promote;
        check = checkCheck;
    }

    public Move(int num, bool wm, int pieceType, int startX, int startY, int endX, int endY, bool ep, bool tp, bool checkCheck){
        moveNumber = num;
        whiteMove = wm;
        type = pieceType; 
        startPosX = startX;
        startPosY = startY;
        endPosX = endX;
        endPosY = endY;
        enPassant = ep;
        takePiece = tp;
        check = checkCheck;
    }
}
