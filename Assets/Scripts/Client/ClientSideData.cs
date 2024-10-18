using System.Collections.Generic;

public static class ClientSideData
{
    public static Dictionary<ulong, object> NetworkToClientObjects = new Dictionary<ulong, object>();

    public delegate void TurnState();
    public static event TurnState OnTurnEnd;

    public static void SendTurnEndEvent()
    {
        OnTurnEnd?.Invoke();
    }
}