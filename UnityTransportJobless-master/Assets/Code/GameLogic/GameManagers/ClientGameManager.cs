using System.Collections.Generic;
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
    [SerializeField]
    private Text messagesText, HPTreasureText;
    [SerializeField]
    private Button claimTreasureButton, attackButton, defendButton, exitDungeonButton;
    [SerializeField]
    private Button northButton, eastButton, southButton, westButton;
    
    [HideInInspector]
    public Client ThisClient { get; set; }
    public Player ThisPlayer { get; set; }
    public List<Client> AllClientsList { get; set; }

    private ClientBehaviour clientBehaviour;

    private void Start()
    {
        GameUI.SetActive(true);
        clientBehaviour = GameObject.FindGameObjectWithTag("Client").GetComponent<ClientBehaviour>();
        clientBehaviour.ClientGameManager = this;
        messagesText.text = "";

        ThisPlayer = new Player();
        ThisPlayer.SetStartHP(ThisClient.StartHP);
        ThisPlayer.TreasureAmount = 0;
        SetHPTreasureText();

    }

    #region Show Messages
    public void ShowPlayerTurnMessage(MessageConnection messageConnection)
    {
        var message = (messageConnection.messageHeader as PlayerTurnMessage);
        SetMessagesText($"It's Player {message.PlayerID}'s turn!");
    }

    public void ShowRoomInfoMessage(MessageConnection messageConnection)
    {
        DisableAllButtons();
        var message = (messageConnection.messageHeader as RoomInfoMessage);
        Wall openDirections = (Wall)message.MoveDirections;
        bool monster = System.Convert.ToBoolean(message.ContainsMonster);
        if (!monster)
        {
            SetActiveDirectionButtons(openDirections);
            SetMessagesText($"You can go to the {openDirections}");
        }           
        else
        {
            SetAttackDefendButtons();
            SetMessagesText("There's a monster in this room!");   
        }
            
    }

    #endregion

    #region SendRequests
    public void SendMoveRequest(string directionString)
    {
        // The button has been clicked, so disable them.
        DisableAllButtons();

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
            $"Treasure amount {ThisPlayer.TreasureAmount}";
    }

    private void SetMessagesText(string text)
    {
        messagesText.text += text + "\n";
        messagesText.rectTransform.sizeDelta = new Vector2(messagesText.rectTransform.sizeDelta.x, messagesText.rectTransform.sizeDelta.y + lineSpacing);
    }
}
