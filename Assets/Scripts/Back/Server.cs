using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class Server : MonoBehaviour
{
    public GameObject goButton;
    public GameObject playerPrefab;

    private WebSocket ws;
    private string gameServerUrl = "ws://localhost:5000/Echo";
    //private string gameServerUrl = "ws://0.tcp.ngrok.io:16554"; // example ngrok URL

    private Player mainPlayerModel;
    private GameObject mainPlayerGO;

    private IDictionary<string, GameObject> playerIdToOtherPlayerGO =
            new Dictionary<string, GameObject>();

    private Queue<string> gameServerMessageQueue = new Queue<string>();

    private const string SERVER_MESSAGE_TYPE_PLAYER_ENTER = "SERVER_MESSAGE_TYPE_PLAYER_ENTER";
    private const string SERVER_MESSAGE_TYPE_PLAYER_EXIT = "SERVER_MESSAGE_TYPE_PLAYER_EXIT";
    private const string SERVER_MESSAGE_TYPE_PLAYER_UPDATE = "SERVER_MESSAGE_TYPE_PLAYER_UPDATE";
    private const string SERVER_MESSAGE_TYPE_GAME_STATE = "SERVER_MESSAGE_TYPE_GAME_STATE";
    private const string SERVER_MESSAGE_START_GAME = "SERVER_MESSAGE_START_GAME";

    // UNITY HOOKS

    private void Start()
    {
        this.InitWebSocketClient();
        this.InitMainPlayer();
    }

    private void Update()
    {
        // process all queued server messages
        while (this.gameServerMessageQueue.Count > 0)
        {
            string gsmq = this.gameServerMessageQueue.Dequeue();
            //Debug.Log(gsmq);
            this.HandleServerMessage(gsmq);
        }
    }

    private void OnDestroy()
    {
        // close websocket connection
        this.ws.Close(CloseStatusCode.Normal);
    }

    // INTERFACE METHODS

    public void SyncPlayerState(GameObject playerGO)
    {
        // send "player update" message to server
        this.mainPlayerModel.position = new Position(
            playerGO.transform.position.x,
            playerGO.transform.position.y
        );
        var playerUpdateMessage = new ClientMessagePlayerUpdate(this.mainPlayerModel);
        this.ws.Send(JsonUtility.ToJson(playerUpdateMessage));
    }

    // IMPLEMENTATION METHODS

    private void InitWebSocketClient()
    {
        // create websocket connection
        this.ws = new WebSocket(this.gameServerUrl);
        this.ws.Connect();
        // add message handler callback
        this.ws.OnMessage += this.QueueServerMessage;
    }

    private void InitMainPlayer()
    {
        // create player game object
        var playerPos = Vector3.zero;
        this.mainPlayerGO = Instantiate(this.playerPrefab, playerPos, Quaternion.identity);
        var mainPlayerScript = this.mainPlayerGO.GetComponent<Hero>();
        mainPlayerScript.server = this;
        mainPlayerScript.isMainPlayer = true;
        // create player model
        string uuid = System.Guid.NewGuid().ToString();
        var pos = new Position(this.transform.position.x, this.transform.position.y);
        this.mainPlayerModel = new Player(uuid, pos);
        // send "player enter" message to server
        var playerEnterMessage = new ClientMessagePlayerEnter(this.mainPlayerModel);

        Debug.Log("player id | " + playerEnterMessage.player.id);
        Debug.Log("player position x | " + playerEnterMessage.player.position.x);
        Debug.Log("player position y | " + playerEnterMessage.player.position.y);

        Debug.Log("player enter message | " + JsonUtility.ToJson(playerEnterMessage));
        this.ws.Send(JsonUtility.ToJson(playerEnterMessage));
    }

    private void QueueServerMessage(object sender, MessageEventArgs e)
    {
        //Debug.Log("Server message received: " + e.Data);
        this.gameServerMessageQueue.Enqueue(e.Data);
    }

    private void HandleServerMessage(string messageJSON)
    {
        // parse message type
        string messageType = JsonUtility.FromJson<ServerMessageGeneric>(messageJSON).messageType;
        // route message to handler based on message type
        if (messageType == SERVER_MESSAGE_TYPE_PLAYER_ENTER)
        {
            this.HandlePlayerEnterServerMessage(messageJSON);
        }
        else if (messageType == SERVER_MESSAGE_TYPE_PLAYER_EXIT)
        {
            this.HandlePlayerExitServerMessage(messageJSON);
        }
        else if (messageType == SERVER_MESSAGE_TYPE_PLAYER_UPDATE)
        {
            this.HandlePlayerUpdateServerMessage(messageJSON);
        }
        else if (messageType == SERVER_MESSAGE_TYPE_GAME_STATE)
        {
            this.HandleGameStateServerMessage(messageJSON);
        }
        else if(messageType == SERVER_MESSAGE_START_GAME)
        {
            this.HandleStartGameMessage();
        }
        else
        {
            Debug.Log(messageJSON);
        }
    }

    private void HandlePlayerEnterServerMessage(string messageJSON)
    {
        var playerEnterMessage = JsonUtility.FromJson<ServerMessagePlayerEnter>(messageJSON);
        this.AddOtherPlayerFromPlayerModel(playerEnterMessage.player);
    }

    private void HandlePlayerExitServerMessage(string messageJSON)
    {
        var playerExitMessage = JsonUtility.FromJson<ServerMessagePlayerExit>(messageJSON);
        string playerId = playerExitMessage.player.id;
        if (this.playerIdToOtherPlayerGO.ContainsKey(playerId))
        {
            UnityEngine.Object.Destroy(this.playerIdToOtherPlayerGO[playerId]);
            this.playerIdToOtherPlayerGO.Remove(playerId);
        }
    }

    private void HandlePlayerUpdateServerMessage(string messageJSON)
    {
        //Debug.Log("messageJson | " + messageJSON);
        var playerUpdateMessage = JsonUtility.FromJson<ServerMessagePlayerUpdate>(messageJSON);
        Player playerModel = playerUpdateMessage.player;
        //Debug.Log("Update id : " + playerModel.id);
        foreach(var playerIdOterList in this.playerIdToOtherPlayerGO)
        {
            //Debug.Log(playerIdOterList.Key);
        }
        if (this.playerIdToOtherPlayerGO.ContainsKey(playerModel.id))
        {
            //Debug.Log("Update id was found");
            var newPosition = new Vector3(
                playerModel.position.x,
                playerModel.position.y,
                0
            );
            this.playerIdToOtherPlayerGO[playerModel.id].transform.position = newPosition;
        }
    }
    
    public void GoButtonPress()
    {
        var clientMessageStartGame = new ClientMessageStartGame();
        this.ws.Send(JsonUtility.ToJson(clientMessageStartGame));
    }
    private void HandleStartGameMessage()
    {
        goButton.SetActive(false);
        Time.timeScale = 1f;
    }

    private void HandleGameStateServerMessage(string messageJSON)
    {
        //Debug.Log(messageJSON);
        var gameStateMessage = JsonUtility.FromJson<ServerMessageGameState>(messageJSON);
        foreach (Player player in gameStateMessage.gameState.playersSend)
        {
            this.AddOtherPlayerFromPlayerModel(player);
        }
    }

    private void AddOtherPlayerFromPlayerModel(Player otherPlayerModel)
    {
        // player is not main player and player is not currently tracked
        if (
            otherPlayerModel.id != this.mainPlayerModel.id
            && !this.playerIdToOtherPlayerGO.ContainsKey(otherPlayerModel.id)
        )
        {
            var otherPlayerPosition = new Vector3(
                otherPlayerModel.position.x,
                otherPlayerModel.position.y,
                0
            );
            GameObject otherPlayerGO = Instantiate(
                this.playerPrefab,
                otherPlayerPosition,
                Quaternion.identity
            );
            var otherPlayerScript = otherPlayerGO.GetComponent<Hero>();
            otherPlayerScript.server = this;
            otherPlayerScript.isMainPlayer = false;
            this.playerIdToOtherPlayerGO.Add(otherPlayerModel.id, otherPlayerGO);
        }
    }
}

[Serializable]
public class Position
{
    public float x;
    public float y;

    public Position(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

}
[Serializable]
public class Player
{
    public string id;
    public Position position;

    public Player(string id, Position position)
    {
        this.id = id;
        this.position = position;
    }
}
[Serializable]
public class GameState
{
    //public List<string> connectionIds;
    public List<Player> playersSend;
}

[Serializable]
public class ClientMessagePlayerEnter
{
    public string messageType = "CLIENT_MESSAGE_TYPE_PLAYER_ENTER";
    public Player player;

    public ClientMessagePlayerEnter(Player playerModel)
    {
         this.player = playerModel;
    }
}

[Serializable]
public class ClientMessageStartGame
{
    public string messageType = "CLIENT_MESSAGE_TYPE_START_GAME";
}