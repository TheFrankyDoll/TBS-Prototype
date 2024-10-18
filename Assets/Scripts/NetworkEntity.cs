// Most Network frameworks do data transmission this on their part, this is just a simplified example.
// e.g Mirror would do this with 'NetworkIdentity' - see https://mirror-networking.gitbook.io/docs/manual/components/network-identity

/// <summary>
/// Lets Server-side objects to have a way to send data to clients and fine-tune what data will be sent.
/// </summary>
public interface IServerItemOf<TClientItem> : IdentifiableServerData
{
    /// <summary>
    /// This should return a copy of an object meant to be sent to client, with all data client must know.
    /// <para> It is relatively slow and other data transfer methods should be used when possible. </para>
    /// </summary>
    public TClientItem MakeClientCopy();

    /// <summary>
    /// Map all fields of client object to new ones from the server.
    /// </summary>
    public void UpdateClientData(TClientItem clientObject); //There are A LOT of ways to handle this. I'll implement it with JsonUtility for this example.
}

public interface IdentifiableServerData
{
    public ulong NetID { get; }
}