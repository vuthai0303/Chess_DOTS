using CortexDeveloper.ECSMessages.Service;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.script.AuthoringAndMono
{
    public class State
    {
        public int id;
        protected StateGameManager gameManager;
        protected World m_world;

        public State(StateGameManager gameManager, int id, World world)
        {
            this.gameManager = gameManager;
            this.id = id;   
            this.m_world = world;
        }

        public virtual void Enter() {
            MessageBroadcaster.PrepareMessage().AliveForOneFrame().PostImmediate(m_world.EntityManager, new GameStateMessage { state = id });
        }
        public virtual void Exit() { }
        public virtual void FixedUpdate() { }
        public virtual void Update() { }
    }

    public class MenuState : State
    {
        protected GameObject m_Button_start;
        protected Canvas m_canvas_menu;
        protected Canvas m_canvas_GameUI;

        public MenuState(StateGameManager manager, int id, World world
                        , GameObject buttonStart,Canvas canvas_menu
                        , Canvas canvas_GameUI) : base(manager, id, world)
        {
            m_Button_start = buttonStart;
            m_canvas_menu = canvas_menu;
            m_canvas_GameUI = canvas_GameUI;
        }

        public override void Enter()
        {
            base.Enter();
            m_canvas_menu.enabled = true;
            m_canvas_GameUI.enabled = false;
            m_Button_start.SetActive(true);

        }

        public override void Exit()
        {
            base.Exit();
            m_canvas_menu.enabled=false;
            m_canvas_GameUI.enabled = true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }

    public class GameLoop : State
    {
        protected GameObject m_text_endGame;
        protected GameObject m_text_turnGame;
        protected GameObject m_text_ScorePlayer1;
        protected GameObject m_text_ScorePlayer2;
        protected GameObject m_button_restart;
        protected GameObject m_button_home;

        public GameLoop(StateGameManager manager, int id, World world
                        , GameObject textEndGame, GameObject textTurnGame
                        , GameObject textScrorePlayer1, GameObject textScorePlayer2
                        , GameObject buttonRestart, GameObject buttonHome) : base(manager, id, world)
        {
            m_text_endGame = textEndGame;
            m_text_turnGame = textTurnGame;
            m_text_ScorePlayer1 = textScrorePlayer1;
            m_text_ScorePlayer2 = textScorePlayer2;
            m_button_restart = buttonRestart;
            m_button_home = buttonHome;
        }

        public override void Enter()
        {
            base.Enter();

            m_text_turnGame.SetActive(true);
            m_text_ScorePlayer1.SetActive(true);
            m_text_ScorePlayer2.SetActive(true);

            m_text_endGame.SetActive(false);
            m_button_restart.SetActive(false);
            m_button_home.SetActive(false);
        }

        public override void Exit()
        {
            base.Exit();
            m_text_turnGame.GetComponent<Text>().text = "";
            m_text_turnGame.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
            ChangeGameStateComponent changeStateComponent;
            var isChangeStateComponent = m_world.EntityManager.CreateEntityQuery(typeof(ChangeGameStateComponent))
                .TryGetSingleton<ChangeGameStateComponent>(out changeStateComponent);
            if (isChangeStateComponent)
            {
                this.gameManager.setCurrentState(gameManager.getState(changeStateComponent.newState));
            }
            else
            {
                writeTurnPlayer();
            }
            writeScorePlayer();
        }

        void writeTurnPlayer()
        {
            TurnPlayComponent turnPlayComponent;
            var isTurnPlayComponent = m_world.EntityManager.CreateEntityQuery(typeof(TurnPlayComponent)).TryGetSingleton<TurnPlayComponent>(out turnPlayComponent);
            if (isTurnPlayComponent)
            {
                if (turnPlayComponent.value == (int)Player.Player1)
                {
                    m_text_turnGame.GetComponent<Text>().text = "Player1 is playing...!";
                }
                else
                {
                    m_text_turnGame.GetComponent<Text>().text = "Player2 is playing...!";
                }
            }
            else
            {
                m_text_turnGame.GetComponent<Text>().text = "Error!";
            }
        }

        void writeScorePlayer()
        {
            ScoreComponent score;
            var isScoreComponent = m_world.EntityManager.CreateEntityQuery(typeof(ScoreComponent)).TryGetSingleton<ScoreComponent>(out score);
            if (isScoreComponent)
            {
                m_text_ScorePlayer1.GetComponent<Text>().text = score.score_player1 + " : Player 1";
                m_text_ScorePlayer2.GetComponent<Text>().text = "Player 2 : " + score.score_player2;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }

    public class EndGame : State
    {
        protected GameObject m_text_endGame;
        protected GameObject m_button_restart;
        protected GameObject m_button_home;

        public EndGame(StateGameManager manager, int id, World world
                        , GameObject textEndGame, GameObject buttonRestart
                        , GameObject buttonHome) : base(manager, id, world)
        {
            m_text_endGame = textEndGame;
            m_button_restart = buttonRestart;
            m_button_home = buttonHome;
        }

        public override void Enter()
        {
            base.Enter();
            WinnerComponent winnerComponent;
            var isWinnerComponent = m_world.EntityManager.CreateEntityQuery(typeof(WinnerComponent)).TryGetSingleton<WinnerComponent>(out winnerComponent);
            if (isWinnerComponent)
            {
                if(winnerComponent.value == (int)Player.Player1)
                {
                    m_text_endGame.GetComponent<Text>().text = "Player1 is Winner";
                }
                else if(winnerComponent.value == (int)Player.Player2)
                {
                    m_text_endGame.GetComponent<Text>().text = "Player2 is Winner";
                }
                else
                {
                    m_text_endGame.GetComponent<Text>().text = "No Player Winner";
                }
            }
            m_text_endGame.SetActive(true);
            m_button_restart.SetActive(true);
            m_button_home.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();
            m_text_endGame.GetComponent<Text>().text = "";
            m_text_endGame.SetActive(false);
            m_button_restart.SetActive(false);
            m_button_home.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }

    public class RestartGame : State
    {

        public RestartGame(StateGameManager manager, int id, World world) : base(manager, id, world)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update()
        {
            base.Update();
            gameManager.setCurrentState(gameManager.getState((int)GameState.GameLoop));
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}