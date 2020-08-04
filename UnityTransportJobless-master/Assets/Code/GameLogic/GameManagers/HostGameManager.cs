using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KernDev.NetworkBehaviour;
using KernDev.GameLogic;

/// <summary>
/// Script on the Host that talks to the server and executes game logic.
/// </summary>
public class HostGameManager : MonoBehaviour
{
    public GameObject fakeMonster;
    public GameObject gridGO;
    public GameObject testSpawnPlayersGO;

    [SerializeField]
    private int minPlayerToMonsterDmg, maxPlayerToMonsterDmg, minMonsterToPlayerDmg, maxMonsterToPlayerDmg;
    private int minMonsterHP = 10, maxMonsterHP = 15, minMonsterDmg = 1, maxMonsterDmg = 5;
    
    private GridSystem grid;
    private ServerBehaviour server;
    private TurnManager turnManager = new TurnManager();

    private List<Client> clientList; // List of all clients
    private Dictionary<Client, Player> AllClientPlayerDictionary = new Dictionary<Client, Player>(); // All clients and their player info
    private Dictionary<Client, Player> ActiveClientPlayerDictionary = new Dictionary<Client, Player>(); // all clients + players still in the turns
    private Dictionary<Player, Client> ActivePlayerClientDictionary = new Dictionary<Player, Client>(); // inverse dictionary
    private List<Player> playerTurnList = new List<Player>(); // The list order is the turn order
    private Player currentActivePlayer;
    private List<Monster> monsterList = new List<Monster>();

    private int randomMonsterAmount;

    private void Start()
    {
        // Instantiate grid
        Instantiate(gridGO);
        grid = gridGO.GetComponent<GridSystem>();
        grid.StartGrid();

        // Get clients and link them to their new player info
        server = GameObject.FindGameObjectWithTag("Server").GetComponent<ServerBehaviour>();
        server.HostGameManager = this;
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
        SpawnMonsters();
        // spawn treasures in random rooms

        // send the turn
        turnManager.Turn = ActiveClientPlayerDictionary.Count - 1;
        TurnExecution();
    }

    private IEnumerator SpawnPlayers()
    {
        // if the grid isn't ready yet, then keep looping.
        if (!grid.finishedGenerating)
        {
            yield return new WaitForSeconds(.2f);
        }
        
        // Set each player to a random room/node in the maze/grid
        foreach (Player player in AllClientPlayerDictionary.Values)
        {
            player.CurrentNode = grid.GetRandomNode();
            SpawnTestPlayerObjects(player.CurrentNode.pos); //testing
        }
        yield break;
    }

    private void SpawnMonsters()
    {
        randomMonsterAmount = Random.Range(1, 6);
        for (int i = 0; i <= randomMonsterAmount; i++)
        {
            Node monsterNode = grid.GetRandomNode();
            if (monsterNode.Monster) // if this node already has a monster
            {
                while (monsterNode.Monster) // keep getting a new random node until monsternode does not have a monster
                {
                    monsterNode = grid.GetRandomNode();
                }
            }
            monsterNode.Monster = true; 
            // Make a monster
            Monster monster = new Monster {
                DamageAmount = Random.Range(minMonsterDmg, maxMonsterDmg),
                CurrentNode = monsterNode
            };
            
            monster.SetStartHP(Random.Range(minMonsterHP, maxMonsterHP));
            monsterList.Add(monster);
            Instantiate(fakeMonster, monster.CurrentNode.pos, Quaternion.identity); // testing
        }
    }

    private void TurnExecution()
    {
        // Send player turn
        int turn = turnManager.NextTurn(ActiveClientPlayerDictionary.Count);
        currentActivePlayer = playerTurnList[turn];
        Client currentActiveClient;
        ActivePlayerClientDictionary.TryGetValue(currentActivePlayer, out currentActiveClient); // make the dictionaries into a struct, tip from Geoffrey. EDIT struct should be used for value types, not reference types

        // Send Player turn
        SendPlayerTurn(currentActiveClient);
        
        // Send Room Info
        SendRoomInfo(currentActivePlayer.CurrentNode, currentActiveClient);
    }

    #region Send Game Messages

    /// <summary>
    /// Send whose turn it is to all clients.
    /// </summary>
    private void SendPlayerTurn(Client activeTurnClient) 
    {
        var playerTurnMessage = new PlayerTurnMessage {
            PlayerID = activeTurnClient.PlayerID,
        };
        foreach(Client c in AllClientPlayerDictionary.Keys)
        {
            server.SendMessage(playerTurnMessage, c.Connection);
        }
    }

