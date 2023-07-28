using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLobbyChat : NetworkBehaviour
{
    [SerializeField]
    private ChatMessage chatMessagePrefab;
    [SerializeField]
    private Transform messageParent;
    [SerializeField]
    private InputField chatInputField;

    private const int MaxNumberOfMessagesInList = 20;
    private List<NetworkVariable<ChatMessage>> _messages;

    private const float MinIntervalBetweenChatMessages = 1f;
    private float _clientSendTimer;

    private void Start()
    {
        _messages = new List<NetworkVariable<ChatMessage>>();
        
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
        Debug.Log("send message " + NetworkManager.Singleton.LocalClientId.ToString() + " " + message);
        SendChatMessageServerRpc(message, NetworkManager.Singleton.LocalClientId);
    }

    private void AddMessage(string message, ulong senderPlayerId)
    {
        var msg = Instantiate(chatMessagePrefab, messageParent);
        msg.SetMessage(senderPlayerId.ToString(), message);

        if(_messages.Count > 0)
        {
            var position = _messages[_messages.Count - 1].Value.GetComponentInParent<Transform>().position;
            position.y -= 30f;
            msg.GetComponentInParent<Transform>().position = position;
        }
        
        _messages.Add(new NetworkVariable<ChatMessage>(msg));

        if (_messages.Count > MaxNumberOfMessagesInList)
        {
            Destroy(_messages[0].Value);
            _messages[0].Dispose();
            _messages.RemoveAt(0);
        }
    }

    [ClientRpc]
    private void ReceiveChatMessageClientRpc(string message, ulong senderPlayerId)
    {
        print(message);
        print(senderPlayerId);
        AddMessage(message, senderPlayerId);
    }

    [ServerRpc (RequireOwnership = false)]
    private void SendChatMessageServerRpc(string message, ulong senderPlayerId)
    {
        print("send SendChatMessageServerRpc");
        ReceiveChatMessageClientRpc(message, senderPlayerId);
    }
}
