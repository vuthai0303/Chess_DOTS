using Assets.script.AuthoringAndMono;
using Unity.Collections;
using Unity.Entities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameController : NetworkBehaviour
{
    public GameObject m_text_room;
    public GameObject m_btn_ready_1;
    public GameObject m_btn_ready_2;
    public GameObject m_text_ready_1;
    public GameObject m_text_ready_2;
    public GameObject m_button_start;
    public GameObject m_text_endGame;
    public GameObject m_text_turnGame;
    public GameObject m_text_ScorePlayer1;
    public GameObject m_text_ScorePlayer2;
    public GameObject m_button_home;
    public Canvas m_canvas_UIMenu;
    public int[] arr;

    private StateGameManager stateGameManager;
    private World _world;

    private void Awake()
    {
        this._world = World.DefaultGameObjectInjectionWorld;
        this.stateGameManager = new StateGameManager();
    }

    void Start()
    {
        stateGameManager.addState((int)GameState.LoadingMap, new LoadingMap(stateGameManager, (int)GameState.LoadingMap, _world, this));

        stateGameManager.addState((int)GameState.Menu, new MenuState(stateGameManager, (int)GameState.Menu, _world, this
                                                                        //, m_input_size, m_input_numOfWall
                                                                        , m_button_start, m_canvas_UIMenu));

        stateGameManager.addState((int)GameState.GameLoop, new GameLoop(stateGameManager, (int)GameState.GameLoop, _world, this
                                                                        , m_text_endGame, m_text_turnGame
                                                                        , m_text_ScorePlayer1, m_text_ScorePlayer2));

        stateGameManager.addState((int)GameState.EndGame, new EndGame(stateGameManager, (int)GameState.EndGame, _world, this
                                                                        , m_text_endGame, m_canvas_UIMenu));

        stateGameManager.addState((int)GameState.RestartGame, new RestartGame(stateGameManager, (int)GameState.RestartGame, _world, this));

        stateGameManager.setCurrentState(stateGameManager.getState((int)GameState.LoadingMap));

        //print(GameObject.Find("SubScreen").GetComponent<SubScene>().EditingScene. );
        //GameObject.Find("SubScreen").GetComponent<SubScene>().enabled = true;
        m_button_start.GetComponent<Button>().onClick.AddListener(OnClickStartGame);

    }

    void Update()
    {
        stateGameManager.getCurrentState().Update();
    }

    public void OnClickStartGame()
    {
        //if (IsServer)
        //{
        //    onClickStartGameServerRPC(0, NetworkManager.LocalClientId, new NativeArray<int>());
        //}
        onClickStartGameServerRPC(0);
    }

    [ClientRpc]
    public void onClickStartGameClientRPC(int roomID, int[] maps) 
    {
        arr = maps;
        if (IsServer)
        {
            return;
        }
        print("client " + NetworkManager.LocalClientId + " is loading map");
        stateGameManager.getState((int)GameState.LoadingMap).setMaps(maps);
        stateGameManager.setCurrentState(stateGameManager.getState((int)GameState.LoadingMap));
    }

    [ServerRpc]
    public void onClickStartGameServerRPC(int roomID)
    {
        print("Server is loadding map");
        stateGameManager.setCurrentState(stateGameManager.getState((int)GameState.LoadingMap));
    }

}