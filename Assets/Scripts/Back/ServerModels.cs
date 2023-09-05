using System.Collections;
using System;

[Serializable]
public class ServerMessageGeneric
{
    public string messageType;
}

[Serializable]
public class ServerMessagePlayerEnter
{
    public string messageType;
    public Player player;
}

[Serializable]
public class ServerMessagePlayerUpdate
{
    public string messageType;
    public Player player;
}

[Serializable]
public class ServerMessagePlayerExit
{
    public string messageType;
    public Player player;
}

[Serializable]
public class ServerMessageGameState
{
    public string messageType;
    public GameState gameState;
}

[Serializable]
public class ClientMessagePlayerUpdate
{
    public string messageType = "CLIENT_MESSAGE_TYPE_PLAYER_UPDATE";
    public Player player;

    public ClientMessagePlayerUpdate(Player playerModel)
    {
        this.player = playerModel;
    }
}