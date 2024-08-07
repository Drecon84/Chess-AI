using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIIOManager 
{
    public ChessManager chessManager;

    public bool player1Move;

    public List<int> outputs;

    public ChessAI player1;
    public ChessAI player2;

    //public bool playerLostGame = false;

    //public bool moveSucessful; 

    //bool movingPiece = false;

    public AIIOManager(ChessManager cm, ChessAI p1, ChessAI p2){
        chessManager = cm;
        player1 = p1;
        player2 = p2;
    }

    // Every frame: 
    // Read inputs, pass to players
    // NextStep
    // ReadOutputs
    // Pass to gameManager
    public void PerformActions(){
        List<int> inputs = ReadInputs();
        player1.GetInputs(inputs);
        player2.GetInputs(inputs);
        bool whiteTurn = chessManager.whiteTurn;
        NextStep(whiteTurn);
        if(whiteTurn){
            outputs = player1.GetOutputs();
            InterpretOutputs(whiteTurn);
        }
        else{
            outputs = player2.GetOutputs();
            InterpretOutputs(whiteTurn);
        }

    }

    public void ConnectPlayers(ChessAI p1, ChessAI p2){
        player1 = p1;
        player2 = p2;
    }

    public List<int> ReadInputs(){
        List<int> inputs = new List<int>();

        // move through all white pieces
        for(int i = 0; i < 6; i++){
            foreach(Field f in chessManager.board.gameBoard){
                if(f.piece != null){
                    if(f.piece.type == i){
                        inputs.Add(100);
                    }
                    else{
                        inputs.Add(0);
                    }
                }
            }
        }
        // move through all black pieces
        for(int i = 0; i < 6; i++){
            foreach(Field f in chessManager.board.gameBoard){
                if(f.piece != null){
                    if(f.piece.type == i){
                        inputs.Add(100);
                    }
                    else{
                        inputs.Add(0);
                    }
                }
            }
        }

        return inputs;
    }

    public void FeedInputs(bool whiteTurn){
        List<int> inputs = ReadInputs();
        if(whiteTurn){
            inputs.Add(100);
            player1.GetInputs(inputs);
            inputs.RemoveAt(inputs.Count-1);
            inputs.Add(0);
            player2.GetInputs(inputs);
        }
        else{
            inputs.Add(0);
            player1.GetInputs(inputs);
            inputs.RemoveAt(inputs.Count-1);
            inputs.Add(100);
            player2.GetInputs(inputs);
        }
    }

    public void NextStep(bool whiteTurn){
        player1.ActivateAllNeurons();
        player2.ActivateAllNeurons();
    }

    public void InterpretOutputs(bool whiteTurn){
        // Currently using output sensitivity of 100, might make this lower in the future
        if(outputs[outputs.Count-1] >= 100){
            int highestActivation = 0;
            int secondHighestActivation = 0;
            int highestNeuronPosition = 0;
            int secondhighestNeuronPosition = 0;
            for(int i = outputs.Count - 4; i < outputs.Count; i++){
                if(outputs[i] > highestActivation){
                    highestActivation = outputs[i];
                    highestNeuronPosition = i;
                }
            }
            chessManager.board.promotePawnTo(highestNeuronPosition - outputs.Count - 4);
            highestActivation = 0;
            highestNeuronPosition = 0;
            Field highestField;
            Field secondHighestField;
            for(int i = 0; i < outputs.Count-5; i++){
                if(outputs[i] > highestActivation){
                    secondHighestActivation = highestActivation;
                    highestActivation = outputs[i];
                    secondhighestNeuronPosition = highestNeuronPosition;
                    highestNeuronPosition = i;
                }
                else if(outputs[i] > secondHighestActivation){
                    secondHighestActivation = outputs[i];
                    secondhighestNeuronPosition = i;
                }
            }
            int tempHighest = highestNeuronPosition;
            int tempSecond = secondhighestNeuronPosition;
            int row = tempHighest / 8;
            int column = tempHighest % 8;
            highestField = chessManager.board.gameBoard[column,row];
            row = tempSecond / 8;
            column = tempSecond % 8;
            secondHighestField = chessManager.board.gameBoard[column,row];
            if(highestField.piece != null){
                if(highestField.piece.colorIsWhite == whiteTurn){
                    if(!chessManager.movingPiece){
                        chessManager.MoveHelper(whiteTurn, highestField.position, secondHighestField.position);
                    }                    
                }
                else{
                    if(!chessManager.movingPiece){
                        chessManager.MoveHelper(whiteTurn, highestField.position, secondHighestField.position);
                    }
                }
            }
            else{
                if(!chessManager.movingPiece){
                    chessManager.MoveHelper(whiteTurn, highestField.position, secondHighestField.position);
                }
            }
        }
    }
}
