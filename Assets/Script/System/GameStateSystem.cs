﻿using Unity.Burst;
using Unity.Entities;

//[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct GameStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        GameStateComponent gameStateSingleton;
        var isStateGame = SystemAPI.TryGetSingleton(out gameStateSingleton);
        if (!isStateGame)
        {
            return;
        }

        ref SystemState changeGameStateSystem = ref state.WorldUnmanaged.GetExistingSystemState<ChangeGameStateSystem>();
        ref SystemState controlMoveSystem = ref state.WorldUnmanaged.GetExistingSystemState<ControlMoveSystem>();
        ref SystemState createMapSystem = ref state.WorldUnmanaged.GetExistingSystemState<CreateMapSystem>();
        ref SystemState configMapSystem = ref state.WorldUnmanaged.GetExistingSystemState<ConfigMapSystem>();
        ref SystemState movingSystem = ref state.WorldUnmanaged.GetExistingSystemState<MovingSystem>();
        ref SystemState renderMapSystem = ref state.WorldUnmanaged.GetExistingSystemState<RenderMapSystem>();
        ref SystemState scoreSystem = ref state.WorldUnmanaged.GetExistingSystemState<ScoreSystem>();
        ref SystemState spawnSystem = ref state.WorldUnmanaged.GetExistingSystemState<SpawnSystem>();
        ref SystemState switchTurnSystem = ref state.WorldUnmanaged.GetExistingSystemState<SwitchTurnSystem>();
        ref SystemState updateMapSystem = ref state.WorldUnmanaged.GetExistingSystemState<UpdateMapSystem>();

        //UnityEngine.Debug.Log(gameStateSingleton.state);

        switch (gameStateSingleton.state)
        {
            case (int)GameState.GameLoop:
                spawnSystem.Enabled = true;
                controlMoveSystem.Enabled = true;
                movingSystem.Enabled = true;
                switchTurnSystem.Enabled = true;
                updateMapSystem.Enabled = true;
                renderMapSystem.Enabled = true;
                changeGameStateSystem.Enabled = true;
                scoreSystem.Enabled = true;
                break;
            case (int)GameState.GameLoopNetwork:
                spawnSystem.Enabled = true;
                controlMoveSystem.Enabled = true;
                movingSystem.Enabled = true;
                switchTurnSystem.Enabled = true;
                updateMapSystem.Enabled = true;
                renderMapSystem.Enabled = true;
                changeGameStateSystem.Enabled = true;
                scoreSystem.Enabled = true;
                break;
            case (int)GameState.EndGame:
                spawnSystem.Enabled = false;
                controlMoveSystem.Enabled = false;
                movingSystem.Enabled = false;
                switchTurnSystem.Enabled = false;
                updateMapSystem.Enabled = false;
                renderMapSystem.Enabled = false;
                changeGameStateSystem.Enabled = false;
                scoreSystem.Enabled = false;
                break;
            case (int)GameState.RestartGame:
                spawnSystem.Enabled = false;
                controlMoveSystem.Enabled = false;
                movingSystem.Enabled = false;
                switchTurnSystem.Enabled = false;
                updateMapSystem.Enabled = false;
                renderMapSystem.Enabled = false;
                changeGameStateSystem.Enabled = false;
                scoreSystem.Enabled = false;
                break;
            default:
                spawnSystem.Enabled = false;
                controlMoveSystem.Enabled = false;
                movingSystem.Enabled = false;
                switchTurnSystem.Enabled = false;
                updateMapSystem.Enabled = false;
                renderMapSystem.Enabled = false;
                changeGameStateSystem.Enabled = false;
                scoreSystem.Enabled = false;
                break;
        }

    }
}