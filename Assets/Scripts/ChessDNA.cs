using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ChessDNA
{
    [XmlArray("ChessGenes")]
    [XmlArrayItem("ChessGene")]
    public List<ChessGene> chessGenes;

    public ChessDNA(){
        
    }

    public ChessDNA(List<ChessGene> chessGene){
        chessGenes = chessGene;
    }
}
