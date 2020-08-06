using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KernDev.NetworkBehaviour;
using System.Linq;

namespace Assets.Code
{
    public class LobbyManager : MonoBehaviour
    {
        private Text outputMessagesText;
        public InputField nameInputField;
        public InputField ipInputField;
        public GameObject lobbyUIGO, hostUIGO, joinUIGO, hostOptionsGO, joinOptionsGO;
        public Text hostMessagesText, joinMessagesText;
        
        [SerializeField]
        private Text outputLogsText;

        [SerializeField]
        private int lineSpacing = 35;

        [SerializeField]
        private Button hostGameButton, joinGameButton;

        [SerializeField]
        private int minPlayerHP = 30, maxPlayerHP = 50;

        private ClientBehaviour clientBehaviour;

        private Client thisClient;
        private List<Client> allClientsList = new List<Client>();
        public List<Client> AllClientsList { 
            get { return allClientsList; } 
            private set { allClientsList = value; } 
        }

        private void Start()
        {
            StartCoroutine(SendKeepConnection());
        }

        private void Update()
        {
            // Set Host and Join buttons to interactable if name has been typed
            if (nameInputField.text == null || nameInputField.text == "")
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
            if (ipInputField.text != "")
            {
                // validize the IP adress
                bool valid = ValidateIPv4(ipInputField.text);
                // if not valid, return.
                if (!valid)
                {
                    outputLogsText.text = "The IP Adress is invalid.";
                    return;
                }
            }

            // Add the Server
            GameObject server = new GameObject();
            server.AddComponent<ServerBehaviour>().ServerStart(ipInputField.text);
            server.name = "Server";
            server.tag = "Server";
            DontDestroyOnLoad(server);
            outputLogsText.text += "The room is created.";

            // Add the Client
            GameObject client = new GameObject();
            clientBehaviour = client.AddComponent<ClientBehaviour>();
            clientBehaviour.ClientStart(ipInputField.text);
            client.name = "Client";
            client.tag = "Client";

            // Update the UI
            lobbyUIGO.SetActive(true);
            hostUIGO.SetActive(true);
            gameObject.GetComponent<LobbyManager>().SetOutputText(hostMessagesText);
            hostOptionsGO.SetActive(true);
            
        }

        public void JoinGame()
        {
            if (ipInputField.text != "")
            {
                // validize the IP adress
                bool valid = ValidateIPv4(ipInputField.text);
                // if not valid, return.
                if (!valid)
                {
                    outputLogsText.text = "The IP Adress is invalid.";
                    return;
                }
            }

            GameObject client = new GameObject();
            clientBehaviour = client.AddComponent<ClientBehaviour>();
            clientBehaviour.ClientStart(ipInputField.text);
            client.name = "Client";
            client.tag = "Client";
            outputLogsText.text += "Joined the game.";

            // Update the UI
            lobbyUIGO.SetActive(true);
            joinUIGO.SetActive(true);
            gameObject.GetComponent<LobbyManager>().SetOutputText(joinMessagesText);
            joinOptionsGO.SetActive(true);
        }

        // Show Messages -------------------------------------------------------------
        public void ShowNewPlayerMessage(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as NewPlayerMessage);
            
            // Get colour and convert
            uint playerColour = message.PlayerColour;
            Color32 color32 = new Color32();
            color32 = ColorExtensions.FromUInt(color32, playerColour);
            string hexColor = ColorExtensions.colorToHex(color32);

            // Display the message
            SetMessagesText($"<color=#{hexColor}>Player {message.PlayerID}, {message.PlayerName} has joined the game!</color>");
            
            // Add to the clientList
            Client otherClient = new Client(message.PlayerID, message.PlayerName, default, false);
            otherClient.PlayerColour = message.PlayerColour;
            AllClientsList.Add(otherClient);
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
            SetMessagesText($"<color=#{hexColor}> Welcome! Your player ID is {message.PlayerID}.</color>");
            bool host = false;
            if (message.PlayerID == 0)
                host = true;

            // Have this clients info ready for the gamemanager.
            thisClient = new Client(message.PlayerID, "", messageConnection.connection, host);
            thisClient.PlayerColour = message.PlayerColour;
            AllClientsList.Add(thisClient);

            SendSetNameMessage();
        }

        public void ShowRequestDeniedMessage(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as RequestDeniedMessage);
            uint deniedID = message.DeniedMessageID;
            SetMessagesText($"<color=#E7D0D7>Your request has been denied. Denied Message ID: {deniedID}</color>");
        } 

        public void ShowPlayerLeftMessage(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as PlayerLeftMessage);
            SetMessagesText($"Player {message.PlayerLeftID} has left the room.");
            
            // Remove the client that left from the All Clients List
            foreach (Client c in AllClientsList)
            {
                if (c.PlayerID == message.PlayerLeftID)
                {
                    AllClientsList.Remove(c);
                }
            }
        }

        public void ShowStartGame(MessageConnection messageConnection)
        {
            var message = (messageConnection.messageHeader as StartGameMessage);
            thisClient.StartHP = message.StartHP;
            SetMessagesText($"The Game is starting in");
            StartCoroutine(StartGameCountDown());
        }

        // Send Messages -------------------------------------------------------------
        public void SendSetNameMessage()
        {
            string name = nameInputField.text;
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
            var startGameMessage = new StartGameMessage { 
                StartHP = (ushort)Random.Range(minPlayerHP, maxPlayerHP) 
            };
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
                for (int i = 3; i > 0; i--)
                {
                    SetMessagesText($"{i}");
                    yield return new WaitForSeconds(1f);
                }
                countdown = false;
                StartGame();
            }
        }

        public void ShowHostTerminatedRoom()
        {
            SetMessagesText("The host terminated the room.");
        }

        private void StartGame()
        {
            if (thisClient.Host)
            { 
                gameObject.GetComponent<HostGameManager>().enabled = true;
            }
            gameObject.GetComponent<ClientGameManager>().enabled = true;
            gameObject.GetComponent<ClientGameManager>().ThisClient = thisClient;
            gameObject.GetComponent<ClientGameManager>().AllClientsList = AllClientsList;
        }

        /// <summary>
        /// Used on the buttons to get the client's messages text GO or the host's.
        /// </summary>
        /// <param name="text"></param>
        public void SetOutputText(Text text)
        {
            outputMessagesText = text;
        }

        private void SetMessagesText(string text)
        {
            outputMessagesText.text += text + "\n";
            outputMessagesText.rectTransform.sizeDelta = new Vector2(outputMessagesText.rectTransform.sizeDelta.x, outputMessagesText.rectTransform.sizeDelta.y + lineSpacing);
        }

        public bool ValidateIPv4(string ipString)
        {
            if (System.String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
    }
}
