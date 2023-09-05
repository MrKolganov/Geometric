using System;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;


public class Echo:WebSocketBehavior
{
    public static GameState gameStateClass = new GameState();

    private const string CLIENT_MESSAGE_TYPE_PLAYER_ENTER = "CLIENT_MESSAGE_TYPE_PLAYER_ENTER";
    private const string CLIENT_MESSAGE_TYPE_PLAYER_EXIT = "CLIENT_MESSAGE_TYPE_PLAYER_EXIT";
    private const string CLIENT_MESSAGE_TYPE_PLAYER_UPDATE = "CLIENT_MESSAGE_TYPE_PLAYER_UPDATE";
    private const string CLIENT_MESSAGE_TYPE_START_GAME = "CLIENT_MESSAGE_TYPE_START_GAME";

    private void routeMessage(string messageRouteMessage)
    {
        ClientMessageType cm = JsonSerializer.Deserialize<ClientMessageType>(messageRouteMessage);
        string messageType = cm.messageType;
        //Console.WriteLine("messageType | " + messageType);
        if(messageType == CLIENT_MESSAGE_TYPE_PLAYER_ENTER)
        {   
            Console.WriteLine("Enter |" + messageRouteMessage);
            string gsm = this.gameStateMessage();
            Console.WriteLine("GSM | " + gsm);
            Send(gsm);
            Console.WriteLine("messageType player enter | " + messageRouteMessage);
            this.HandlePlayerEnter(messageRouteMessage);
        }
        else if(messageType == CLIENT_MESSAGE_TYPE_PLAYER_UPDATE)
        { 
            this.HandlePlayerUpdate(messageRouteMessage);
        }
        else if(messageType == CLIENT_MESSAGE_TYPE_PLAYER_EXIT)
        {
            this.HandlePlayerExit(messageRouteMessage);
            Console.WriteLine("Exit");
        }
        else if(messageType == CLIENT_MESSAGE_TYPE_START_GAME)
        {
            this.HandleStartGame();
        }
        else
        {
            Console.WriteLine("else | " + messageType);
        }
    }

    private void HandleStartGame()
    {
        Sessions.Broadcast(startGameMessage());
    }
   
    private void HandlePlayerEnter(string message)
    {
        Player player = JsonSerializer.Deserialize<ClientMessageType>(message).player;
        gameStateClass.addPlayer(player);
        string enterPlayerString = enterPlayerMessage(player);
        Console.WriteLine("Handle player enter: " + enterPlayerString);
        Sessions.Broadcast(enterPlayerMessage(player));
    }

    private void HandlePlayerUpdate(string message)
    {
        object locker = new();
        Player playerUpdate = JsonSerializer.Deserialize<ClientMessageType>(message).player;
        Position position = playerUpdate.position;
        //Console.WriteLine("message in Update : " + message);
        Player player = gameStateClass.getPlayerByWebSocket(playerUpdate.id);
        //if (player == null)
        //{
        //    Console.WriteLine("Player wasn't found");
        //}
        //else
        //{
        //    Console.WriteLine("Player was found");
        //}
        lock(locker)
        {
            player.position = position;
        }
        string pUM = playerUpdateMessage(player);
        Sessions.Broadcast(pUM);
    }

    private void HandlePlayerExit(string message)
    {
        Player playerExit = JsonSerializer.Deserialize<ClientMessageType>(message).player;
        Player player = gameStateClass.getPlayerByWebSocket(playerExit.id);
        if (player != null)
        {
            gameStateClass.removePlayer(player);
            Sessions.Broadcast(playerExitMessage(player));
        }
        else
        {
            Console.WriteLine("player not found by websocket");
        }
    }
    private string startGameMessage()
    {
        Console.WriteLine("Отправляю | SERVER_MESSAGE_START_GAME");
        ServerMessageStartGame serverMessageStartGame = new ServerMessageStartGame();
        serverMessageStartGame.messageType = "SERVER_MESSAGE_START_GAME";
        return JsonSerializer.Serialize(serverMessageStartGame);
    }

    private string enterPlayerMessage(Player player)
    {
        ServerMessageEnterPlayer playerEnterMessage = new ServerMessageEnterPlayer();
        playerEnterMessage.messageType = "SERVER_MESSAGE_TYPE_PLAYER_ENTER";
        playerEnterMessage.player = player;
        Console.WriteLine("playerEnterMessage: " + JsonSerializer.Serialize(playerEnterMessage));
        return JsonSerializer.Serialize(playerEnterMessage);
    }

