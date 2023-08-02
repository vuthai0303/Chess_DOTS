using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkRoom : NetworkBehaviour
{
    public GameObject avatar_pl1;
    public GameObject avatar_pl2;
    public GameObject pl1_name;
    public GameObject pl2_name;
    public GameObject btn_ready1;
    public GameObject btn_ready2;
    public GameObject text_ready1;
    public GameObject text_ready2;
    public GameObject textRoomID;
    public GameObject btn_StartGame;

    private Dictionary<int, List<PlayerObj>> roomManager = new Dictionary<int, List<PlayerObj>>();
    private int myRoomID = -1;
    private List<PlayerObj> myRoom = new List<PlayerObj>();

    private const string READY = "ready";
    private const string UNREADY = "unready";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //print("network Client");

        enabled = IsClient;
        if (!enabled && myRoomID != -1)
        {
            return;
        }

        //print("player " + NetworkManager.Singleton.LocalClientId + " join room");
        connetRoomServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    private void Awake()
    {
        btn_ready1.GetComponentInChildren<Text>().text = READY;
        btn_ready2.GetComponentInChildren<Text>().text = UNREADY;
        btn_ready1.GetComponent<Button>().onClick.AddListener(delegate { onClickReadyButton(btn_ready1); });
        btn_ready2.GetComponent<Button>().onClick.AddListener(delegate { onClickReadyButton(btn_ready2); });
        btn_StartGame.GetComponent<Button>().onClick.AddListener(onClickStartGame);
    }

    private void Update()
    {
        if(myRoom.Count == 2)
        {

        }
        else
        {

        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private void setUI()
    {
        textRoomID.GetComponent<Text>().text = "Room: " + myRoomID;
        bool isPlayer1 = true;
        bool isReady = true;
        foreach(var player in myRoom)
        {
            if(player.playerID == NetworkManager.Singleton.LocalClientId)
            {
                if(isPlayer1)
                {
                    avatar_pl1.GetComponent<Image>().color = new Color(255, 0, 0);
                    pl1_name.GetComponent<Text>().text = player.name;
                    if(player.isReady)
                    {
                        btn_ready1.SetActive(true);
                        btn_ready1.GetComponentInChildren<Text>().text = UNREADY;
                        text_ready1.SetActive(false);
                    }
                    else
                    {
                        btn_ready1.SetActive(true);
                        btn_ready1.GetComponentInChildren<Text>().text = READY;
                        text_ready1.SetActive(false);
                    }
                }
                else
                {
                    avatar_pl2.GetComponent<Image>().color = new Color(255, 0, 0);
                    pl2_name.GetComponent<Text>().text = player.name;
                    if (player.isReady)
                    {
                        btn_ready2.SetActive(true);
                        btn_ready2.GetComponentInChildren<Text>().text = UNREADY;
                        text_ready2.SetActive(false);
                    }
                    else
                    {
                        btn_ready2.SetActive(true);
                        btn_ready2.GetComponentInChildren<Text>().text = READY;
                        text_ready2.SetActive(false);
                    }
                }
            }
            else
            {
                if (isPlayer1)
                {
                    avatar_pl1.GetComponent<Image>().color = new Color(0, 255, 0);
                    pl1_name.GetComponent<Text>().text = player.name;
                    if (player.isReady)
                    {
                        btn_ready1.SetActive(false);
                        text_ready1.SetActive(true);
                    }
                    else
                    {
                        btn_ready1.SetActive(false);
                        text_ready1.SetActive(false);
                    }
                }
                else
                {
                    avatar_pl2.GetComponent<Image>().color = new Color(0, 255, 0);
                    pl2_name.GetComponent<Text>().text = player.name;
                    if (player.isReady)
                    {
                        btn_ready2.SetActive(false);
                        text_ready2.SetActive(true);
                    }
                    else
                    {
                        btn_ready2.SetActive(false);
                        text_ready2.SetActive(false);
                    }
                }
            }
            isPlayer1 = false;
            isReady = isReady && player.isReady;
        }

        if(isReady && myRoom[0].playerID == NetworkManager.Singleton.LocalClientId)
        {
            btn_StartGame.SetActive(true);
        }
    }

    public void onClickStartGame()
    {
        NetworkManager.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void onClickReadyButton(GameObject btn)
    {
        if(btn.GetComponentInChildren<Text>().text == READY)
        {
            btn.GetComponentInChildren<Text>().text = UNREADY;
            ocClickBtnReadyServerRpc(NetworkManager.Singleton.LocalClientId, true, myRoomID);
        }
        else
        {
            btn.GetComponentInChildren<Text>().text = READY;
            ocClickBtnReadyServerRpc(NetworkManager.Singleton.LocalClientId, false, myRoomID);
        }
    }

    [ClientRpc]
    private void joinRoomClientRpc(PlayerObj[] lstPlayer, int roomID)
    {
        if(myRoomID == -1 || myRoomID == roomID)
        {
            myRoomID = roomID;
            myRoom.Clear();
            myRoom.AddRange(lstPlayer);
            setUI();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void connetRoomServerRpc(ulong senderPlayerId)
    {
        foreach (var roomId in roomManager.Keys)
        {
            var lstPlayer = roomManager[roomId];
            if (lstPlayer.Count >= 2)
            {
                continue;
            }
            var newPlayer = new PlayerObj("Player " + senderPlayerId, senderPlayerId, false, roomId);
            lstPlayer.Add(newPlayer);
            //print("player " + senderPlayerId + " have join room " + roomId);
            //GameObject.Find("UIMessage").GetComponent<NetworkLobbyChat>().ReceiveChatMessageClientRpc("Player " + newPlayer.playerID + " have join room " + newPlayer.roomID, senderPlayerId, true);
            joinRoomClientRpc(lstPlayer.ToArray(), roomId);
            return;
        }
        var lstPlayer_ = new List<PlayerObj>();
        var newPlayer_ = new PlayerObj("Player " + senderPlayerId, senderPlayerId, false, 0);
        lstPlayer_.Add(newPlayer_);
        roomManager.Add(0, lstPlayer_);
        //print("player " + senderPlayerId + " have join room 0");
        //if(GameObject.Find("UIMessage").TryGetComponent(out NetworkLobbyChat networkLobbyChat)){
        //    networkLobbyChat.ReceiveChatMessageClientRpc("Player " + newPlayer_.playerID + " have join room " + newPlayer_.roomID, senderPlayerId, true);
        //}
        joinRoomClientRpc(lstPlayer_.ToArray(), 0);
    }

    [ClientRpc]
    private void ocClickBtnReadyClientRpc(ulong senderPlayerId, bool isReady, int roomID)
    {
        if (roomID == myRoomID)
        {
            var newRoom = new List<PlayerObj>();
            for (var i = 0; i < myRoom.Count; i++)
            {
                var player = myRoom[i];
                var newPlayer = new PlayerObj(player);
                if (player.playerID == senderPlayerId)
                {
                    newPlayer.isReady = isReady;
                    print(newPlayer.name + " đã " + (newPlayer.isReady ? "sẵn sàng" : "hủy sẵn sàng"));
                }
                newRoom.Add(newPlayer);
            }
            myRoom = newRoom;
            setUI();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ocClickBtnReadyServerRpc(ulong senderPlayerId, bool isReady, int roomID)
    {
        ocClickBtnReadyClientRpc(senderPlayerId, isReady, roomID);
    }
}
