using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLobbyChat : NetworkBehaviour
{
    public ChatMessage chatMessagePrefab;
    public Transform messageParent;
    public InputField chatInputField;

    private const int MaxNumberOfMessagesInList = 20;
    //private static List<NetworkVariable<ChatMessage>> _messages;
    public List<ChatMessage> _messages;

    private const float MinIntervalBetweenChatMessages = 1f;
    private float _clientSendTimer;

    private void Start()
    {
        //_messages = new List<NetworkVariable<ChatMessage>>();
        _messages = new List<ChatMessage>();
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //print("spawn");
    }

    private void Update()
    {
        if(NetworkManager.Singleton.IsClient)
        {
            _clientSendTimer += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (chatInputField.text.Length > 0 && _clientSendTimer > MinIntervalBetweenChatMessages)
                {
                    SendMessage();
                }
                else
                {
                    chatInputField.Select();
                    chatInputField.ActivateInputField();
                }
            }
        }
    }

    public void SendMessage()
    {
        string message = chatInputField.text;
        chatInputField.text = "";

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _clientSendTimer = 0;
        //Debug.Log("send message " + NetworkManager.Singleton.LocalClientId.ToString() + " " + message);
        SendChatMessageServerRpc(message, NetworkManager.Singleton.LocalClientId);
    }

    private void AddMessage(string message, ulong senderPlayerId, bool isServer)
    {
        var msg = Instantiate(chatMessagePrefab, messageParent);
        msg.SetMessage(senderPlayerId.ToString(), message, isServer);

        if(_messages.Count > 0)
        {
            //var position = _messages[_messages.Count - 1].Value.GetComponentInParent<Transform>().position;
            var position = _messages[_messages.Count - 1].GetComponentInParent<Transform>().position;
            position.y -= 30f;
            msg.GetComponentInParent<Transform>().position = position;
        }
        
        //_messages.Add(new NetworkVariable<ChatMessage>(msg));
        _messages.Add(msg);

        if (_messages.Count > MaxNumberOfMessagesInList)
        {
            //Destroy(_messages[0].Value);
            //_messages[0].Dispose();
            Destroy(_messages[0]);
            _messages.RemoveAt(0);
        }
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong senderPlayerId, bool isServer)
    {
        //print(message);
        //print(senderPlayerId);
        print(_messages.Count);
        AddMessage(message, senderPlayerId, isServer);
        print(_messages.Count);
    }

    [ServerRpc (RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ulong senderPlayerId)
    {
        //print("send SendChatMessageServerRpc");
        ReceiveChatMessageClientRpc(message, senderPlayerId, false);
    }
}
