using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ChessAIDNA 
{
    [XmlAttribute("Name")]
    public string aiName;

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
        MakeName();
    }

    public void MakeName(){
        string alphabet = "abcdefghijklmnopqrstuvwxyz";
        char[] name = new char[10];

        for(int i = 0; i < 10; i++){
            name[i] = alphabet[Random.Range(0, 26)];
        }
        aiName = new string(name);
    }


}
