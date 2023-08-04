using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        var networkManager = NetworkManager.Singleton;
        if(!networkManager.IsServer && !networkManager.IsClient)
        {
            if (GUILayout.Button("Host"))
            {
                networkManager.StartHost();
                //networkManager.SceneManager.LoadScene("MenuGame", LoadSceneMode.Single);
            }
            if (GUILayout.Button("Client"))
            {
                networkManager.StartClient();
            }
            if (GUILayout.Button("Server"))
            {
                networkManager.StartServer();
                //networkManager.SceneManager.LoadScene("MenuGame", LoadSceneMode.Single);
            }
        }
        else
        {
            GUILayout.Label($"Mode: {(networkManager.IsHost ? "Host" : networkManager.IsServer ? "Server" : "Client")}");
            //if (networkManager.IsClient)
            //{
            //    if (GUILayout.Button("Random position"))
            //    {
            //        if (networkManager.LocalClient != null)
            //        {
            //            if (networkManager.LocalClient.PlayerObject.TryGetComponent(out NetworkPlayer networkPlayer))
            //            {
            //                networkPlayer.randomPositionServerRPC();
            //            }
            //        }
            //    }
            //}
        }

        GUILayout.EndArea();
    }
}
