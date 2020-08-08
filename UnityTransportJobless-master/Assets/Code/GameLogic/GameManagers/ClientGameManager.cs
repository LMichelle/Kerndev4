using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KernDev.NetworkBehaviour;
using KernDev.GameLogic;

/// <summary>
/// Script on all clients that manages the clients UI & Requests.
/// </summary>
public class ClientGameManager : MonoBehaviour
{
    [SerializeField]
    private float lineSpacing = 35;
    [SerializeField]
    private GameObject GameUI;
    public Text messagesText;
    [SerializeField]
    private Text HPTreasureText;
    [SerializeField]
    private Button claimTreasureButton, attackButton, defendButton, exitDungeonButton;
    [SerializeField]
    private Button northButton, eastButton, southButton, westButton;
    
    [HideInInspector]
    public Client ThisClient { get; set; }
    public Player ThisPlayer { get; set; }

    public List<Client> AllClientsList { get; set; }

    private ClientBehaviour clientBehaviour;

    public void StartGame(Client thisClient, List<Client> allClients)
    {
        ThisClient = thisClient;
        AllClientsList = allClients;
        GameUI.SetActive(true);
        clientBehaviour = GameObject.FindGameObjectWithTag("Client").GetComponent<ClientBehaviour>();
        clientBehaviour.ClientGameManager = this;
        messagesText.text = "";
        DisableAllButtons();

        ThisPlayer = new Player();
        ThisPlayer.SetStartHP(ThisClient.StartHP);
        ThisPlayer.TreasureAmount = 0;
        SetHPTreasureText();

    }

