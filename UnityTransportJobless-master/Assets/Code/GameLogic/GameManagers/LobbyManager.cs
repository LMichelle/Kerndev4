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
        [SerializeField]
        private Text outputMessagesText;
        public InputField usernameInputField;
        public InputField passwordInputField;
        public InputField nameInputField;
        public InputField ipInputField;
        public GameObject lobbyUIGO, hostUIGO, joinUIGO, hostOptionsGO, joinOptionsGO, loginUIGO;
        public Text hostMessagesText, joinMessagesText;
        private UserData myData;
        
        [SerializeField]
        private Text outputLogsText;


        [SerializeField]
        private Button hostGameButton, joinGameButton;


        private ClientBehaviour clientBehaviour;

        public Client thisClient;
        private List<Client> allClientsList = new List<Client>();
        public List<Client> AllClientsList { 
            get { return allClientsList; } 
            private set { allClientsList = value; } 
        }

        private void Start()
        {
            StartCoroutine(SendKeepConnection());
            StartCoroutine(DataBaseStart());
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

        public void Login()
        {
            StartCoroutine(CheckData());
        }

        /// <summary>
        /// Logs in to the database and creates a session.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DataBaseStart()
        {
            yield return StartCoroutine(DatabaseManager.GetHTTP("ServerLogin.php?id=" + DatabaseManager.serverID + "&Password=" + DatabaseManager.serverPassword));
            DatabaseManager.sessionID = DatabaseManager.response;
        }

        /// <summary>
        /// Checks if the player login is valid and if so, continues to the game.
        /// </summary>
        /// <returns></returns>
        public IEnumerator CheckData()
        {
            yield return StartCoroutine(DatabaseManager.GetHTTP($"PlayerLoginServer.php?PHPSESSID={DatabaseManager.sessionID}&Email={usernameInputField.text}&Password={passwordInputField.text}"));
            if(DatabaseManager.response == "Invalid e-mail address.<br />") {
                outputLogsText.text = "Email adress is invalid.";
                yield break;
            }
            else if (DatabaseManager.response == "nullError (You have an error in your SQL syntax; check the manual that corresponds to your MariaDB server version for the right syntax to use near '' at line 1) 1064")
            {
                outputLogsText.text = "Password is invalid.";
                yield break;
            }
            else if (DatabaseManager.response != 0 + "")
            {
                myData = JsonUtility.FromJson<UserData>(DatabaseManager.response);
                loginUIGO.SetActive(false);
            }
            else
            {
                outputLogsText.text = "Something went wrong. Please try again later.";
            }
        }

        /// <summary>
        /// Adds both a Serverbehaviour and a Clientbehaviour.
        /// </summary>
        public void HostGame() 
        {  
            if (ipInputField.text != "")
            {
                // validate the IP adress
                bool valid = ValidateIPv4(ipInputField.text);
                // if not valid, return.
                if (!valid)
                {
                    outputLogsText.text = "The IP Adress is invalid.";
                    return;
                }
            }

            // Add the Server Behaviour
            GameObject server = new GameObject();
            server.AddComponent<ServerBehaviour>().ServerStart(ipInputField.text);
            server.name = "Server";
            server.tag = "Server";
            DontDestroyOnLoad(server);
            outputLogsText.text += "The room is created.";

            // Enable the HostGameManager
            gameObject.GetComponent<HostGameManager>().enabled = true;

            // Add the Client Behaviour
            GameObject client = new GameObject();
            clientBehaviour = client.AddComponent<ClientBehaviour>();      
            client.name = "Client";
            client.tag = "Client";
            gameObject.GetComponent<ClientGameManager>().enabled = true;
            gameObject.GetComponent<ClientGameManager>().SetOutputText(hostMessagesText);
            gameObject.GetComponent<ClientGameManager>().StartClientGameManager();

            clientBehaviour.ClientStart(ipInputField.text);
            

            // Update the UI to go to the Lobby
            lobbyUIGO.SetActive(true);
            hostUIGO.SetActive(true);
            gameObject.GetComponent<LobbyManager>().SetOutputText(hostMessagesText);
            hostOptionsGO.SetActive(true);
            
        }

        public void JoinGame()
        {
            if (ipInputField.text != "")
            {
                // validate the IP adress
                bool valid = ValidateIPv4(ipInputField.text);
                // if not valid, return.
                if (!valid)
                {
                    outputLogsText.text = "The IP Adress is invalid.";
                    return;
                }
            }
            
            // Add the Client Behaviour
            GameObject client = new GameObject();
            clientBehaviour = client.AddComponent<ClientBehaviour>(); 
            client.tag = "Client";
            client.name = "Client";

            gameObject.GetComponent<ClientGameManager>().enabled = true;
            gameObject.GetComponent<ClientGameManager>().SetOutputText(joinMessagesText);
            gameObject.GetComponent<ClientGameManager>().StartClientGameManager();
            
            clientBehaviour.ClientStart(ipInputField.text);
            
            outputLogsText.text += "Joined the game.";
            
            // Update the UI
            lobbyUIGO.SetActive(true);
            joinUIGO.SetActive(true);
            gameObject.GetComponent<LobbyManager>().SetOutputText(joinMessagesText);
            joinOptionsGO.SetActive(true);
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

        /// <summary>
        /// Used to determine the client's messages text GO or the host's.
        /// </summary>
        /// <param name="text"></param>
        public void SetOutputText(Text text)
        {
            outputMessagesText = text;
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
