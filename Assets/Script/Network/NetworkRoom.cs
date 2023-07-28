using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkRoom : NetworkBehaviour
{
    public GameObject avatar_pl1;
    public GameObject avatar_pl2;
    public GameObject textRoomID;

    private Dictionary<int, List<ulong>> roomManager = new Dictionary<int, List<ulong>>();
    private int myRoomID = -1;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //print("network Client");

        enabled = IsClient;
        if (!enabled && myRoomID != -1)
        {
            return;
        }

        print("player " + NetworkManager.Singleton.LocalClientId + " join room");
        connetRoomServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    private void Update()
    {
        //if(IsClient)
        //{

        //}
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private void setUI(bool isPlayer1, ulong senderPlayerId)
    {
        textRoomID.GetComponent<Text>().text = "Room: " + myRoomID;
        print(senderPlayerId);
        print(NetworkManager.Singleton.LocalClientId);
        if (senderPlayerId == NetworkManager.Singleton.LocalClientId)
        {
            if (isPlayer1)
            {
                avatar_pl1.GetComponent<Image>().color = new Color(255, 0, 0);
                avatar_pl2.GetComponent<Image>().color = new Color(255, 255, 255);
            }
            else
            {
                avatar_pl2.GetComponent<Image>().color = new Color(255, 0, 0);
                avatar_pl1.GetComponent<Image>().color = new Color(0, 255, 0);
            }
        }
        else
        {
            avatar_pl2.GetComponent<Image>().color = new Color(0, 255, 0);
            avatar_pl1.GetComponent<Image>().color = new Color(255, 0, 0);
        }
        
    }

    [ClientRpc]
    private void joinRoomClientRpc(bool isPlayer1, ulong senderPlayerId, int roomID)
    {
        setUI(isPlayer1, senderPlayerId);
        myRoomID = roomID;
        print("player " + senderPlayerId + "Join room " + myRoomID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void connetRoomServerRpc(ulong senderPlayerId)
    {
        foreach (var roomId in roomManager.Keys)
        {
            var value = roomManager[roomId];
            if (value.Count >= 2)
            {
                continue;
            }
            value.Add(senderPlayerId);
            print("player " + senderPlayerId + " have join room " + roomId);
            joinRoomClientRpc(false, senderPlayerId, roomId);
            return;
        }
        var lst = new List<ulong>();
        lst.Add(senderPlayerId);
        roomManager.Add(0, lst);
        print("player " + senderPlayerId + " have join room 0");
        joinRoomClientRpc(true, senderPlayerId, 0);
    }
}
