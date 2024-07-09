using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ChessNeuron
{
    // type == 0 for input neurons, 1 for intermediate, 2 for pre-output and 3 for output
    [XmlAttribute("NeuronType")]
    public int neuronType;

    [XmlArray("ConnectionsIn")]
    [XmlArrayItem("Neuron")]
    public List<ChessNeuron> connectionsIn;

    [XmlArray("PossibleIn")]
    [XmlArrayItem("Connection")]
    public List<int> possibleConnectionsIn;

    [XmlArray("ConnectionsOut")]
    [XmlArrayItem("Neuron")]
    public List<ChessNeuron> connectionsOut;

    [XmlArray("PossibleOut")]
    [XmlArrayItem("Connection")]
    public List<int> possibleConnectionsOut;

    [XmlElement("WaitForConnection")]
    public int waitForConnection;

    [XmlElement("Threshold")]
    public int threshold;
    [XmlIgnore]
    public int currentActivation;

    [XmlIgnore]
    public bool isActivated;

    [XmlElement("OutputStrength")]
    public int outputStrength;

    [XmlArray("GeneCode")]
    [XmlArrayItem("ChessGene")]
    public List<ChessGene> geneCode;

    [XmlElement("CurrentGene")]
    public ChessGene currentGene;

    public ChessNeuron(){
        
    }

    // This one is only used for output Neurons
    public ChessNeuron(int type){
        neuronType = type;
    }

    public ChessNeuron(List<ChessGene> DNA){
        // Make this neuron based on the first gene, then remove it from the list
        geneCode = new List<ChessGene>();
        foreach(ChessGene gene in DNA){
            if(gene.neuronType == 0){
                geneCode.Add(new ChessGene(0, null, gene.CopyOuts(), 0, 0, 100));
            }
            else{
                geneCode.Add(new ChessGene(gene.neuronType, gene.CopyIns(), gene.CopyOuts(), 
                    gene.waitForConnection, gene.threshold, gene.outputStrength));
            }
        }
        currentGene = geneCode[0];
        geneCode.RemoveAt(0);
        neuronType = currentGene.neuronType;
        if(neuronType == 1){
            MakeIntermediateNeuron();
        }
    }

    public List<ChessGene> FetchGenes(){
        List<ChessGene> newList = new List<ChessGene>();

        foreach(ChessGene gene in geneCode){
            newList.Add(new ChessGene(gene.neuronType, gene.CopyIns(), gene.CopyOuts(), 
                    gene.waitForConnection, gene.threshold, gene.outputStrength));
        }

        return newList;
    }

    public int GetNeuronType(){
        return neuronType;
    }

    public void CheckNeuronActivation(){
        // Do we need an exception for output neurons?
        if(currentActivation >= threshold){
            isActivated = true;
        }
        else{
            isActivated = false;
        }
        
    }

    public void FireNeuron(){
        if(isActivated){
            foreach(ChessNeuron neuron in connectionsOut){
                neuron.currentActivation += outputStrength;
            }
        }
        outputStrength = 0;
    }

    public int ReadOutputNeuron(){
        if(neuronType == 3){
            return currentActivation;
        }
        return 0;
    }
    
    // Make new InputNeuron
    public void MakeInputNeuron(){
        possibleConnectionsOut = currentGene.possibleConnectionsOut;
        connectionsOut = new List<ChessNeuron>();
        foreach(int i in possibleConnectionsOut){
            connectionsOut.Add(null);
        }
        waitForConnection = currentGene.waitForConnection;
        outputStrength = currentGene.outputStrength;
    }

    public void MakeIntermediateNeuron(){
        possibleConnectionsIn = currentGene.possibleConnectionsIn;
        connectionsIn = new List<ChessNeuron>();
        foreach(int i in possibleConnectionsIn){
            connectionsIn.Add(null);
        }
        possibleConnectionsOut = currentGene.possibleConnectionsOut;
        connectionsOut = new List<ChessNeuron>();
        foreach(int i in possibleConnectionsOut){
            connectionsOut.Add(null);
        }
        waitForConnection = currentGene.waitForConnection;
        threshold = currentGene.threshold;
        outputStrength = currentGene.outputStrength;
    }

    // Make new OutputNeuron
    public void MakePreOutputNeuron(List<ChessNeuron> outList){
        possibleConnectionsIn = currentGene.possibleConnectionsIn;
        connectionsIn = new List<ChessNeuron>();
        foreach(int i in possibleConnectionsIn){
            connectionsIn.Add(null);
        }
        possibleConnectionsOut = currentGene.possibleConnectionsOut;
        connectionsOut = new List<ChessNeuron>();
        for(int i = 0; i < possibleConnectionsOut.Count; i++){
            connectionsOut.Add(outList[possibleConnectionsOut[i]]);
        }
        threshold = currentGene.threshold;
        outputStrength = currentGene.outputStrength;
    }

    public void MakeOutputNeuron(){
        // Possible functionality???
    }

    public bool CheckIfAvailable(){
        if(waitForConnection > 0){
            return false;
        }
        waitForConnection--;

        return true;
    }

    public List<int> CheckPossibleOut(){
        List<int> toReturn = new List<int>();

        for(int i = 0; i < connectionsOut.Count; i++){
            if(connectionsOut[i] == null){
                toReturn.Add(possibleConnectionsOut[i]);
            }
        }

        return toReturn;
    }

    public void ConnectToNeuron(ChessNeuron chessNeuron, int transmitter){
        for(int i = 0; i < possibleConnectionsOut.Count; i++){
            if(possibleConnectionsOut[i] == transmitter && connectionsOut[i] == null){
                connectionsOut[i] = chessNeuron;
                break;
            }
        }
        chessNeuron.ConnectBack(this, transmitter);
    }

    public void ConnectBack(ChessNeuron chessNeuron, int transmitter){
        for(int i = 0; i < possibleConnectionsIn.Count; i++){
            //Debug.Log(possibleConnectionsIn);
            if(possibleConnectionsIn[i] == transmitter && connectionsIn[i] == null){
                connectionsIn[i] = chessNeuron;
                break;
            }
        }
    }

    public void ForceConnect(ChessNeuron chessNeuron){
        connectionsOut[0] = chessNeuron;
        chessNeuron.connectionsIn[0] = this;
    }

    public bool CheckConnection(int transmitter){
        for(int i = 0; i < possibleConnectionsIn.Count; i++){
            if(possibleConnectionsIn[i] == transmitter && connectionsIn[i] == null){
                return true;
            }
        }
        return false;
    }

    public bool CheckIfInConnectionsFull(){
        for(int i = 0; i < connectionsIn.Count; i++){
            if(connectionsIn[i] == null){
                return false;
            }
        }
        return true;
    }

    public ChessNeuron MakeNewNeuron(int transmitter){
        for(int i = 0; i < connectionsOut.Count; i++){
            if(possibleConnectionsOut[i] == transmitter && connectionsOut[i] == null){
                connectionsOut[i] = new ChessNeuron(FetchGenes());

                return connectionsOut[i];
            }
        }
        return null;
    }

}
