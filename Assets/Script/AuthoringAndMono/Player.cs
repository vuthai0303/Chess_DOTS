using Unity.Netcode;

public struct PlayerObj: INetworkSerializable
{
    public string name;
    public ulong playerID;
    public bool isReady;
    public int roomID;

    public PlayerObj(string name, ulong playerID, bool isReady, int roomID) { 
        this.name = name;
        this.playerID = playerID;
        this.roomID = roomID;
        this.isReady = isReady;
    }

    public PlayerObj(PlayerObj oldPlayer)
    {
        this.name = oldPlayer.name;
        this.playerID = oldPlayer.playerID;
        this.roomID = oldPlayer.roomID;
        this.isReady = oldPlayer.isReady;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref roomID);
    }
}