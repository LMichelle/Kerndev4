using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using Assets.Code;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;

public class ServerBehaviour : MonoBehaviour
{
    #region Variables
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;

    private Queue<MessageConnection> messagesQueue;

    public List<Client> clientList = new List<Client>();
    public int deniedMessageID = 0;

    public MessageEvent[] ServerCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count];
    // these are all 7 message type events. Every Message Type has its own event that it listens to. 
    // e.g. the setname message type is ServerCallbacks[(int)MessageHeader.MessageType.SetName)]. I can add functions to this event that need to be
    // invoked when I receive a message of the setname type.
    #endregion

    void Start()
    {
        // Create network connection
        networkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        
        if(networkDriver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind port");
        }
        else
        {
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        // Initialize MessageQueue and All ServerCallback Events
        messagesQueue = new Queue<MessageConnection>();
        for (int i = 0; i < ServerCallbacks.Length; i++)
        {
            ServerCallbacks[i] = new MessageEvent(); // Instantiate all Message Type events.
        }

        // Add event listeners
        ServerCallbacks[(int)MessageHeader.MessageType.SetName].AddListener(HandleSetName); // this is how we would add a function to the Setname event. 
        ServerCallbacks[(int)MessageHeader.MessageType.PlayerLeft].AddListener(HandlePlayerLeft);
        ServerCallbacks[(int)MessageHeader.MessageType.StartGame].AddListener(HandleStartGame);
        ServerCallbacks[(int)MessageHeader.MessageType.None].AddListener(HandleNone); // the optional one to keep the connection going in inactivity

    }

    void Update()
    {
        networkJobHandle.Complete();

        // Clean up connections
        for(int i = 0; i < connections.Length; ++i)
        {
            if(!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept connections
        NetworkConnection connection;
        while((connection = networkDriver.Accept()) != default)
        {
            // We can only accept 4 connections
            if (clientList.Count >= 4)
            {
                // Send Request Denied Message
                SendRequestDeniedMessage(connection);
            }
            else
            {
                connections.Add(connection);
                Debug.Log("Accepted connection");

                // Log the client & Send the Welcome Message
                bool host = false;
                if (connection.InternalId == 0)
                    host = true;
                Client newClient = new Client(connection.InternalId, "", connection, host);
                clientList.Add(newClient);
                SendWelcomeMessage(newClient);
            }
            
        }

        // Read the stream
        DataStreamReader reader;
        for(int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty)
            {
                if(cmd == NetworkEvent.Type.Data)
                {
                    var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                    switch (messageType)
                    {
                        case MessageHeader.MessageType.None:
                            var noneMessage = new NoneMessage();
                            noneMessage.DeserializeObject(ref reader);
                            MessageConnection mcNone = new MessageConnection(connections[i], noneMessage);
                            messagesQueue.Enqueue(mcNone);
                            break;
                        case MessageHeader.MessageType.NewPlayer:
                            break;
                        case MessageHeader.MessageType.Welcome:
                            break;
                        case MessageHeader.MessageType.SetName:  
                            var setNameMessage = new SetNameMessage();
                            setNameMessage.DeserializeObject(ref reader);
                            MessageConnection mcSetName = new MessageConnection(connections[i], setNameMessage);
                            messagesQueue.Enqueue(mcSetName);
                            break;
                        case MessageHeader.MessageType.RequestDenied:
                            break;
                        case MessageHeader.MessageType.PlayerLeft:
                            var playerLeftMessage = new PlayerLeftMessage();
                            playerLeftMessage.DeserializeObject(ref reader);
                            MessageConnection mcPlayerLeft = new MessageConnection(connections[i], playerLeftMessage);
                            messagesQueue.Enqueue(mcPlayerLeft);
                            break;
                        case MessageHeader.MessageType.StartGame:
                            var startGameMessage = new StartGameMessage();
                            startGameMessage.DeserializeObject(ref reader);
                            MessageConnection mcStartGame = new MessageConnection(connections[i], startGameMessage);
                            messagesQueue.Enqueue(mcStartGame);
                            break;
                        case MessageHeader.MessageType.Count:
                            break;
                        default:
                            break;
                    }
                }
                else if(cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();
        networkJobHandle.Complete();
        ProcessMessagesQueue();
    }

    private void ProcessMessagesQueue()
    {
        while(messagesQueue.Count > 0)
        {
            var message = messagesQueue.Dequeue(); // get the message that came in first.
            ServerCallbacks[(int)message.messageHeader.Type].Invoke(message); // get the type of that message, invoke the event of that type and sent with it the message.
        }
    }

    /// <summary>
    /// Send a welcome message to the new player to tell him his ID and colour.
    /// </summary>
    /// <param name="client"></param>
    private void SendWelcomeMessage(Client client)
    {
        var welcomeMessage = new WelcomeMessage {
            PlayerID = client.PlayerID,
            PlayerColour = client.PlayerColour
        };
        SendMessage(welcomeMessage, client.Connection);
    }

    private void SendRequestDeniedMessage(NetworkConnection c)
    {
        deniedMessageID++;
        var requestDeniedMessage = new RequestDeniedMessage {
            DeniedMessageID = (uint)deniedMessageID
        };
        SendMessage(requestDeniedMessage, c);
    }

    private void HandleSetName(MessageConnection message)
    {
        // first we want to set the name to the right client, and send him the info of the other players
        foreach (Client c in clientList)
        {
            if (c.Connection == message.connection) // the client who sent in the name
            {
                c.PlayerName = (message.messageHeader as SetNameMessage).Name;
                NewPlayerMessageToNew(c);
            } 
        }
        
        // then we want to send the info of the new player to the others. This can only be done after we know for sure the name has registered.
        foreach(Client c in clientList)
        {
            if (c.Connection == message.connection) // this is the new player, we don't want to send him anthing
            NewPlayerMessageToAll(c); 
        }
        

    }


    /// <summary>
    /// This function sends a message to the other players about the new player
    /// </summary>
    /// <param name="message"></param>
    private void NewPlayerMessageToAll(Client client)
    {
        // Info of the new connection
        var newPlayerMessage = new NewPlayerMessage {
            PlayerID = client.PlayerID,
            PlayerColour = client.PlayerColour,
            PlayerName = client.PlayerName
        };

       foreach(Client c in clientList)
        {
            if (c == client)
                continue;
            SendMessage(newPlayerMessage, c.Connection);
        }
        
    }
   
    /// <summary>
    /// This function sends a message to the new player containing the info of the other players.
    /// </summary>
    /// <param name="message"></param>
    private void NewPlayerMessageToNew(Client client)
    {
        // make a message about each of the existing players for the new player
        foreach (Client c in clientList)
        {
            if (c == client)
                continue;
            var newPlayerMessage = new NewPlayerMessage {
                PlayerID = c.PlayerID,
                PlayerColour = c.PlayerColour,
                PlayerName = c.PlayerName
            };
            // send the info to the new player.
            SendMessage(newPlayerMessage, client.Connection);
        }
    }

    private void HandlePlayerLeft(MessageConnection message)
    {
        foreach (Client c in clientList)
        {
            if (c.Connection == message.connection)
            {
                clientList.Remove(c);
                break;
            }
        }
        foreach (Client c in clientList)
        {
            SendMessage(message.messageHeader, c.Connection);
        }
        foreach(NetworkConnection c in connections)
        {

        }
    }

    private void HandleNone(MessageConnection message)
    {
        foreach(Client c in clientList)
        {
            SendMessage(message.messageHeader, c.Connection);
        }
    }

    private void HandleStartGame(MessageConnection message)
    {
        foreach (Client c in clientList)
        {
            SendMessage(message.messageHeader, c.Connection);
        }
    }

    private void SendMessage(MessageHeader message, NetworkConnection connection)
    {
        if (connection != default)
        {
            var writer = networkDriver.BeginSend(connection);
            message.SerializeObject(ref writer);
            networkDriver.EndSend(writer);
        }
    }

    public void Disconnect()
    {
        foreach (Client c in clientList)
        {
            networkDriver.Disconnect(c.Connection);
        }
    }

    private void OnDestroy()
    {
        networkJobHandle.Complete();
        networkDriver.Dispose();
        connections.Dispose();
    }
}
