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
    public GameObject gridPrefab;

    [SerializeField]
    private int minMonsterSpawnAmt = 1, maxMonsterSpawnAmt = 5;
    [SerializeField]
    private int minMonsterHP = 10, maxMonsterHP = 15, minMonsterDmg = 1, maxMonsterDmg = 5;
    [SerializeField]
    private int minPlayerDmg = 3, maxPlayerDmg = 7;
    [SerializeField]
    private int variance = 2;
    [SerializeField]
    private int minHealAmt = 2, maxHealAmt = 7;
    [SerializeField]
    private int minTreasureContainingAmt = 200, maxTreasureContainingAmt = 300;
    [SerializeField]
    private int minTreasureSpawnAmt = 10, maxTreasureSpawnAmt = 30;
    [SerializeField]
    private int healAttackDmgSubtraction = -3;

    private GridSystem grid;
    private ServerBehaviour server;
    private TurnManager turnManager = new TurnManager();

    private List<Client> clientList; // List of all clients
    private Dictionary<Client, Player> AllClientPlayerDictionary = new Dictionary<Client, Player>(); // All clients and their player info
    private Dictionary<Client, Player> ActiveClientPlayerDictionary = new Dictionary<Client, Player>(); // all clients/players still in the turns
    private Dictionary<Player, Client> inverseActiveDictionary = new Dictionary<Player, Client>(); // inverse dictionary
    private List<Player> playerTurnList = new List<Player>(); // The list order is the turn order
    private Player currentActivePlayer;
    private List<Opponent> monsterList = new List<Opponent>();



    private void Start()
    {
        // Instantiate grid
        Instantiate(gridPrefab);
        grid = gridPrefab.GetComponent<GridSystem>();
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
            playerTurnList.Add(player);
            player.SetStartHP(c.StartHP);
            player.DamageAmount = Random.Range(minPlayerDmg, maxPlayerDmg);
        }
        UpdateInverseDictionary();


        // spawn players, monsters and treasures.
        StartCoroutine(SpawnPlayers());
        SpawnMonsters();
        SpawnTreasures();
        SpawnDungeonExit();

        // send the turn
        turnManager.Turn = ActiveClientPlayerDictionary.Count - 1;
        turnManager.FormerAmountOfPlayers = ActiveClientPlayerDictionary.Count;
        TurnExecution();
    }

    #region Spawning
    private IEnumerator SpawnPlayers()
    {
        // if the grid isn't ready yet, then keep looping.
        if (!grid.finishedGenerating)
        {
            yield return new WaitForSeconds(.2f);
        }
        
        // Set each player to a random room/node in the maze/grid
        foreach (Player player in ActiveClientPlayerDictionary.Values)
        {
            player.CurrentNode = grid.GetRandomNode();
            UpdateInverseDictionary();
        }
        yield break;
    }

    private void SpawnMonsters()
    {
        int randomMonsterAmount = Random.Range(minMonsterSpawnAmt, maxMonsterSpawnAmt);
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
            Opponent monster = new Opponent {
                DamageAmount = Random.Range(minMonsterDmg, maxMonsterDmg),
                CurrentNode = monsterNode
            };
            
            monster.SetStartHP(Random.Range(minMonsterHP, maxMonsterHP));
            monsterList.Add(monster);
        }
    }

    private void SpawnTreasures()
    {
        int randomTreasureAmt = Random.Range(minTreasureSpawnAmt, maxTreasureSpawnAmt);
        for (int i = 0; i <= randomTreasureAmt; i++)
        {
            Node treasureNode = grid.GetRandomNode();
            if (treasureNode.Treasure)
            {
                while (treasureNode.Treasure)
                    treasureNode = grid.GetRandomNode();
            }
            treasureNode.Treasure = true;
        }
    }

    private void SpawnDungeonExit()
    {
        Node exitNode = grid.GetRandomNode();
        exitNode.DungeonExit = true;
    }
    #endregion

    private void TurnExecution()
    {
        // Send player turn if there are players left
        if (ActiveClientPlayerDictionary.Count > 0)
        {
            int turn = turnManager.NextTurn(ActiveClientPlayerDictionary.Count);
            currentActivePlayer = playerTurnList[turn];
            Client currentActiveClient;
            inverseActiveDictionary.TryGetValue(currentActivePlayer, out currentActiveClient); // make the dictionaries into a struct, tip from Geoffrey. EDIT struct should be used for value types, not reference types

            // Send Player turn
            SendPlayerTurn(currentActiveClient);

            // Send Room Info
            SendRoomInfo(currentActivePlayer.CurrentNode, currentActiveClient);
        }
        else // No players left = endgame
        {
            SendEndGame();
        }
        
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
        foreach(Client c in AllClientPlayerDictionary.Keys) // Finished players still get to see this stuff
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
            treasureAmt = Random.Range(minTreasureContainingAmt, maxTreasureContainingAmt);
            roomNode.TreasureAmount = treasureAmt;
        }
        
        //Get everyone that is in this room
        foreach (KeyValuePair<Client, Player> clientPlayerPair in ActiveClientPlayerDictionary)
        {
            if (clientPlayerPair.Value.CurrentNode == roomNode && clientPlayerPair.Key != activeTurnClient)
            {
                numberOfOtherPlayers++;
                otherPlayerIDs.Add(clientPlayerPair.Key.PlayerID);
            }
        }
        byte numberOfOtherPlayersByte = System.Convert.ToByte(numberOfOtherPlayers);

        var roomInfoMessage = new RoomInfoMessage {
            MoveDirections = (byte)openDirection,
            TreasureInRoom = (ushort)treasureAmt,
            ContainsMonster = monster,
            ContainsExit = exit,
            NumberOfOtherPlayers = numberOfOtherPlayersByte,
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
    private void SendObtainTreasure(int amt, List<Client> receivingClients)
    {
        var message = new ObtainTreasureMessage {
            Amount = (ushort)amt
        };
        foreach(Client c in receivingClients)
            server.SendMessage(message, c.Connection);
    }

    /// <summary>
    /// Send to all clients when a monster gets hit.
    /// </summary>
    private void SendHitMonster(int dmg)
    {
        Client activeClient;
        inverseActiveDictionary.TryGetValue(currentActivePlayer, out activeClient);
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
        inverseActiveDictionary.TryGetValue(currentActivePlayer, out activeClient);
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
        inverseActiveDictionary.TryGetValue(currentActivePlayer, out activeClient);
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
    private void SendPlayerLeftDungeon(Client leavingClient)
    {
        var message = new PlayerLeftDungeonMessage() {
            PlayerID = leavingClient.PlayerID
        };
        foreach(Client c in AllClientPlayerDictionary.Keys)
        {
            server.SendMessage(message, c.Connection);
        }
        
    }

    /// <summary>
    /// Send to all clients when a player dies.
    /// </summary>
    private void SendPlayerDies() 
    {
        Debug.Log("Player DED");
        Client activeClient = null;
        inverseActiveDictionary.TryGetValue(currentActivePlayer, out activeClient);

        // remove client from the 2 active clients dictionary
        ActiveClientPlayerDictionary.Remove(activeClient);
        UpdateInverseDictionary();
        playerTurnList.Remove(currentActivePlayer);

        var message = new PlayerDiesMessage() {
            PlayerID = activeClient.PlayerID
        };
        foreach(Client c in clientList)
        {
            server.SendMessage(message, c.Connection);
        }
    }

    /// <summary>
    /// Send to all clients when the game ends, with the high score in it.
    /// </summary>
    private void SendEndGame()
    {
        byte numberOfScore = System.Convert.ToByte(AllClientPlayerDictionary.Count);
        List<int> playerIDs = new List<int>();
        List<ushort> highScores = new List<ushort>();

        // Get the high scores and sort them
        foreach (KeyValuePair<Client, Player> keyValuePair in AllClientPlayerDictionary)
            highScores.Add((ushort)keyValuePair.Value.TreasureAmount);

        highScores.Sort(); // sort ascending
        highScores.Reverse(); // reverse for descending

        // get the corresponding player. If players have the same score this will just randomly decide whose the first and whose the second
        foreach(int highScore in highScores)
        {
            foreach(KeyValuePair<Client, Player> clientPlayerPair in AllClientPlayerDictionary)
            {
                if (highScore == clientPlayerPair.Value.TreasureAmount)
                    playerIDs.Add(clientPlayerPair.Key.PlayerID);

            }
        }
        
        // Send the message
        var message = new EndGameMessage() {
            NumberOfScores = numberOfScore,
            PlayerID = playerIDs,
            HighScores = highScores
        };
        
        foreach(Client c in clientList)
        {
            server.SendMessage(message, c.Connection);
        }
        
    }
    #endregion

    #region React To Requests
    public void HandleMoveRequest(MessageConnection messageConnection)
    {
        var moveRequestMessage = (messageConnection.messageHeader as MoveRequestMessage);
        Client movingClient;
        inverseActiveDictionary.TryGetValue(currentActivePlayer, out movingClient);

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
        

        // If player enters a room with others, send player enter room to the others
        foreach(KeyValuePair<Client, Player> keyValuePair in ActiveClientPlayerDictionary) // each active client
        {
            if (keyValuePair.Value.CurrentNode == currentActivePlayer.CurrentNode) // if the node is the same
            {
                if (keyValuePair.Value == currentActivePlayer) // don't send to ourselves 
                    continue; // scrap that, I want the player to know he entered a room with others in it.
                SendPlayerEnterRoom(keyValuePair.Key, movingClient);
                SendPlayerEnterRoom(movingClient, keyValuePair.Key);
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
        Opponent monster = new Opponent();
        foreach(Opponent m in monsterList)
        {
            if (m.CurrentNode == monsterNode)
                monster = m;
        }
        
        // random bool playerHitfirst, if true player hits first
        bool hitPlayerFirst = true;
        if (Random.value >= .5f)
            hitPlayerFirst = true;

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
                    SendPlayerDies();
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
                SendPlayerDies();
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
        // Get the monster
        Node monsterNode = grid.GetSpecificNodeInstance(currentActivePlayer.CurrentNode);
        Opponent monster = new Opponent();
        foreach (Opponent m in monsterList)
        {
            if (m.CurrentNode == monsterNode)
                monster = m;
        }

        // Will player or monster go first?
        bool healPlayerFirst = true;
        if (Random.value >= .5f)
            healPlayerFirst = true;

        if (healPlayerFirst)
        {
            // heal player
            currentActivePlayer.Heal(Random.Range(minHealAmt, maxHealAmt));
            SendPlayerDefends();

            // Take damage but less than usually
            currentActivePlayer.TakeDamage(monster.DamageAmount + Random.Range(-variance, variance) - healAttackDmgSubtraction);
            SendHitByMonster(currentActivePlayer.CurrentHP);
            if (currentActivePlayer.Dead)
                SendPlayerDies();
        }
        else
        {
            // Take damage but less than usually
            currentActivePlayer.TakeDamage(monster.DamageAmount + Random.Range(-variance, variance) - healAttackDmgSubtraction);
            SendHitByMonster(currentActivePlayer.CurrentHP);
            if (currentActivePlayer.Dead)
                SendPlayerDies();

            // heal player
            currentActivePlayer.Heal(Random.Range(minHealAmt, maxHealAmt));
            SendPlayerDefends();
        }
        TurnExecution();
    }

    public void HandleClaimTreasureRequest(MessageConnection messageConnection)
    {
        Node treasureNode = grid.GetSpecificNodeInstance(currentActivePlayer.CurrentNode);
        List<Client> receiveTreasureClients = new List<Client>();
        // Get all clients on this node
        foreach(KeyValuePair<Client, Player> clientPlayerPair in ActiveClientPlayerDictionary)
        {
            if (clientPlayerPair.Value.CurrentNode == treasureNode)
                receiveTreasureClients.Add(clientPlayerPair.Key);
        }
        // divide the treasure amount over the players
        int treasureAmountPerPlayer = Mathf.RoundToInt(treasureNode.TreasureAmount / receiveTreasureClients.Count);

        // Send the obtain treasure to all those clients
        foreach(Client c in receiveTreasureClients)
        {
            foreach (KeyValuePair<Client, Player> clientPlayerPair in ActiveClientPlayerDictionary)
            {
                if (clientPlayerPair.Key == c)
                {
                    clientPlayerPair.Value.TreasureAmount += treasureAmountPerPlayer;
                }
        }
        }

        SendObtainTreasure(treasureAmountPerPlayer, receiveTreasureClients);
        treasureNode.Treasure = false;
        TurnExecution();
    }

    public void HandleLeaveDungeonRequest(MessageConnection messageConnection)
    {
        var leaveDungeonRequestMessage = (messageConnection.messageHeader as LeaveDungeonRequestMessage);
        // send the player left dungeon messages

        // player is removed from the active client lists, which also shortens the turncycle
        Client removeClient = null;
        foreach (KeyValuePair<Client, Player> clientPlayerPair in ActiveClientPlayerDictionary)
        {
            if (clientPlayerPair.Key.Connection == messageConnection.connection) // the leaving client
            {
                SendPlayerLeftDungeon(clientPlayerPair.Key);
                playerTurnList.Remove(clientPlayerPair.Value); // remove from the playerturnlist
                removeClient = clientPlayerPair.Key; // I can't directly delete the client in the foreach, because that is not safe to do.
                
            }
        }
        if (removeClient != null)
        {
            ActiveClientPlayerDictionary.Remove(removeClient);
            
            UpdateInverseDictionary();
        }
        

        TurnExecution();
    }
    #endregion

    private void UpdateInverseDictionary()
    {
        inverseActiveDictionary.Clear();
        foreach (KeyValuePair<Client, Player> keyValuePair in ActiveClientPlayerDictionary)
        {
            inverseActiveDictionary.Add(keyValuePair.Value, keyValuePair.Key);
        }
    }


    
}
