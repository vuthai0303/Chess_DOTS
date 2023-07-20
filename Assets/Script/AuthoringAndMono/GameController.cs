using Assets.script.AuthoringAndMono;
using Unity.Entities;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject m_button_start;
    public GameObject m_input_size;
    public GameObject m_input_numOfWall;
    public GameObject m_text_endGame;
    public GameObject m_text_turnGame;
    public GameObject m_text_ScorePlayer1;
    public GameObject m_text_ScorePlayer2;
    public GameObject m_button_restart;
    public GameObject m_button_home;
    public Canvas m_canvas_menu;
    public Canvas m_canvas_gameUI;

    private StateGameManager stateGameManager;

    private World _world;

    private void Awake()
    {
        this._world = World.DefaultGameObjectInjectionWorld;
        this.stateGameManager = new StateGameManager();
    }

    void Start()
    {
        stateGameManager.addState((int)GameState.Menu, new MenuState(stateGameManager, (int)GameState.Menu, _world
                                                                        , m_input_size, m_input_numOfWall
                                                                        , m_button_start, m_canvas_menu, m_canvas_gameUI));

        stateGameManager.addState((int)GameState.GameLoop, new GameLoop(stateGameManager, (int)GameState.GameLoop, _world
                                                                        , m_text_endGame, m_text_turnGame
                                                                        , m_text_ScorePlayer1, m_text_ScorePlayer2
                                                                        , m_button_restart, m_button_home));

        stateGameManager.addState((int)GameState.EndGame, new EndGame(stateGameManager, (int)GameState.EndGame, _world
                                                                        , m_text_endGame, m_button_restart, m_button_home));

        stateGameManager.addState((int)GameState.RestartGame, new RestartGame(stateGameManager, (int)GameState.RestartGame, _world));

        stateGameManager.setCurrentState(stateGameManager.getState((int)GameState.Menu));
    }

    void Update()
    {
        stateGameManager.getCurrentState().Update();
    }

    public void OnClickStartGame()
    {
        stateGameManager.setCurrentState(stateGameManager.getState((int)GameState.GameLoop));
    }

    public void onClickRestartGame()
    {
        stateGameManager.setCurrentState(stateGameManager.getState((int)GameState.RestartGame));
    }

    public void onClickHome()
    {
        stateGameManager.setCurrentState(stateGameManager.getState((int)GameState.Menu));
    }
}