    /// <summary>
    /// Send info of the current room to the client as an answer to the MoveRequest & Start of the Game.
    /// </summary>
    private void SendRoomInfo(Node roomNode, Client activeTurnClient)
    {
        // testing
        int treasureAmt = 300;
        Wall openDirection = roomNode.GetOpenDirection();
        byte monster = System.Convert.ToByte(roomNode.Monster);

        // Not complete!!
        var roomInfoMessage = new RoomInfoMessage {
            MoveDirections = (byte)openDirection,
            TreasureInRoom = (ushort)treasureAmt,
            ContainsMonster = monster,
            ContainsExit = 0,
            NumberOfOtherPlayers = 0,
            OtherPlayerIDs = {0}
        };
        server.SendMessage(roomInfoMessage, activeTurnClient.Connection);
    }

    /// <summary>
    /// When a player enters a room, send this to all clients in the same room.
    /// </summary>
    private void SendPlayerEnterRoom()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// When a player leaves a room, send this to all clients in the room.
    /// </summary>
    private void SendPlayerLeaveRoom()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Send to the client how much treasure he obtained.
    /// </summary>
    private void SendObtainTreasure()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Send to all clients when a monster gets hit.
    /// </summary>
    private void SendHitMonster()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Send to all clients when a player gets hit by a monster.
    /// </summary>
    private void SendHitByMonster()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Send to all clients when a player defends and HP heals.
    /// </summary>
    private void SendPlayerDefends()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Send to all clients when a player leaves the dungeon.
    /// </summary>
    private void SendPlayerLeftDungeon()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Send to all clients when a player dies.
    /// </summary>
    private void SendPlayerDies() 
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Send to all clients when the game ends, with the high score in it.
    /// </summary>
    private void SendEndGame()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region React To Requests
    public void HandleMoveRequest(MessageConnection messageConnection)
    {
        var moveRequestMessage = (messageConnection.messageHeader as MoveRequestMessage);
        Node newRoomNode = grid.GetSpecificNeighbourNode(currentActivePlayer.CurrentNode, (Wall)moveRequestMessage.Direction);
        currentActivePlayer.CurrentNode = newRoomNode;

        // Player has moved, so now it's the next player's turn.
        TurnExecution();
    }

    public void HandleAttackRequest(MessageConnection messageConnection)
    {
        var attackRequestMessage = (messageConnection.messageHeader as AttackRequestMessage);
        // get the current active player and the monster in that node
        Node monsterNode = grid.GetSpecificNodeInstance(currentActivePlayer.CurrentNode);
        Monster monster = new Monster();
        foreach(Monster m in monsterList)
        {
            if (m.CurrentNode == monsterNode)
                monster = m;
        }
        
        // random bool playerHitfirst, if true player hits first
        bool hitPlayerFirst = true;
        //if (Random.value >= .5f)
            //hitPlayerFirst = true;

        if (hitPlayerFirst)
        {
            monster.TakeDamage(Random.Range(minPlayerToMonsterDmg, maxPlayerToMonsterDmg));
            //SendHitMonster();

            if (monster.Dead)
            {
                monsterNode.Monster = false;
            }
            else
            {
                currentActivePlayer.TakeDamage(Random.Range(minMonsterToPlayerDmg, maxMonsterToPlayerDmg));
                SendHitByMonster();
            }           
        }
        else
        {
            currentActivePlayer.TakeDamage(Random.Range(minMonsterToPlayerDmg, maxMonsterToPlayerDmg));
            SendHitByMonster();

            monster.TakeDamage(Random.Range(minPlayerToMonsterDmg, maxPlayerToMonsterDmg));
            SendHitMonster();
        }

        // Everything done? Next turn.
    }

    public void HandleDefendRequest(MessageConnection messageConnection)
    {
        var defendRequestMessage = (messageConnection.messageHeader as DefendRequestMessage);
    }

    public void HandleClaimTreasureRequest(MessageConnection messageConnection)
    {
        var claimTreasureRequestMessage = (messageConnection.messageHeader as ClaimTreasureRequestMessage);
    }

    public void HandleLeaveDungeonRequest(MessageConnection messageConnection)
    {
        var leaveDungeonRequestMessage = (messageConnection.messageHeader as LeaveDungeonRequestMessage);
    }
    #endregion

    // testing stuff
    private void SpawnTestPlayerObjects(Vector3 pos)
    {
        Instantiate(testSpawnPlayersGO, pos, testSpawnPlayersGO.transform.rotation);
    }
}
