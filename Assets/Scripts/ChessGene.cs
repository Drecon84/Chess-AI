using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ChessGene
{
    [XmlAttribute("Neurontype")]
    public int neuronType;

    [XmlArray("PossibleIn")]
    [XmlArrayItem("Connection")]
    public List<int> possibleConnectionsIn;

    [XmlArray("PossibleOut")]
    [XmlArrayItem("Connection")]
    public List<int> possibleConnectionsOut;

    [XmlElement("Threshold")]
    public int threshold;

    [XmlElement("OutputStrength")]
    public int outputStrength;

    // TODO: figure out what a reasonable time is to wait?
    [XmlElement("WaitForConnection")]
    public int waitForConnection;

    public ChessGene(){
        // Probably make some random settings? 
    }

    public ChessGene(int type, List<int> input, List<int> output, int wait, int thresh, int outStr){
        neuronType = type;
        possibleConnectionsIn = input;
        possibleConnectionsOut = output;
        threshold = thresh;
        waitForConnection = wait;
        outputStrength = outStr;
    }

    public List<int> CopyIns(){
        List<int> toReturn = new List<int>();
        foreach(int i in possibleConnectionsIn){
            toReturn.Add(i);
        }
        return toReturn;
    }

    public List<int> CopyOuts(){
        List<int> toReturn = new List<int>();
        foreach(int i in possibleConnectionsOut){
            toReturn.Add(i);
        }
        return toReturn;
    }

}
