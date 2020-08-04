﻿using System.Collections;
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
    public GameObject treasureGO;
    public GameObject gridGO;
    public GameObject testSpawnPlayersGO;

    [SerializeField]
    private int minMonsterHP = 10, maxMonsterHP = 15, minMonsterDmg = 1, maxMonsterDmg = 5;
    private int minPlayerDmg = 3, maxPlayerDmg = 7;
    [SerializeField]
    private int variance = 2;
    [SerializeField]
    private int minHealAmt = 2, maxHealAmt = 7;
    private int minTreasureAmt = 200, maxTreasureAmt = 300;
    
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
            player.SetStartValues(c.StartHP);
            player.DamageAmount = Random.Range(minPlayerDmg, maxPlayerDmg);
        }

        // spawn players, monsters and treasures.
        StartCoroutine(SpawnPlayers());
        SpawnMonsters();
        SpawnTreasures();
        SpawnDungeonExit();

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
            
            monster.SetStartValues(Random.Range(minMonsterHP, maxMonsterHP));
            monsterList.Add(monster);
            Instantiate(fakeMonster, monster.CurrentNode.pos, Quaternion.identity); // testing
        }
    }

    private void SpawnTreasures()
    {
        int randomTreasureAmt = Random.Range(10, 30);
        for (int i = 0; i <= randomTreasureAmt; i++)
        {
            Node treasureNode = grid.GetRandomNode();
            if (treasureNode.Treasure)
            {
                while (treasureNode.Treasure)
                    treasureNode = grid.GetRandomNode();
            }
            treasureNode.Treasure = true;
            Instantiate(treasureGO, treasureNode.pos, Quaternion.identity); // testing
        }
    }

    private void SpawnDungeonExit()
    {
        Node exitNode = grid.GetRandomNode();
        exitNode.DungeonExit = true;
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
        int treasureAmt = 0;
        Wall openDirection = roomNode.GetOpenDirection();
        byte monster = System.Convert.ToByte(roomNode.Monster);
        byte exit = System.Convert.ToByte(roomNode.DungeonExit);
        int numberOfOtherPlayers = 0;
        List<int> otherPlayerIDs = new List<int>();
        if (roomNode.Treasure)
        {
            treasureAmt = Random.Range(minTreasureAmt, maxTreasureAmt);
            roomNode.TreasureAmount = treasureAmt;
        }
        // Get other Player info
        //foreach(KeyValuePair<Client, Player> clientPlayerPair in ActiveClientPlayerDictionary)
        //{
        //    if (clientPlayerPair.Value.CurrentNode == roomNode && clientPlayerPair.Key != activeTurnClient)
        //    {
        //        numberOfOtherPlayers++;
        //        otherPlayerIDs.Add(clientPlayerPair.Key.PlayerID);
        //    }
        //}
        //byte numberOfOtherPlayersByte = System.Convert.ToByte(numberOfOtherPlayers);

        var roomInfoMessage = new RoomInfoMessage {
            MoveDirections = (byte)openDirection,
            TreasureInRoom = (ushort)treasureAmt,
            ContainsMonster = monster,
            ContainsExit = exit,
            NumberOfOtherPlayers = 0,
            OtherPlayerIDs = otherPlayerIDs
        };
        server.SendMessage(roomInfoMessage, activeTurnClient.Connection);
    }

    /// <summary>
    /// When a player enters a room, send this to all clients in the same room.
    /// </summary>
    private void SendPlayerEnterRoom(Client receivingClient, Client enteringClient)
    {
        var message = new PlayerEnterRoomMessage() {
            PlayerID = enteringClient.PlayerID
        };
        server.SendMessage(message, receivingClient.Connection);
    }

    /// <summary>
    /// When a player leaves a room, send this to all clients in the room.
    /// </summary>
    private void SendPlayerLeaveRoom(Client receivingClient, Client leavingClient)
    {
        var message = new PlayerLeaveRoomMessage() {
            PlayerID = leavingClient.PlayerID
        };
        server.SendMessage(message, receivingClient.Connection);
    }

    /// <summary>
    /// Send to the client how much treasure he obtained.
    /// </summary>
    private void SendObtainTreasure(int amt)
    {
        Client activeClient;
        ActivePlayerClientDictionary.TryGetValue(currentActivePlayer, out activeClient);
        var message = new ObtainTreasureMessage {
            Amount = (ushort)amt
        };
        server.SendMessage(message, activeClient.Connection);
    }

    /// <summary>
    /// Send to all clients when a monster gets hit.
    /// </summary>
    private void SendHitMonster(int dmg)
    {
        Client activeClient;
        ActivePlayerClientDictionary.TryGetValue(currentActivePlayer, out activeClient);
        var message = new HitMonsterMessage() {
            PlayerID = activeClient.PlayerID,
            DamageDealt = (ushort)dmg
        };

        foreach (Client c in clientList)
        {
            server.SendMessage(message, c.Connection);
        }
    }

    /// <summary>
    /// Send to all clients when a player gets hit by a monster.
    /// </summary>
    private void SendHitByMonster(int newHP)
    {
        Client activeClient;
        ActivePlayerClientDictionary.TryGetValue(currentActivePlayer, out activeClient);
        var message = new HitByMonsterMessage() {
            PlayerID = activeClient.PlayerID,
            NewHP = (ushort)newHP
        };
        foreach (Client c in clientList)
        {
            server.SendMessage(message, c.Connection);
        }
    }

    /// <summary>
    /// Send to all clients when a player defends and HP heals.
    /// </summary>
    private void SendPlayerDefends()
    {
        Client activeClient;
        ActivePlayerClientDictionary.TryGetValue(currentActivePlayer, out activeClient);
        var message = new PlayerDefendsMessage() { 
            PlayerID = activeClient.PlayerID, 
            NewHP = (ushort)currentActivePlayer.CurrentHP 
        };
        foreach (Client c in clientList)
        {
            server.SendMessage(message, c.Connection);
        }
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
        // remove client from the 2 active clients dictionary
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
        Client movingClient;
        ActivePlayerClientDictionary.TryGetValue(currentActivePlayer, out movingClient);

        // if the player was in a room with others, then send player leave room
        foreach (KeyValuePair<Client, Player> clientPlayerPair in ActiveClientPlayerDictionary)
        {
            if (clientPlayerPair.Value.CurrentNode == currentActivePlayer.CurrentNode) // his 'old' current node before getting the new one
            {
                if (clientPlayerPair.Value == currentActivePlayer) // don't send to ourselves
                    continue;
                SendPlayerLeaveRoom(clientPlayerPair.Key, movingClient);
            }
        }

        // Get the new room
        Node newRoomNode = grid.GetSpecificNeighbourNode(currentActivePlayer.CurrentNode, (Wall)moveRequestMessage.Direction);
        currentActivePlayer.CurrentNode = newRoomNode;
        

        // If player enters a room with others, send player enter room
        foreach(KeyValuePair<Client, Player> keyValuePair in ActiveClientPlayerDictionary) // each active client
        {
            if (keyValuePair.Value.CurrentNode == currentActivePlayer.CurrentNode) // if the node is the same
            {
                if (keyValuePair.Value == currentActivePlayer) // don't send to ourselves 
                    continue;
                SendPlayerEnterRoom(keyValuePair.Key, movingClient);
            }
        }

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

        if (hitPlayerFirst) // Player hits first
        {
            // Monster takes damage
            int monsterDmg = currentActivePlayer.DamageAmount + Random.Range(-variance, variance);
            monster.TakeDamage(monsterDmg);
            SendHitMonster(monsterDmg);

            if (monster.Dead) // if the monster is dead, then next turn
            {
                monsterNode.Monster = false;
                //TurnExecution();
            }
            else // Player gets attacked and may die
            {
                currentActivePlayer.TakeDamage(monster.DamageAmount + Random.Range(-variance, variance));
                SendHitByMonster(currentActivePlayer.CurrentHP);
                if (currentActivePlayer.Dead)
                {
                    // send dead message, do other stuff
                    throw new System.NotImplementedException();
                }
            }           
        }
        else // Monster hits first
        {
            currentActivePlayer.TakeDamage(monster.DamageAmount + Random.Range(-variance, variance));
            SendHitByMonster(currentActivePlayer.CurrentHP);
            if (currentActivePlayer.Dead) // if the player dies, then monster can't attack, go to next turn
            {
                // send dead message, do other stuff
                throw new System.NotImplementedException();
            }

            int monsterDmg = currentActivePlayer.DamageAmount + Random.Range(-variance, variance);
            monster.TakeDamage(monsterDmg);
            SendHitMonster(monsterDmg);
            if (monster.Dead) // if dead, remove the monster from the node
            {
                monsterNode.Monster = false;
            }
        }

        // Everything done? Next turn.
        TurnExecution();
    }

    public void HandleDefendRequest(MessageConnection messageConnection)
    {
        currentActivePlayer.Heal(Random.Range(minHealAmt, maxHealAmt));
        SendPlayerDefends();
        TurnExecution();
    }

    public void HandleClaimTreasureRequest(MessageConnection messageConnection)
    {
        Node treasureNode = grid.GetSpecificNodeInstance(currentActivePlayer.CurrentNode);
        currentActivePlayer.TreasureAmount += treasureNode.TreasureAmount;
        SendObtainTreasure(treasureNode.TreasureAmount);
        treasureNode.Treasure = false;
        TurnExecution();
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
