using UnityEngine;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Assets.Code;
using Unity.Jobs;

namespace KernDev.NetworkBehaviour
{
    public class ClientBehaviour : MonoBehaviour
    {
        private NetworkDriver networkDriver;
        private NetworkConnection connection;

        private JobHandle networkJobHandle;

        private Queue<MessageConnection> messagesQueue;
        public MessageEvent[] ClientCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count];
        private bool disconnectedMessage = false;

        private LobbyManager lobbyManager;
        public LobbyManager LobbyManager {
            get { return lobbyManager; }
            set {
                lobbyManager = value;
                AddLobbyEventListeners(); }
        }

        private ClientGameManager clientGameManager;
        public ClientGameManager ClientGameManager {
            get { return clientGameManager; }
            set {
                clientGameManager = value;
                AddGameEventListeners();
            }
        }

        void Start()
        {
            // create the network connection
            networkDriver = NetworkDriver.Create();
            connection = default;

            var endpoint = NetworkEndPoint.LoopbackIpv4;
            endpoint.Port = 9000;
            connection = networkDriver.Connect(endpoint);

            // Instatiate & Finds
            messagesQueue = new Queue<MessageConnection>();
            for (int i = 0; i < ClientCallbacks.Length; i++)
            {
                ClientCallbacks[i] = new MessageEvent(); // Instantiate all Message Type events.
            }

            LobbyManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<LobbyManager>();
        }

