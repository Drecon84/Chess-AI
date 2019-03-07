using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class TournamentManager : MonoBehaviour
{
    public AIIOManager iOManager;

    public ChessManager chessManager;

    ChessNetworkBuilder networkBuilder;

    public List<ChessAI> playerList;

    public PlayerList playerDNA;

    public List<int> pointsList;

    int maxPlayerNumber = 200;

    int gamesInRound = 400;

    int currentGame;

    bool whitePlayerLost;
    bool blackPlayerLost;

    int player1Index;
    int player2Index;

    XMLSaving xmlSaving;

    public string path;

    public bool waitforMakePlayers = false;

    public void StartTournament(){
        path = Application.dataPath + "/ChessPlayers.xml";
        xmlSaving = new XMLSaving();
        playerDNA = xmlSaving.Load(path);
        if(playerDNA == null){
            playerDNA = new PlayerList();
            CreatePlayers();
        }     
        else{
            pointsList = new List<int>();
            CreatePlayersFromDNA();
            Debug.Log("Loaded players");
        }  
    }

    public void NextRound(){
        Debug.Log("Starting next round...");
        currentGame = 0;
        pointsList = new List<int>();
        for(int i = 0; i < maxPlayerNumber; i++){
            pointsList.Add(0);
        }
        NextGame();
    }

    public void NextGame(){ 
        whitePlayerLost = false;
        blackPlayerLost = false;
        player1Index = Random.Range(0, playerList.Count-1);
        player2Index = Random.Range(0, playerList.Count-1);
        // Will cause an infinite loop if there's exactly one player, that should never happen
        while(player1Index == player2Index){
            player2Index = Random.Range(0, playerList.Count-1);
        }
        iOManager = new AIIOManager(chessManager, playerList[player1Index], playerList[player2Index]);
        chessManager.SetPlayerNames(playerList[player1Index].aIDNA.aiName, playerList[player2Index].aIDNA.aiName);
        CreateGame(playerList[player1Index], playerList[player2Index]);
        chessManager.StartNewGame(2);
    }

    public void PauseTournament(){
        // Save progress
        Debug.Log("Pausing not yet implemented");
    }

    public void PlayerLostGame(bool whitePlayer){
        if(whitePlayer){
            whitePlayerLost = true;
        }
        else{
            blackPlayerLost = true;
        }
    }


    void Update(){
        if(!chessManager.gameActive){
            if(whitePlayerLost){
                // Update points list, we want to give different points for different types of Victories!
                // if round not over, start new game
                if(currentGame < gamesInRound){
                    playerList[player1Index].gameList.Add(new ChessGame(chessManager.board.moveList, true));
                    playerList[player2Index].gameList.Add(new ChessGame(chessManager.board.moveList, false));
                    ScorePlayers();
                    currentGame++;
                    NextGame();
                }
                else{
                    if(!waitforMakePlayers){
                        playerList[player1Index].gameList.Add(new ChessGame(chessManager.board.moveList, true));
                        playerList[player2Index].gameList.Add(new ChessGame(chessManager.board.moveList, false));
                        ScorePlayers();
                        waitforMakePlayers = true;
                        whitePlayerLost = false;
                        FindBestGame();
                        xmlSaving.Save(path, playerDNA);
                        StartCoroutine(MakePlayers());
                    }
                    
                }

            }
            if(blackPlayerLost){
                // if round not over, start new game
                if(currentGame < gamesInRound){
                    playerList[player1Index].gameList.Add(new ChessGame(chessManager.board.moveList, true));
                    playerList[player2Index].gameList.Add(new ChessGame(chessManager.board.moveList, false));
                    ScorePlayers();
                    currentGame++;
                    NextGame();
                }
                else{
                    if(!waitforMakePlayers){
                        playerList[player1Index].gameList.Add(new ChessGame(chessManager.board.moveList, true));
                        playerList[player2Index].gameList.Add(new ChessGame(chessManager.board.moveList, false));
                        ScorePlayers();
                        waitforMakePlayers = true;
                        blackPlayerLost = false;
                        FindBestGame();
                        xmlSaving.Save(path, playerDNA);
                        StartCoroutine(MakePlayers());
                    }
                }
            }
        }
        if(chessManager.gameActive && !waitforMakePlayers && !chessManager.movingPiece){
            iOManager.PerformActions();
        }
    }

    public void CreatePlayers(){
        playerList = new List<ChessAI>();
        pointsList = new List<int>();
        int numInputs = 64*12+1;
        int numOutputs = 64 + 4 + 1;
        networkBuilder = new ChessNetworkBuilder(numInputs, numOutputs, 4, 4, 10, 5);
        for(int i = 0; i < maxPlayerNumber; i++){
            ChessAIDNA aIDNA = networkBuilder.BuildGene();
            playerDNA.playerList.Add(aIDNA);
            playerList.Add(networkBuilder.BuildNetworkFromGene(aIDNA));
            pointsList.Add(0);
        }
        Debug.Log("Done making players");
    }

    public void CreatePlayersFromDNA(){
        playerList = new List<ChessAI>();
        pointsList = new List<int>();
        int numInputs = 64*12+1;
        int numOutputs = 64 + 4 + 1;
        networkBuilder = new ChessNetworkBuilder(numInputs, numOutputs, 4, 4, 10, 5);
        foreach(ChessAIDNA gene in playerDNA.playerList){
            playerList.Add(networkBuilder.BuildNetworkFromGene(gene));
            pointsList.Add(0);
        } 
    }

    public IEnumerator MakePlayers(){
        playerList = networkBuilder.NextGeneration(playerList, pointsList, maxPlayerNumber);
        playerDNA.playerList = new List<ChessAIDNA>();
        foreach(ChessAI player in playerList){
            playerDNA.playerList.Add(player.aIDNA);
        }
        yield return new WaitForEndOfFrame();
        waitforMakePlayers = false;
        yield break;
    }

    public void CreateGame(ChessAI player1, ChessAI player2){
        iOManager.ConnectPlayers(player1, player2);
    }

    public void ScorePlayers(){
        // Currently a draw awards 0 points... is this correct? 
        if(whitePlayerLost){
            if(chessManager.board.CheckIfCheckMate(true)){
                pointsList[player1Index]--;
                pointsList[player2Index] += 20;
            }
            else if(!chessManager.tryMove){
                pointsList[player1Index] -= 0;
                pointsList[player2Index]++;
            }
            else if(playerList[player1Index].gameList[playerList[player1Index].gameList.Count-1].moveList.Count > 15){
                pointsList[player1Index]++;
                pointsList[player2Index] += 7;
            }
            else if(playerList[player1Index].gameList[playerList[player1Index].gameList.Count-1].moveList.Count > 5){
                pointsList[player1Index]++;
                pointsList[player2Index] += 5;
            }
            else if(playerList[player1Index].gameList[playerList[player1Index].gameList.Count-1].moveList.Count > 0){
                pointsList[player1Index]++;
                pointsList[player2Index] += 3;        
            }
            else if(chessManager.timerP1 <= 0){
                pointsList[player1Index] -= 5;
                pointsList[player2Index]++;
            }
        }
        if(blackPlayerLost){
            if(chessManager.board.CheckIfCheckMate(true)){
                pointsList[player1Index] += 20;
                pointsList[player2Index]--;
            }
            else if(!chessManager.tryMove){
                pointsList[player1Index]++;
                pointsList[player2Index] -= 0;
            }
            else if(playerList[player2Index].gameList[playerList[player2Index].gameList.Count-1].moveList.Count > 15){
                pointsList[player1Index] += 7;
                pointsList[player2Index]++;
            }
            else if(playerList[player2Index].gameList[playerList[player2Index].gameList.Count-1].moveList.Count > 5){
                pointsList[player1Index] += 5;
                pointsList[player2Index]++;
            }
            else if(playerList[player2Index].gameList[playerList[player2Index].gameList.Count-1].moveList.Count > 0){
                pointsList[player1Index] += 3;
                pointsList[player2Index]++;
            }  
            else if(chessManager.timerP2 <= 0){
                pointsList[player1Index] += 3;
                pointsList[player2Index] -= 5;
            }         
        }
    }

    public void FindBestGame(){
        int highestScore = int.MinValue;
        int bestPlayerIndex = 0;
        for(int i = 0; i < playerList.Count; i++){
            if(pointsList[i] > highestScore){
                highestScore = pointsList[i];
                bestPlayerIndex = i;
            }
        }
        List<ChessGame> gamesList = playerList[bestPlayerIndex].gameList;
        if(gamesList[gamesList.Count-1].moveList.Count > 0){
            Debug.Log("Number of moves in last game of best player: " + gamesList[gamesList.Count-1].moveList.Count);
        }
        else{
            Debug.Log("Best player has not made moves in latest game.");
        }
    }
}
