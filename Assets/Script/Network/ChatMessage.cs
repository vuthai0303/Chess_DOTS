using UnityEngine;
using UnityEngine.UI;

public class ChatMessage : MonoBehaviour
{

    [SerializeField] private Text TextField;

    public void SetMessage(string playerName, string message)
    {
        //textField.text = $"<color=grey>{playerName}</color>: {message}";
        TextField.text = $"<color=Blue>Player {playerName}</color>: {message}";
    }
}