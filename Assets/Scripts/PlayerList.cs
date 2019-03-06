using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class PlayerList
{
    [XmlArray("Playerlist")]
    [XmlArrayItem("Player")]
    public List<ChessAIDNA> playerList;

    public PlayerList(){
        playerList = new List<ChessAIDNA>();
    }
}