        void Update()
        {
            networkJobHandle.Complete();

            if (!connection.IsCreated)
            {
                Debug.Log("Can't create connection.");
                if (clientGameManager != null && !disconnectedMessage)
                {
                    StartCoroutine(clientGameManager.ShowConnectionDisconnected());
                    disconnectedMessage = true;
                }   
                return;
            }

            DataStreamReader reader;
            NetworkEvent.Type cmd;
            while ((cmd = connection.PopEvent(networkDriver, out reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect) // Connecting to the server
                {
                    Debug.Log("Connected to server");
                }
                else if (cmd == NetworkEvent.Type.Data) // Getting in Data
                {
                    var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                    switch (messageType) // client reads all except setname. 
                    {
                        #region Lobby Protocol
                        case MessageHeader.MessageType.None:
                            break;
                        case MessageHeader.MessageType.NewPlayer:
                            var newPlayerMessage = new NewPlayerMessage();
                            newPlayerMessage.DeserializeObject(ref reader);
                            MessageConnection mcNewPlayer = new MessageConnection(connection, newPlayerMessage);
                            messagesQueue.Enqueue(mcNewPlayer);
                            break;
                        case MessageHeader.MessageType.Welcome:
                            var welcomeMessage = new WelcomeMessage();
                            welcomeMessage.DeserializeObject(ref reader);
                            MessageConnection mcWelcome = new MessageConnection(connection, welcomeMessage);
                            messagesQueue.Enqueue(mcWelcome);

                            break;
                        case MessageHeader.MessageType.SetName:
                            break;
                        case MessageHeader.MessageType.RequestDenied:
                            var requestDeniedMessage = new RequestDeniedMessage();
                            requestDeniedMessage.DeserializeObject(ref reader);
                            MessageConnection mcDenied = new MessageConnection(connection, requestDeniedMessage);
                            messagesQueue.Enqueue(mcDenied);
                            break;
                        case MessageHeader.MessageType.PlayerLeft:
                            var playerLeftMessage = new PlayerLeftMessage();
                            playerLeftMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerLeft = new MessageConnection(connection, playerLeftMessage);
                            messagesQueue.Enqueue(mcPlayerLeft);
                            break;
                        case MessageHeader.MessageType.StartGame:
                            var startGameMessage = new StartGameMessage();
                            startGameMessage.DeserializeObject(ref reader);
                            MessageConnection mcStartGame = new MessageConnection(connection, startGameMessage);
                            messagesQueue.Enqueue(mcStartGame);
                            break;
                        #endregion
                        #region Game Protocol
                        case MessageHeader.MessageType.PlayerTurn:
                            var playerTurnMessage = new PlayerTurnMessage();
                            playerTurnMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerTurn = new MessageConnection(connection, playerTurnMessage);
                            messagesQueue.Enqueue(mcPlayerTurn);
                            break;
                        case MessageHeader.MessageType.RoomInfo:
                            var roomInfoMessage = new RoomInfoMessage();
                            roomInfoMessage.DeserializeObject(ref reader);
                            MessageConnection mcRoomInfo = new MessageConnection(connection, roomInfoMessage);
                            messagesQueue.Enqueue(mcRoomInfo);
                            break;
                        case MessageHeader.MessageType.PlayerEnterRoom:
                            var playerEnterRoomMessage = new PlayerEnterRoomMessage();
                            playerEnterRoomMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerEnterRoom = new MessageConnection(connection, playerEnterRoomMessage);
                            messagesQueue.Enqueue(mcPlayerEnterRoom);
                            break;
                        case MessageHeader.MessageType.PlayerLeaveRoom:
                            var playerLeaveRoomMessage = new PlayerLeaveRoomMessage();
                            playerLeaveRoomMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerLeaveRoom = new MessageConnection(connection, playerLeaveRoomMessage);
                            messagesQueue.Enqueue(mcPlayerLeaveRoom);
                            break;
                        case MessageHeader.MessageType.ObtainTreasure:
                            var obtainTreasureMessage = new ObtainTreasureMessage();
                            obtainTreasureMessage.DeserializeObject(ref reader);
                            MessageConnection mcObtainTreasure = new MessageConnection(connection, obtainTreasureMessage);
                            messagesQueue.Enqueue(mcObtainTreasure);
                            break;
                        case MessageHeader.MessageType.HitMonster:
                            var hitMonsterMessage = new HitMonsterMessage();
                            hitMonsterMessage.DeserializeObject(ref reader);
                            MessageConnection mcHitMonster = new MessageConnection(connection, hitMonsterMessage);
                            messagesQueue.Enqueue(mcHitMonster);
                            break;
                        case MessageHeader.MessageType.HitByMonster:
                            var hitByMonsterMessage = new HitByMonsterMessage();
                            hitByMonsterMessage.DeserializeObject(ref reader);
                            MessageConnection mcHitByMonster = new MessageConnection(connection, hitByMonsterMessage);
                            messagesQueue.Enqueue(mcHitByMonster);
                            break;
                        case MessageHeader.MessageType.PlayerDefends:
                            var playerDefendsMessage = new PlayerDefendsMessage();
                            playerDefendsMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerDefends = new MessageConnection(connection, playerDefendsMessage);
                            messagesQueue.Enqueue(mcPlayerDefends);
                            break;
                        case MessageHeader.MessageType.PlayerLeftDungeon:
                            var playerLeftDungeonMessage = new PlayerLeftDungeonMessage();
                            playerLeftDungeonMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerLeftDungeon = new MessageConnection(connection, playerLeftDungeonMessage);
                            messagesQueue.Enqueue(mcPlayerLeftDungeon);
                            break;
                        case MessageHeader.MessageType.PlayerDies:
                            var playerDiesMessage = new PlayerDiesMessage();
                            playerDiesMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerDies = new MessageConnection(connection, playerDiesMessage);
                            messagesQueue.Enqueue(mcPlayerDies);
                            break;
                        case MessageHeader.MessageType.EndGame:
                            var endGameMessage = new EndGameMessage();
                            endGameMessage.DeserializeObject(ref reader);
                            MessageConnection mcEndGame = new MessageConnection(connection, endGameMessage);
                            messagesQueue.Enqueue(mcEndGame);
                            break;
                        case MessageHeader.MessageType.MoveRequest:
                            break;
                        case MessageHeader.MessageType.AttackRequest:
                            break;
                        case MessageHeader.MessageType.DefendRequest:
                            break;
                        case MessageHeader.MessageType.ClaimTreasureRequest:
                            break;
                        case MessageHeader.MessageType.LeaveDungeonRequest:
                            break;
                        #endregion
                        case MessageHeader.MessageType.Count:
                            break;
                        default:
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Disconnected from server");
                    connection = default;
                    LobbyManager.ShowHostTerminatedRoom();
                }
            }

            networkJobHandle = networkDriver.ScheduleUpdate();
            networkJobHandle.Complete();
            ProcessMessagesQueue();
        }

        private void ProcessMessagesQueue()
        {
            while (messagesQueue.Count > 0)
            {
                var message = messagesQueue.Dequeue(); // get the message that came in first.
                ClientCallbacks[(int)message.messageHeader.Type].Invoke(message); // get the type of that message, invoke the event of that type and sent with it the message.
            }
        }

        public void SendMessage(MessageHeader message)
        {
            if (connection != default)
            {
                var writer = networkDriver.BeginSend(connection);
                message.SerializeObject(ref writer);
                networkDriver.EndSend(writer);
            }

        }

        private void AddLobbyEventListeners()
        {
            ClientCallbacks[(int)MessageHeader.MessageType.NewPlayer].AddListener(LobbyManager.ShowNewPlayerMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.Welcome].AddListener(LobbyManager.ShowWelcomeMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.PlayerLeft].AddListener(LobbyManager.ShowPlayerLeftMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.RequestDenied].AddListener(LobbyManager.ShowRequestDeniedMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.StartGame].AddListener(LobbyManager.ShowStartGame);
        }

        public void AddGameEventListeners()
        {
            ClientCallbacks[(int)MessageHeader.MessageType.PlayerTurn].AddListener(ClientGameManager.ShowPlayerTurnMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.RoomInfo].AddListener(ClientGameManager.ShowRoomInfoMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.HitMonster].AddListener(ClientGameManager.ShowHitMonsterMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.HitByMonster].AddListener(ClientGameManager.ShowHitByMonsterMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.PlayerDefends].AddListener(ClientGameManager.ShowPlayerDefendsMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.ObtainTreasure].AddListener(ClientGameManager.ShowObtainTreasureMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.PlayerEnterRoom].AddListener(ClientGameManager.ShowPlayerEnterRoomMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.PlayerLeaveRoom].AddListener(ClientGameManager.ShowPlayerLeaveRoomMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.PlayerLeftDungeon].AddListener(ClientGameManager.ShowPlayerLeftDungeonMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.EndGame].AddListener(ClientGameManager.ShowEndGameMessage);
            ClientCallbacks[(int)MessageHeader.MessageType.PlayerDies].AddListener(ClientGameManager.ShowPlayerDiesMessage);

        }

        public void Disconnect()
        {
            networkDriver.Disconnect(connection);
        }


        private void OnDestroy()
        {
            networkJobHandle.Complete();
            networkDriver.Dispose();
        }
    }
}