    #region Show Messages
    public void ShowPlayerTurnMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as PlayerTurnMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"It's Player {message.PlayerID}'s turn!");
    }

    public void ShowRoomInfoMessage(MessageConnection messageConnection)
    {
        DisableAllButtons();
        var message = (messageConnection.messageHeader as RoomInfoMessage);
        Wall openDirections = (Wall)message.MoveDirections;
        bool monster = System.Convert.ToBoolean(message.ContainsMonster);
        int numberOfOtherPlayers = System.Convert.ToInt32(message.NumberOfOtherPlayers);
        List<int> playerIDs = message.OtherPlayerIDs;

        if (numberOfOtherPlayers > 0)
        {
            for (int i = 0; i < numberOfOtherPlayers; i++)
            {
                Color color = Color.white;
                foreach (Client c in AllClientsList)
                {
                    if (c.PlayerID == playerIDs[i]) ;
                    color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
                }
                SetMessagesText(color, $"Player {playerIDs[i]} is in this room.");
            }
        }

        if (!monster)
        {
            if (message.TreasureInRoom != 0)
            {
                claimTreasureButton.interactable = true;
                SetMessagesText(Color.yellow, "There's treasure in this room!");
            }
            if (message.ContainsExit != 0)
            {
                exitDungeonButton.interactable = true;
                SetMessagesText(Color.yellow, "You found the door to the exit!");
            }
            SetActiveDirectionButtons(openDirections);
            SetMessagesText(Color.white, $"You can go to the {openDirections}");
        }           
        else
        {
            SetAttackDefendButtons();
            SetMessagesText(Color.red, "There's a monster in this room!");   // Is there a way to show clients if a monster died though?
        }
            
    }

    public void ShowHitMonsterMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as HitMonsterMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"Player {message.PlayerID} attacked the monster with {message.DamageDealt} hitpoints!");
    }

    public void ShowHitByMonsterMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as HitByMonsterMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"Player {message.PlayerID} got attacked! His HP went down to {message.NewHP}!");
        if(message.PlayerID == ThisClient.PlayerID)
            ThisPlayer.CurrentHP = message.NewHP;
        SetHPTreasureText();
    }

    public void ShowPlayerDefendsMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as PlayerDefendsMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"Player {message.PlayerID} defended and healed! His HP went up to {message.NewHP}!");
        SetHPTreasureText();
    }

    public void ShowObtainTreasureMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as ObtainTreasureMessage);
        SetMessagesText(Color.yellow, $"You obtained {message.Amount} gold!");
        ThisPlayer.TreasureAmount += message.Amount;
        SetHPTreasureText();
    }

    public void ShowPlayerEnterRoomMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as PlayerEnterRoomMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"Player {message.PlayerID} entered the room.");
    }

    public void ShowPlayerLeaveRoomMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as PlayerLeaveRoomMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"Player {message.PlayerID} left the room.");
    }

    public void ShowPlayerLeftDungeonMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as PlayerLeftDungeonMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"Player {message.PlayerID} left the dungeon!");
    }

    public void ShowPlayerDiesMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as PlayerDiesMessage);
        Color color = Color.white;
        foreach (Client c in AllClientsList)
        {
            if (c.PlayerID == message.PlayerID)
                color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
        }
        SetMessagesText(color, $"Player {message.PlayerID} died!");
    }

    public void ShowEndGameMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as EndGameMessage);
        for (int i = 0; i < message.NumberOfScores; i++)
        {
            Color color = Color.white;
            foreach (Client c in AllClientsList)
            {
                if (c.PlayerID == message.PlayerID[i])
                    color = ColorExtensions.ColorFromUInt(color, c.PlayerColour);
            }
            SetMessagesText(color, $"Player: {message.PlayerID[i]}'s score is {message.HighScores[i]}");
        }
        StartCoroutine(Restart());
    }
    #endregion

    #region SendRequests
    public void SendMoveRequest(string directionString)
    {
        // The button has been clicked, so disable them.
        DisableAllButtons();
        SetMessagesText(Color.white, $"You went {directionString}");

        Wall moveDirection = Wall.NORTH | Wall.EAST | Wall.SOUTH | Wall.WEST;
        switch (directionString)
        {
            case "NORTH":
                moveDirection = (moveDirection & ~Wall.EAST);
                moveDirection = (moveDirection & ~Wall.SOUTH);
                moveDirection = (moveDirection & ~Wall.WEST);
                break;
            case "EAST":
                moveDirection = (moveDirection & ~Wall.NORTH);
                moveDirection = (moveDirection & ~Wall.SOUTH);
                moveDirection = (moveDirection & ~Wall.WEST);
                break;
            case "SOUTH":
                moveDirection = (moveDirection & ~Wall.EAST);
                moveDirection = (moveDirection & ~Wall.NORTH);
                moveDirection = (moveDirection & ~Wall.WEST);
                break;
            case "WEST":
                moveDirection = (moveDirection & ~Wall.EAST);
                moveDirection = (moveDirection & ~Wall.SOUTH);
                moveDirection = (moveDirection & ~Wall.NORTH);
                break;
            default:
                break;
        }     
        var moveRequestMessage = new MoveRequestMessage() {
            Direction = (byte)moveDirection
        };
        clientBehaviour.SendMessage(moveRequestMessage);
    }

    public void SendAttackRequest()
    {
        DisableAllButtons();
        var attackRequestMessage = new AttackRequestMessage();
        clientBehaviour.SendMessage(attackRequestMessage);
    }

    public void SendDefendRequest()
    {
        DisableAllButtons();
        var message = new DefendRequestMessage();
        clientBehaviour.SendMessage(message);
    }

    public void SendClaimTreasureRequest()
    {
        DisableAllButtons();
        var message = new ClaimTreasureRequestMessage();
        clientBehaviour.SendMessage(message);
    }

    public void SendLeaveDungeonRequest()
    {
        DisableAllButtons();
        var message = new LeaveDungeonRequestMessage();
        clientBehaviour.SendMessage(message);
    }
    #endregion

    private void SetActiveDirectionButtons(Wall openDirections)
    {
        // Enable the correct buttons
        if((openDirections & Wall.NORTH) != 0)
            northButton.interactable = true;
        if ((openDirections & Wall.EAST) != 0)
            eastButton.interactable = true;
        if ((openDirections & Wall.SOUTH) != 0)
            southButton.interactable = true;
        if ((openDirections & Wall.WEST) != 0)
            westButton.interactable = true;
    }

    private void SetAttackDefendButtons()
    {
        attackButton.interactable = true;
        defendButton.interactable = true;
    }

    private void DisableAllButtons()
    {
        northButton.interactable = false;
        eastButton.interactable = false;
        southButton.interactable = false;
        westButton.interactable = false;
        claimTreasureButton.interactable = false;
        attackButton.interactable = false;
        defendButton.interactable = false;
        exitDungeonButton.interactable = false;
    }

    private void SetHPTreasureText()
    {
        HPTreasureText.text = $"HP: {ThisPlayer.CurrentHP} \n" +
            $"Gold: {ThisPlayer.TreasureAmount}";
    }

    private void SetMessagesText(Color32 color, string text)
    {
        string hexColor = ColorExtensions.ColorToHex(color);
        messagesText.text += $"<color=#{hexColor}>" + text + "</color>\n";
        messagesText.rectTransform.sizeDelta = new Vector2(messagesText.rectTransform.sizeDelta.x, messagesText.rectTransform.sizeDelta.y + lineSpacing);
    }

    public IEnumerator ShowConnectionDisconnected()
    {
        DisableAllButtons();
        SetMessagesText(Color.white, "The connection has been lost. You will be returned to start.");
        yield return new WaitForSeconds(2f);
        clientBehaviour.Disconnect();
        Destroy(clientBehaviour.gameObject);
        gameObject.GetComponent<SceneManagement>().ReloadScene();
    }

    private IEnumerator Restart()
    {
        SetMessagesText(Color.white, "The game will soon restart.");
        bool countdown = true;
        while (countdown)
        {
            for (int i = 3; i > 0; i--)
                yield return new WaitForSeconds(1f);
            countdown = false;
        }
        gameObject.GetComponent<SceneManagement>().ReloadScene();
    }

}
