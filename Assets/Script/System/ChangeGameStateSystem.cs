using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct ChangeGameStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        GameStateComponent gameStateComponent;
        var isStateGame = SystemAPI.TryGetSingleton(out gameStateComponent);
        if (!isStateGame)
        {
            return;
        }

        MapComponent mapComponent;
        var isMapComponent = SystemAPI.TryGetSingleton(out mapComponent);
        if (!isMapComponent)
        {
            return;
        }

        var stateGame = gameStateComponent.state;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        //check change game state
        bool isLosePlayer1 = false;
        bool isLosePlayer2 = false;
        foreach(var playerComponent in SystemAPI.Query<RefRO<PlayerComponent>>())
        {
            bool isMove = isHaveMove(convertInttoVector(playerComponent.ValueRO.cellID, mapComponent.size), mapComponent);
            if (playerComponent.ValueRO.playerID == (int)Player.Player1 && !isMove)
            {
                isLosePlayer1 = true;
            }
            else if(playerComponent.ValueRO.playerID == (int)Player.Player2 && !isMove)
            {
                isLosePlayer2 = true;
            }
        }
        
        if (isLosePlayer1 && isLosePlayer2)
        {
            stateGame = (int)GameState.EndGame;
            foreach (var (score, entity) in SystemAPI.Query<RefRO<ScoreComponent>>().WithEntityAccess())
            {
                if (score.ValueRO.score_player1 == score.ValueRO.score_player2)
                {
                    ecb.AddComponent(entity, new WinnerComponent { value = (int)StateEndGame.NoPlayer });
                }else if(score.ValueRO.score_player1 > score.ValueRO.score_player2)
                {
                    ecb.AddComponent(entity, new WinnerComponent { value = (int)StateEndGame.Player1 });
                }
                else
                {
                    ecb.AddComponent(entity, new WinnerComponent { value = (int)StateEndGame.Player2 });
                }
            }
        }
        else if (isLosePlayer1)
        {
            foreach (var (score, entity) in SystemAPI.Query<RefRO<ScoreComponent>>().WithEntityAccess())
            {
                if (score.ValueRO.score_player1 <= score.ValueRO.score_player2)
                {
                    stateGame = (int)GameState.EndGame;
                    ecb.AddComponent(entity, new WinnerComponent { value = (int)StateEndGame.Player2 });
                }
            }
        }
        else if (isLosePlayer2)
        {
            foreach (var (score, entity) in SystemAPI.Query<RefRO<ScoreComponent>>().WithEntityAccess())
            {
                if (score.ValueRO.score_player2 <= score.ValueRO.score_player1)
                {
                    stateGame = (int)GameState.EndGame;
                    ecb.AddComponent(entity, new WinnerComponent { value = (int)StateEndGame.Player1 });
                }
            }
        }
        

        if (stateGame != gameStateComponent.state)
        {
            var newEntity = ecb.CreateEntity();
            ecb.AddComponent(newEntity, new ChangeGameStateComponent { newState = stateGame });
        }
        

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public bool isHaveMove(int2 position, MapComponent mapComponent)
    {
        int size = mapComponent.size;
        //check have move UP
        if (position.y - 1 >= 0 && mapComponent.maps[convertVectorToInt(new int2(position.x, position.y - 1), size)] == (int)ColorCell.Empty)
        {
            return true;
        }
        //check have move DOWN
        if (position.y + 1 < size && mapComponent.maps[convertVectorToInt(new int2(position.x, position.y + 1), size)] == (int)ColorCell.Empty)
        {
            return true;
        }
        //check have move LEFT
        if (position.x - 1 >= 0 && mapComponent.maps[convertVectorToInt(new int2(position.x - 1, position.y), size)] == (int)ColorCell.Empty)
        {
            return true;
        }
        //check have move RIGHT
        if (position.x + 1 < size && mapComponent.maps[convertVectorToInt(new int2(position.x + 1, position.y), size)] == (int)ColorCell.Empty)
        {
            return true;
        }
        return false;
    }

    int2 convertInttoVector(int cellID, int sizeMap)
    {
        int x = (int)math.floor(cellID / sizeMap);
        int y = cellID % sizeMap;
        return new int2(x, y);
    }

    int convertVectorToInt(int2 cellID, int sizeMap)
    {
        return cellID.x * sizeMap + cellID.y;
    }
}