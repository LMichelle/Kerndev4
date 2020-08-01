using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KernDev.NetworkBehaviour;
using KernDev.GameLogic;
using System.Linq;

public class HostGameManager : MonoBehaviour
{
    public GameObject gridGO;
    public GameObject testSpawnPlayersGO;
    private GridSystem grid;
    private ServerBehaviour server;
    private List<Client> clientList;
    private Dictionary<Client, Player> AllClientPlayerDictionary = new Dictionary<Client, Player>();
    private Dictionary<Client, Player> ActiveClientPlayerDictionary = new Dictionary<Client, Player>();
    private Dictionary<Player, Client> ActivePlayerClientDictionary = new Dictionary<Player, Client>();
    private List<Player> playerTurnList = new List<Player>();
    private TurnManager turnManager;

    private void Start()
    {
        // Instantiate grid
        Instantiate(gridGO);
        grid = gridGO.GetComponent<GridSystem>();
        grid.StartGrid();

        // Get clients and link them to their new player info
        server = GameObject.FindGameObjectWithTag("Server").GetComponent<ServerBehaviour>();
        clientList = server.clientList;
        foreach(Client c in clientList)
        {
            Player player = new Player();
            AllClientPlayerDictionary.Add(c, player);
            ActiveClientPlayerDictionary.Add(c, player);
            ActivePlayerClientDictionary.Add(player, c);
            playerTurnList.Add(player);
            player.SetStartHP(c.StartHP);
        }

        // spawn players in a room
        StartCoroutine(SpawnPlayers());

        // spawn monsters in random rooms
        // spawn treasures in random rooms

        // send the turn
        TurnExecution();
    }

    private IEnumerator SpawnPlayers()
    {
        // if the grid isn't ready yet, then keep looping. If it is finished, the next thing can be done.
        if (!grid.finishedGenerating)
        {
            yield return new WaitForSeconds(.2f);
        }
        
        foreach (Player player in AllClientPlayerDictionary.Values)
        {
            player.CurrentNode = grid.GetRandomNode();
            //Debug.Log(player.CurrentNode.pos);
            SpawnTestPlayerObjects(player.CurrentNode.pos);
        }
        yield break;
    }

    private void SpawnMonsters()
    {

    }

    private void TurnExecution()
    {
        // Send player turn
        int turn = turnManager.NextTurn(ActiveClientPlayerDictionary.Count);
        Player currentActivePlayer = playerTurnList[turn];
        Client client;
        ActivePlayerClientDictionary.TryGetValue(currentActivePlayer, out client); // make this into a struct

        // Send Player turn
        // Send Room Info
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

    // testing stuff

    private void SpawnTestPlayerObjects(Vector3 pos)
    {
        Instantiate(testSpawnPlayersGO, pos, testSpawnPlayersGO.transform.rotation);
    }
}
