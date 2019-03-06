using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ChessAIDNA 
{
    [XmlElement("HighestTransmitterNumber")]
    public int highestTransmitterNumber;

    [XmlArray("DNA")]
    [XmlArrayItem("ChessDNA")]
    public List<ChessDNA> DNA;

    public ChessAIDNA(){
        
    }

    public ChessAIDNA(List<ChessDNA> cdna, int transmitters){
        highestTransmitterNumber = transmitters;
        DNA = cdna;
    }


}
