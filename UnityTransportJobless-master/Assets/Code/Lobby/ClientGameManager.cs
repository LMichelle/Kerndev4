using UnityEngine;
using UnityEngine.UI;

public class ClientGameManager : MonoBehaviour
{
    public GameObject GameUI;
    public Text messagesText, HPTreasureText;
    public Button claimTreasureButton, attackButton, defendButton, exitDungeonButton;
    public Button northButton, eastButton, southButton, westButton;
    [HideInInspector]
    public Client ThisClient { get; set; }

    private void Start()
    {
        GameUI.SetActive(true);
    }
}
