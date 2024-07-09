using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ChessGame
{
    [XmlArray("MoveList")]
    [XmlArrayItem("Move")]
    public List<Move> moveList;

    // This variable tells us if the AI that played this game was playing white or black
    [XmlElement("WhiteMove")]
    bool whiteMove;

    public ChessGame(){
        
    }

    public ChessGame(List<Move> moves, bool whiteM){
        moveList = moves;
        whiteMove = whiteM;
    }
}