    private string playerUpdateMessage(Player player)
    {
        ServerMessageUpdatePlayer playerUpdateMessage = new ServerMessageUpdatePlayer();
        playerUpdateMessage.messageType = "SERVER_MESSAGE_TYPE_PLAYER_UPDATE";
        playerUpdateMessage.player = player;
        return JsonSerializer.Serialize(playerUpdateMessage);
    }

    private string playerExitMessage(Player player)
    {
        ServerMessageExitPlayer exitPlayer = new ServerMessageExitPlayer();
        exitPlayer.messageType = "SERVER_MESSAGE_TYPE_PLAYER_EXIT";
        exitPlayer.player = player;
        return JsonSerializer.Serialize(exitPlayer);
    }
    private string gameStateMessage()
    {
        ServerMessageGameState gameStateMessage = new ServerMessageGameState();
        gameStateMessage.messageType = "SERVER_MESSAGE_TYPE_GAME_STATE";
        gameStateMessage.gameState = gameStateClass;
        return JsonSerializer.Serialize(gameStateMessage);
    }
    protected override void OnMessage(MessageEventArgs e)
    {
        base.OnMessage(e);
        //Console.WriteLine("e.Data | " + e.Data);
        routeMessage(e.Data);
    }
}

[Serializable]
public class ServerMessageStartGame
{
    public string messageType { get; set; }
}

[Serializable]
public class ServerMessageEnterPlayer
{
    public string messageType { get; set; }
    public Player player { get; set; }
}
[Serializable]
public class ServerMessageUpdatePlayer
{
    public string messageType { get; set; }
    public Player player { get; set; }
}
[Serializable]
public class ServerMessageExitPlayer
{
    public string messageType { get; set; }
    public Player player { get; set; }
}
[Serializable]
public class ServerMessageGameState
{
    public string messageType { get; set; }
    public GameState gameState { get; set; }
}

[Serializable]
public class ClientMessageType
{
    public string messageType { get; set; }
    public Player player { get; set; }
}

[Serializable]
public class ClientStartGame
{
    public string messageType { get; set; }
}

[Serializable]
public class Player
{
    public string id { get; set; }
    public Position position { get;set; }
}

[Serializable]
public class Position
{
    public float x { get; set; }
    public float y { get; set; }
}
[Serializable]
public class GameState
{
    private List<Player> players = new List<Player>();
    public List<Player> playersSend { get { return players; } }
    object locker = new();

    public string playersString()
    {
        return JsonSerializer.Serialize(players);
    }

    public void addPlayer(Player player)
    {
        lock (locker)
        {
            players.Add(player);
        }
    }

    public void removePlayer(Player player)
    {
        lock (locker)
        {
            players.Remove(player);
        }
    }

    public Player getPlayerByWebSocket(string id)
    {
        foreach (Player player in this.players)
        {
            if (player.id == id)
                return player;
        }
        return null;
    }

    public void printPlayers()
    {
        if (players.Count > 0)
            foreach (var gamer in players)
                Console.WriteLine(gamer.id);
        else
            Console.WriteLine("На сервере нет игроков");
    }

}

class Program
{
    private const string host = "ws://localhost:5000";

    public static void Main(string[] args)
    {
        var wssv = new WebSocketServer(host);
        wssv.AddWebSocketService<Echo>("/Echo");
        wssv.Start();
        Console.WriteLine("Сервер | Сервер запущен " + DateTime.Now);
        string messageForServer = "";

        while(messageForServer != "/stop") {
            messageForServer = Console.ReadLine();
            if (messageForServer == "/stop")
            {
                Console.WriteLine("Остановка | " + DateTime.Now);
                wssv.Stop();
            }
            else if(messageForServer == "/help")
            {
                Console.WriteLine("Вот список доступных команд: \n\n" +
                        "   /stop | Останавливает сервер \n" +
                        "   /id players | Выводит id всех игроков \n");
            }
            else if(messageForServer == "/id players")
            {
                Echo.gameStateClass.printPlayers();
            }
            else
            {
                Console.WriteLine("Команда не найдена, для вывода списка команд введите: /help");
            }
        }
    }
}