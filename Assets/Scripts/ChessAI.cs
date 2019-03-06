using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ChessAI 
{
    [XmlArray("IntInputs")]
    public List<int> intInputs;

    [XmlArray("InputNeurons")]
    [XmlArrayItem("Neuron")]
    public List<ChessNeuron> inputNeurons;

    [XmlArray("OutputNeurons")]
    [XmlArrayItem("Neuron")]
    public List<ChessNeuron> outputNeurons;

    [XmlArray("AllNeurons")]
    [XmlArrayItem("Neuron")]
    public List<ChessNeuron> allNeurons;

    [XmlElement("DNA")]
    public ChessAIDNA aIDNA;

    [XmlArray("GameList")]
    [XmlArrayItem("Game")]
    public List<ChessGame> gameList;

    public ChessAI(){
        
    }
    
    public ChessAI(ChessAIDNA DNA){
        aIDNA = DNA;
        gameList = new List<ChessGame>();
    }

    public void GetInputs(List<int> numInputs){
        intInputs = numInputs;
        for(int i = 0; i < numInputs.Count; i++){
            inputNeurons[i].currentActivation += numInputs[i];
        }
    }

    public void ActivateAllNeurons(){
        foreach(ChessNeuron neuron in allNeurons){
            neuron.CheckNeuronActivation();
            
        }
        foreach(ChessNeuron neuron in allNeurons){
            neuron.FireNeuron();
        }
    }

    public List<int> GetOutputs(){
        List<int> intOutputs = new List<int>();

        foreach(ChessNeuron neuron in outputNeurons){
            intOutputs.Add(neuron.ReadOutputNeuron());
        }

        return intOutputs;
    }

}
