using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessNetworkBuilder
{

    public int inputNumber;

    public int outputNumber;

    public int GlobalStopChance;

    public int maxInput, maxOutput, maxWait;

    public int highestTransmitterNumber = 0;

    public int numToCull = 40;

    public int maxMutations = 20;


    // Building from blank slate first. Cross-over and mutation will follow soon
    // Currently choosing: max 4 input, max 4 output
    // Waittime max = 10
    // Will have to fiddle with these setting in the future. 

    // Cross-over can happen if we take a random input DNA strand from each parent
    // Mutation we'll have to figure out later

    public ChessNetworkBuilder(int inNum, int outNum, int maxIn, int maxOut, int maxWaitTime, int stop){
        inputNumber = inNum;
        outputNumber = outNum;
        maxInput = maxIn;
        maxOutput = maxOut;
        maxWait = maxWaitTime;
        GlobalStopChance = stop;
    }

    public ChessAIDNA BuildGene(){
        List<ChessDNA> DNAStrands = new List<ChessDNA>();
        for(int i = 0; i < inputNumber; i++){
            DNAStrands.Add(MakeChessGenes());
        }
        return new ChessAIDNA(DNAStrands, highestTransmitterNumber);
    }

    public ChessDNA MakeChessGenes(){
        List<ChessGene> DNA = new List<ChessGene>();
        highestTransmitterNumber = 0;
        
        // Q: What is the length we're going for at first? 1 in 4 chance?
        // First make an input gene, then make out genes until we randomly make a pre-output gene.
        DNA.Add(MakeInputGene());
        int stopChance = Random.Range(0, GlobalStopChance);
        while(stopChance != 0){
            DNA.Add(MakeGene(1));
            stopChance--;
        }
        DNA.Add(MakeGene(2));

        return new ChessDNA(DNA);
    }

    // Input neurons don't have inconnections and Thresholds. They always output with strength 100.
    public ChessGene MakeInputGene(){
        int outConnections = Random.Range(1,maxOutput);
        List<int> outList = new List<int>();
        for(int i = 0; i < outConnections; i++){
            int tempTrans = Random.Range(0,highestTransmitterNumber+2);
            outList.Add(tempTrans);
            if(tempTrans > highestTransmitterNumber){
                highestTransmitterNumber = tempTrans;
            }
        }
        return new ChessGene(0, null, outList, 0, 0, 100);

    }

    public ChessGene MakeGene(int type){
        int inConnections = Random.Range(1,maxInput);
        int outConnections = Random.Range(1,maxOutput);
        int threshold = Random.Range(0,101);
        int outStr = Random.Range(0,101);
        int waitTime = Random.Range(0, maxWait);
        List<int> inList = new List<int>();
        List<int> outList = new List<int>();
        for(int i = 0; i < inConnections; i++){
            int tempTrans = Random.Range(0,highestTransmitterNumber+2);
            inList.Add(tempTrans);
            if(tempTrans > highestTransmitterNumber){
                highestTransmitterNumber = tempTrans;
            }
        }
        for(int i = 0; i < outConnections; i++){
            if(type != 2){
                int tempTrans = Random.Range(0,highestTransmitterNumber+2);
                outList.Add(tempTrans);
                if(tempTrans > highestTransmitterNumber){
                    highestTransmitterNumber = tempTrans;
                }
            }
            else{
                outList.Add(Random.Range(0, outputNumber));
            }
        }
        return new ChessGene(type, inList, outList, waitTime, threshold, outStr);
    }

    public ChessAI BuildNetworkFromGene(ChessAIDNA gene){
        // Keep a list of all neurons that are available to make new connections (initialize at input neurons)
        // If the neuron is set to wait, let it wait, otherwise, find a neuron that can be connected
        // If there is no neuron available with the correct transmitter, make a new neuron with the settings from the gene

        ChessAI aI = new ChessAI(gene);
        aI.inputNeurons = new List<ChessNeuron>();

        List<ChessNeuron> availableOutNeurons = new List<ChessNeuron>();
        List<ChessNeuron> availableInNeurons = new List<ChessNeuron>();
        List<ChessNeuron> outputNeurons = new List<ChessNeuron>();
        List<ChessNeuron> allNeurons = new List<ChessNeuron>();

        // First, make the list of output neurons. 
        outputNeurons = MakeOutputNeurons();

        foreach(ChessDNA chessDNA in gene.DNA){
            ChessNeuron tempNeuron = new ChessNeuron(chessDNA.chessGenes);
            tempNeuron.MakeInputNeuron();
            aI.inputNeurons.Add(tempNeuron);
            availableOutNeurons.Add(tempNeuron);
            allNeurons.Add(tempNeuron);
        }
        while(availableOutNeurons.Count > 0){
            // Build the network
            List<ChessNeuron> nextAvailableList = new List<ChessNeuron>();
            foreach(ChessNeuron neuron in availableOutNeurons){
                if(neuron.CheckIfAvailable()){
                    List<int> possibleOut = neuron.CheckPossibleOut();
                    foreach(int i in possibleOut){
                        ChessNeuron possibleConnection = CheckPossibleIns(i, availableInNeurons);
                        if(possibleConnection != null){
                            neuron.ConnectToNeuron(possibleConnection, i);
                            if(possibleConnection.CheckIfInConnectionsFull()){
                                availableInNeurons.Remove(possibleConnection);
                            }
                        }
                        else{
                            ChessNeuron tempNeuron = neuron.MakeNewNeuron(i);
                            allNeurons.Add(tempNeuron);

                            if(tempNeuron.GetNeuronType() == 1){
                                neuron.ForceConnect(tempNeuron);
                                if(!tempNeuron.CheckIfInConnectionsFull()){
                                    availableInNeurons.Add(tempNeuron);
                                }
                                nextAvailableList.Add(tempNeuron);
                            }
                            else if(tempNeuron.GetNeuronType() == 2){
                                tempNeuron.MakePreOutputNeuron(outputNeurons);
                                if(!tempNeuron.CheckIfInConnectionsFull()){
                                    availableInNeurons.Add(tempNeuron);
                                }
                            }
                            else{
                                Debug.Log("Error: unexpected Neuron type?");
                            }
                        }
                    }
                }
                else{
                    // put the neuron in the nextavailableList with a position equal to its wait time
                    if(nextAvailableList.Count >= neuron.waitForConnection){
                        nextAvailableList.Insert(neuron.waitForConnection, neuron);
                        neuron.waitForConnection = 0;
                    }
                    else{
                        nextAvailableList.Add(neuron);
                        neuron.waitForConnection -= nextAvailableList.Count;
                    }
                }
            }
            availableOutNeurons = nextAvailableList;
        }
        aI.outputNeurons = outputNeurons;
        aI.allNeurons = allNeurons;

        return aI;
    }

    private List<ChessNeuron> MakeOutputNeurons(){
        List<ChessNeuron> outputList = new List<ChessNeuron>();

        for(int i = 0; i < outputNumber; i++){
            outputList.Add(new ChessNeuron(3));
        }

        return outputList;
    }

    private ChessNeuron CheckPossibleIns(int transmitter, List<ChessNeuron> possibleNeurons){
        foreach(ChessNeuron neuron in possibleNeurons){
            if(neuron.CheckConnection(transmitter)){
                return neuron;
            }
        }
        return null;
    }

    public List<ChessAI> NextGeneration(List<ChessAI> oldGen, List<int> pointsList, int maxPlayerNumber){
        return MakeChildren(CullAIs(oldGen, pointsList), pointsList, maxPlayerNumber);
    }

    private List<ChessAI> CullAIs(List<ChessAI> playerList, List<int> pointsList){
        // Cull the weak
        List<ChessAI> raffle = new List<ChessAI>();
        for(int i = 0; i < playerList.Count; i++){
            if(pointsList[i] < -5){
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
            }
            else if(pointsList[i] < 0){
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
            }
            else{
                raffle.Add(playerList[i]);
            }
        } 
        for(int i = 0; i < numToCull; i++){
            int toCUll = Random.Range(0, raffle.Count);
            ChessAI aIToCull = raffle[toCUll];
            playerList.Remove(aIToCull);
            raffle.RemoveAll(aI => aI == aIToCull);
        }
        return playerList;
    }

    private List<ChessAI> MakeChildren(List<ChessAI> playerList, List<int> pointsList, int maxPlayerNumber){
        List<ChessAI> raffle = new List<ChessAI>();
    	for(int i = 0; i < playerList.Count; i++){
            if(pointsList[i] > 5){
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
            }
            else if(pointsList[i] > 0){
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
                raffle.Add(playerList[i]);
            }
            else{
                raffle.Add(playerList[i]);
            }
        }
        while(playerList.Count < maxPlayerNumber){
            int father = Random.Range(0, raffle.Count);
            int mother = Random.Range(0, raffle.Count);
            if(father != mother){
                ChessAIDNA kid = CrossOver(raffle[father], raffle[mother]);
                kid = Mutate(kid);
                
                ChessAI newAI = BuildNetworkFromGene(kid);
                playerList.Add(newAI);
            }
        }
        
        Debug.Log("Next Generation ready!");
        return playerList;
    } 

    private ChessAIDNA CrossOver(ChessAI father, ChessAI mother){
        highestTransmitterNumber = father.aIDNA.highestTransmitterNumber;
        if(mother.aIDNA.highestTransmitterNumber > highestTransmitterNumber){
            highestTransmitterNumber = mother.aIDNA.highestTransmitterNumber;
        }
        List<ChessDNA> kidDNA = new List<ChessDNA>();
        for(int i = 0; i < father.aIDNA.DNA.Count; i++){
            int chooseOne = Random.Range(0,2);
            if(chooseOne == 0){
                kidDNA.Add(CopyDNA(father.aIDNA.DNA[i].chessGenes));
            }
            else{
                kidDNA.Add(CopyDNA(mother.aIDNA.DNA[i].chessGenes));
            }
        }
        return new ChessAIDNA(kidDNA, highestTransmitterNumber);
    }

    private ChessDNA CopyDNA(List<ChessGene> toCopy){
        List<ChessGene> DNACopy = new List<ChessGene>();

        foreach(ChessGene gene in toCopy){
            if(gene.neuronType == 0){
                DNACopy.Add(new ChessGene(0, null, gene.CopyOuts(), 0, 0, 100));
            }
            else{
                DNACopy.Add(new ChessGene(gene.neuronType, gene.CopyIns(), gene.CopyOuts(), 
                    gene.waitForConnection, gene.threshold, gene.outputStrength));
            }
        }

        return new ChessDNA(DNACopy);
    }

    private ChessAIDNA Mutate(ChessAIDNA toMutate){
        for(int i = 0; i < maxMutations; i++){
            // Possible mutations: 
            int mutationType = Random.Range(0, 14);
            // Switch two DNA strands
            if(mutationType == 0){
                int toSwitch1 = Random.Range(0, toMutate.DNA.Count);
                int toSwitch2 = Random.Range(0, toMutate.DNA.Count);
                ChessDNA tempDNA = CopyDNA(toMutate.DNA[toSwitch1].chessGenes);
                toMutate.DNA[toSwitch1] = CopyDNA(toMutate.DNA[toSwitch2].chessGenes);
                toMutate.DNA[toSwitch2] = tempDNA;
            }
            // Add a neuron in the DNA
            if(mutationType == 1){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int insertSpot = Random.Range(1, toMutate.DNA[randomDNA].chessGenes.Count);
                highestTransmitterNumber = toMutate.highestTransmitterNumber;
                ChessGene newGene = MakeGene(1);
                toMutate.DNA[randomDNA].chessGenes.Insert(insertSpot, newGene);
            }
            // Remove a neuron from the DNA
            if(mutationType == 2){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                // Can't remove if there's no internediate neurons
                if(toMutate.DNA[randomDNA].chessGenes.Count > 2){
                    int removeSpot = Random.Range(1, toMutate.DNA[randomDNA].chessGenes.Count-1);
                    toMutate.DNA[randomDNA].chessGenes.RemoveAt(removeSpot);
                }
            }
            // Change a Threshold
            if(mutationType == 3){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                toMutate.DNA[randomDNA].chessGenes[randomNeuron].threshold = Random.Range(0,101);
            }
            // Add input
            if(mutationType == 4){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                if(toMutate.DNA[randomDNA].chessGenes[randomNeuron].neuronType != 0){
                    int numIn = toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsIn.Count;
                    if(numIn < maxInput){
                        int tempTrans = Random.Range(0,highestTransmitterNumber+2);
                        toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsIn.Add(tempTrans);
                        if(tempTrans > highestTransmitterNumber){
                            highestTransmitterNumber = tempTrans;
                        }
                    }
                }
            }
            // Remove input
            if(mutationType == 5){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                if(toMutate.DNA[randomDNA].chessGenes[randomNeuron].neuronType != 0){
                    int numIn = toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsIn.Count;
                    if(numIn > 1){
                        int toRemove = Random.Range(0, numIn);
                        toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsIn.RemoveAt(toRemove);
                    }
                }
                
            }
            // Add output
            if(mutationType == 6){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                if(toMutate.DNA[randomDNA].chessGenes[randomNeuron].neuronType != 2){
                    int numOut = toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsOut.Count;
                    if(numOut < maxOutput){
                        int tempTrans = Random.Range(0,highestTransmitterNumber+2);
                        toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsOut.Add(tempTrans);
                        if(tempTrans > highestTransmitterNumber){
                            highestTransmitterNumber = tempTrans;
                        }
                    }
                }
            }
            // Remove output
            if(mutationType == 7){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                if(toMutate.DNA[randomDNA].chessGenes[randomNeuron].neuronType != 2){
                    int numOut = toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsOut.Count;
                    if(numOut > 1){
                        int toRemove = Random.Range(0, numOut);
                        toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsOut.RemoveAt(toRemove);
                    }
                }
            }
            // Change output strength
            if(mutationType == 8){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                toMutate.DNA[randomDNA].chessGenes[randomNeuron].outputStrength = Random.Range(0,101);
            }
            // Change a transmitter value in Inputs
            if(mutationType == 9){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                if(toMutate.DNA[randomDNA].chessGenes[randomNeuron].neuronType != 0){
                    int randomInput = Random.Range(0, toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsIn.Count);
                    int transmitterNum = Random.Range(0, highestTransmitterNumber+2);
                    toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsIn[randomInput] = transmitterNum;
                    if(transmitterNum > highestTransmitterNumber){
                        highestTransmitterNumber = transmitterNum;
                    }
                }
            }
            // Change a transmitter value in Outputs
            if(mutationType == 10){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int randomNeuron = Random.Range(0,toMutate.DNA[randomDNA].chessGenes.Count);
                if(toMutate.DNA[randomDNA].chessGenes[randomNeuron].neuronType != 2){
                    int randomOutput = Random.Range(0,toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsOut.Count);
                    int transmitterNum = Random.Range(0, highestTransmitterNumber+2);
                    toMutate.DNA[randomDNA].chessGenes[randomNeuron].possibleConnectionsOut[randomOutput] = transmitterNum;
                    if(transmitterNum > highestTransmitterNumber){
                        highestTransmitterNumber = transmitterNum;
                    }
                }
            }
            // Add output of pre-output neuron
            if(mutationType == 11){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int preOutputLocation = toMutate.DNA[randomDNA].chessGenes.Count-1;
                if(toMutate.DNA[randomDNA].chessGenes[preOutputLocation].neuronType != 2){
                    Debug.Log("I fucked up captain");
                }
                int numOutputs = toMutate.DNA[randomDNA].chessGenes[preOutputLocation].possibleConnectionsOut.Count;
                if(numOutputs < 5){
                    int randomOut = Random.Range(0, outputNumber);
                    toMutate.DNA[randomDNA].chessGenes[preOutputLocation].possibleConnectionsOut.Add(randomOut);
                }
            }
            // Remove output of pre-output neuron
            if(mutationType == 12){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int preOutputLocation = toMutate.DNA[randomDNA].chessGenes.Count-1;
                if(toMutate.DNA[randomDNA].chessGenes[preOutputLocation].neuronType != 2){
                    Debug.Log("I fucked up captain");
                }
                int numOutputs = toMutate.DNA[randomDNA].chessGenes[preOutputLocation].possibleConnectionsOut.Count;
                if(numOutputs > 1){
                    int randomOut = Random.Range(0, numOutputs);
                    toMutate.DNA[randomDNA].chessGenes[preOutputLocation].possibleConnectionsOut.RemoveAt(randomOut);
                }
            }
            // Change value of pre-output output
            if(mutationType == 13){
                int randomDNA = Random.Range(0, toMutate.DNA.Count);
                int preOutputLocation = toMutate.DNA[randomDNA].chessGenes.Count-1;
                if(toMutate.DNA[randomDNA].chessGenes[preOutputLocation].neuronType != 2){
                    Debug.Log("I fucked up captain");
                }
                int randomOut = Random.Range(0, outputNumber);
                int toRandom = Random.Range(0, toMutate.DNA[randomDNA].chessGenes[preOutputLocation].possibleConnectionsOut.Count);
                toMutate.DNA[randomDNA].chessGenes[preOutputLocation].possibleConnectionsOut[toRandom] = randomOut;
            }
        }

        return toMutate;
    }
}
