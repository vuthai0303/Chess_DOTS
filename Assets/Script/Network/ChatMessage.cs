using UnityEngine;
using UnityEngine.UI;

public class ChatMessage : MonoBehaviour
{

    [SerializeField] private Text TextField;

    public void SetMessage(string playerName, string message, bool isServer)
    {
        if(isServer)
        {
            TextField.text = $"<color=yellow>{message}</color>";
        }
        else
        {
            TextField.text = $"<color=Blue>Player {playerName}</color>: {message}";
        }
        
    }
}