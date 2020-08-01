using System.Collections.Generic;
using UnityEngine;
using KernDev.NetworkBehaviour;
using KernDev.GameLogic;

public class HostGameManager : MonoBehaviour
{
    public GameObject gridGO;
    private GridSystem grid;
    private ServerBehaviour server;
    private List<Client> clientList;
    private Dictionary<Client, Player> AllClientPlayerDictionary = new Dictionary<Client, Player>();
    private Dictionary<Client, Player> ActiveClientPlayerDictionary = new Dictionary<Client, Player>();
    private List<Player> playerTurnList = new List<Player>();
    private TurnManager turnManager;

    private void Start()
    {
        // Instantiate grid
        Instantiate(gridGO);
        grid = gridGO.GetComponent<GridSystem>();

        // Get clients and link them to their new player info
        server = GameObject.FindGameObjectWithTag("Server").GetComponent<ServerBehaviour>();
        clientList = server.clientList;
        foreach(Client c in clientList)
        {
            Player player = new Player();
            AllClientPlayerDictionary.Add(c, player);
            ActiveClientPlayerDictionary.Add(c, player);
            playerTurnList.Add(player);
            player.SetStartHP(c.StartHP);
        }

        // spawn players in a room
        SpawnPlayers();

        // spawn monsters in random rooms
        // spawn treasures in random rooms
        // determine turn order
        
        // send the turn
    }

    private void SpawnPlayers()
    {
        // if the grid isn't ready yet, then keep looping. If it is finished, the next thing can be done.
        while (!grid.finishedGenerating)
            break;
        foreach (Player player in AllClientPlayerDictionary.Values)
        {
            player.CurrentNode = grid.GetRandomNode();
            Debug.Log(player.CurrentNode);
        }
    }

    private void SpawnMonsters()
    {

    }

    private void TurnExecution()
    {
        // Send player turn
        int turn = turnManager.NextTurn(ActiveClientPlayerDictionary.Count);
        Player currentActivePlayer = playerTurnList[turn];
    }

    #region Send Game Messages

    /// <summary>
    /// Send whose turn it is to all clients.
    /// </summary>
    private void SendPlayerTurn() 
    { 
    
    }

    /// <summary>
    /// Send info of the current room to the client as an answer to the MoveRequest & Start of the Game.
    /// </summary>
    private void SendRoomInfo()
    {

    }

    /// <summary>
    /// When a player enters a room, send this to all clients in the same room.
    /// </summary>
    private void SendPlayerEnterRoom()
    {

    }

    /// <summary>
    /// When a player leaves a room, send this to all clients in the room.
    /// </summary>
    private void SendPlayerLeaveRoom()
    {

    }

    /// <summary>
    /// Send to the client how much treasure he obtained.
    /// </summary>
    private void SendObtainTreasure()
    {

    }

    /// <summary>
    /// Send to all clients when a monster gets hit.
    /// </summary>
    private void SendHitMonster()
    {

    }

    /// <summary>
    /// Send to all clients when a player gets hit by a monster.
    /// </summary>
    private void SendHitByMonster()
    {

    }

    /// <summary>
    /// Send to all clients when a player defends and HP heals.
    /// </summary>
    private void SendPlayerDefends()
    {

    }

    /// <summary>
    /// Send to all clients when a player leaves the dungeon.
    /// </summary>
    private void SendPlayerLeftDungeon()
    {

    }

    /// <summary>
    /// Send to all clients when a player dies.
    /// </summary>
    private void SendPlayerDies() 
    { 

    }

    /// <summary>
    /// Send to all clients when the game ends, with the high score in it.
    /// </summary>
    private void SendEndGame()
    {

    }
    #endregion

    
}
