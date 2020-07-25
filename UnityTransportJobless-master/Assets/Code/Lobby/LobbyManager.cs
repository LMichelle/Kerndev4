using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code
{
    public class LobbyManager : MonoBehaviour
    {
        private Text outputMessagesText;
        public InputField inputField;
        private ClientBehaviour clientBehaviour;

        [SerializeField]
        private Text outputLogsText;

        [SerializeField]
        private Button hostGameButton, joinGameButton;

        private Client thisClient;

        private void Start()
        {
            StartCoroutine(SendKeepConnection());
        }

        private void Update()
        {
            // Set Host and Join buttons to interactable if name has been typed
            if (inputField.text == null || inputField.text == "")
            {
                hostGameButton.interactable = false;
                joinGameButton.interactable = false;
            }
            else
            {
                hostGameButton.interactable = true;
                joinGameButton.interactable = true;
            }

        }

        public void HostGame() // when hosting the game, add both a server and client.
        {
            GameObject server = new GameObject();
            server.AddComponent<ServerBehaviour>();
            server.name = "Server";
            server.tag = "Server";
            DontDestroyOnLoad(server);
            outputLogsText.text += "The room is created.";
            GameObject client = new GameObject();
            clientBehaviour = client.AddComponent<ClientBehaviour>();
            client.name = "Client";
        }

        public void JoinGame()
        {
            GameObject client = new GameObject();
            clientBehaviour = client.AddComponent<ClientBehaviour>();
            client.name = "Client";
            outputLogsText.text += "Joined the game.";
        }

        // Show Messages -------------------------------------------------------------
        public void ShowNewPlayerMessage(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as NewPlayerMessage);
            uint playerColour = message.PlayerColour;
            Color32 color32 = new Color32();
            color32 = ColorExtensions.FromUInt(color32, playerColour);
            string hexColor = ColorExtensions.colorToHex(color32);
            outputMessagesText.text += $"<color=#{hexColor}>Player {message.PlayerID}, {message.PlayerName} has joined the game!</color>\n";
        }

        /// <summary>
        /// Show the Welcome Message and send the set name message.
        /// </summary>
        /// <param name="messageConnection"></param>
        public void ShowWelcomeMessage(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as WelcomeMessage);
            uint playerColour = message.PlayerColour;
            Color32 color32 = new Color32();
            color32 = ColorExtensions.FromUInt(color32, playerColour);
            string hexColor = ColorExtensions.colorToHex(color32);
            outputMessagesText.text += $"<color=#{hexColor}> Welcome! Your player ID is {message.PlayerID}.</color>\n";
            bool host = false;
            if (message.PlayerID == 0)
                host = true;

            // Have this clients info ready for the gamemanager.
            thisClient = new Client(message.PlayerID, "", messageConnection.connection, host);
            thisClient.PlayerColour = message.PlayerColour;

            SendSetNameMessage();
        }

        public void ShowRequestDeniedMessage(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as RequestDeniedMessage);
            uint deniedID = message.DeniedMessageID;
            outputMessagesText.text += $"<color=#E7D0D7>Your request has been denied. Denied Message ID: {deniedID}</color>\n";
        } 

        public void ShowPlayerLeftMessage(MessageConnection messageConnection)
        {
            Debug.Log("Showing Player Left Message");
            var message = (messageConnection.messageHeader as PlayerLeftMessage);
            int playerID = message.PlayerLeftID;
            outputMessagesText.text += $"Player {playerID} has left the room.\n";
        }

        public void ShowStartGame(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as StartGameMessage);
            //message.StartHP();
            outputMessagesText.text += $"The Game is starting in \n";
            StartCoroutine(StartGameCountDown());
        }

        // Send Messages -------------------------------------------------------------
        public void SendSetNameMessage()
        {
            string name = inputField.text;
            var setNameMessage = new SetNameMessage {
                Name = name
            };      
            clientBehaviour.SendMessage(setNameMessage);
            
            thisClient.PlayerName = name;
        }

        public void SendTerminateRoom()
        {
            if(GameObject.FindGameObjectWithTag("Server") != null)
            {
                GameObject server = GameObject.FindGameObjectWithTag("Server");
                server.GetComponent<ServerBehaviour>().Disconnect();
                Destroy(clientBehaviour.gameObject, 2f);
                Destroy(server, 3f);
            }
        }

        public void SendLeaveRoom()
        {
            if (thisClient != null)
            {
                var playerLeftMessage = new PlayerLeftMessage {
                    PlayerLeftID = thisClient.PlayerID
                };
                clientBehaviour.SendMessage(playerLeftMessage);
            }
            clientBehaviour.Disconnect();
            Destroy(clientBehaviour.gameObject, 3f);
        }

        /// <summary>
        /// Starts the game and sends a message to the others
        /// </summary>
        public void SendStartGame()
        {
            var startGameMessage = new StartGameMessage { StartHP = 300 };
            clientBehaviour.SendMessage(startGameMessage);
        }

        /// <summary>
        /// To not have the server disconnect after inactivity, send a 'none' message every 10 seconds.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SendKeepConnection()
        {
            while (true)
            {
                var message = new NoneMessage { };
                if (clientBehaviour != null)
                {
                    clientBehaviour.SendMessage(message);
                }

                yield return new WaitForSeconds(10f);
            }
        }



        // Other Functions -------------------------------------------------------------
        public IEnumerator StartGameCountDown()
        {
            bool countdown = true;
            while (countdown)
            {
                //Debug.Log("Countdown");
                for (int i = 3; i > 0; i--)
                {
                    Debug.Log("counting down");
                    outputMessagesText.text += $"{i}\n";
                    yield return new WaitForSeconds(1f);
                }
                countdown = false;
                StartGame();
            }
        }
        
        public void SetOutputText(Text text)
        {
            outputMessagesText = text;
        }

        public void ShowHostTerminatedRoom()
        {
            outputMessagesText.text = "The host terminated the room.";
        }

        private void StartGame()
        {
            if (thisClient.Host)
            {
                // set the host & client gamemanager to go on
            } else
            {
                // set the client gamemanager to go on
            }
        }
    }
}
