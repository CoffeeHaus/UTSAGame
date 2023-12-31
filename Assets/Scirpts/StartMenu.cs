using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public NetworkManager networkManager;
    public TextMeshProUGUI playerNameInput;
    public Button hostGameButton;
    public Button joinGameButton;

    void Start()
    {
        hostGameButton.onClick.AddListener(HostGame);
        joinGameButton.onClick.AddListener(JoinGame);
    }

    public void StartHosting()
    {
        networkManager.StartHost();
        // Optionally, hide your menu here or transition to another UI.
    }

    void HostGame()
    {
        // Set player name, or any logic to validate player name
        string playerName = playerNameInput.text;

        // TODO: Set playerName in your player script or send it to the server

        // Start hosting the game
        MyNetworkManager.singleton.StartHost();
    }

    void JoinGame()
    {
        // Set player name, or any logic to validate player name
        string playerName = playerNameInput.text;

        // TODO: Set playerName in your player script or send it to the server

        // Start as a client
        MyNetworkManager.singleton.StartClient();
    }
    
        public void QuitGame()
    {
        Application.Quit();
    }
}
