using System.Diagnostics;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateBefore(typeof(MovingSystem))]
public partial struct ALMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var moveComponent in SystemAPI.Query<RefRO<MoveComponent>>())
        {
            return;
        }

        MapComponent map;
        var isMapComponent = SystemAPI.TryGetSingleton<MapComponent>(out map);
        if (!isMapComponent)
        {
            return;
        }

        TurnPlayComponent turnPlayComponent;
        var isTurnPlayComponent = SystemAPI.TryGetSingleton<TurnPlayComponent>(out turnPlayComponent);
        if (!isTurnPlayComponent)
        {
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        int player1_cellId = 99;
        int player2_cellId = 0;

        foreach(var playerComponent in SystemAPI.Query<RefRO<PlayerComponent>>())
        {
            if(playerComponent.ValueRO.playerID == (int)Player.Player1)
            {
                player1_cellId = playerComponent.ValueRO.cellID;
            }
            else
            {
                player2_cellId = playerComponent.ValueRO.cellID;
            }
        }

        new AlMoveJob
        { 
            ECB = ecb, 
            mapComponent = map,
            turnPlaycomponent = turnPlayComponent,
            mRandom = Unity.Mathematics.Random.CreateFromIndex((uint)System.DateTime.Now.TimeOfDay.TotalSeconds),
            player1_cellId = player1_cellId,
            player2_cellId = player2_cellId,
        }.Schedule();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

//[BurstCompile]
public partial struct AlMoveJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public MapComponent mapComponent;
    public TurnPlayComponent turnPlaycomponent;
    public Unity.Mathematics.Random mRandom;
    public int player1_cellId;
    public int player2_cellId;

    void Execute(RefRO<AIMoveTag> ALMove, RefRO<PlayerComponent> playerComponent, Entity entity)
    {
        if(turnPlaycomponent.value != playerComponent.ValueRO.playerID)
        {
            return;
        }

        int2 myPosition;
        int2 yourPosition;
        if (playerComponent.ValueRO.playerID == (int)Player.Player1)
        {
            myPosition = convertInttoVector(player1_cellId);
            yourPosition = convertInttoVector(player2_cellId);
        }
        else
        {
            myPosition = convertInttoVector(player2_cellId);
            yourPosition = convertInttoVector(player1_cellId);
        }

        var map = new NativeArray<int>(mapComponent.maps.Length, Allocator.Temp);
        mapComponent.maps.CopyTo(map);

        //int nextMove = getRandomNextMove(myPosition, map);
        int nextMove = getMiniMaxMove(myPosition, yourPosition, map);
        if(nextMove == -1){
            return;
        }

        ECB.AddComponent(entity, new MoveComponent { direction = nextMove, speed = 10f, rangeMove = 0f });
        map.Dispose();

    }

    int getRandomNextMove(int2 position, NativeArray<int> map)
    {
        FixedList32Bytes<int> arrNextMove = checkNextMove(position, map, mapComponent.size);
        if(arrNextMove.Length == 0)
        {
            return -1;
        }
        int cellID = convertVectorToInt(position);
        int random = mRandom.NextInt(0, arrNextMove.Length * 10);
        return arrNextMove[ (int)math.floor(random / 10)];
    }

    int getMiniMaxMove(int2 myPosition, int2 yourPosition, NativeArray<int> map)
    {
        FixedList32Bytes<int> arrNextMove = checkNextMove(myPosition, map, mapComponent.size);
        if (arrNextMove.Length == 0)
        {
            return -1;
        }

        int max = -map.Length * map.Length; ;
        int dir_move = -1;
        foreach(int direction in arrNextMove)
        {
            var cloneMap = new NativeArray<int>(map.Length, Allocator.Temp);
            map.CopyTo(cloneMap);
            var nextMove = moveTemp(myPosition, direction);
            cloneMap[convertVectorToInt(nextMove)] = cloneMap[convertVectorToInt(myPosition)];
            int score = CalculatorMiniMaxMove(yourPosition, nextMove, cloneMap, false, 12);
            //UnityEngine.Debug.Log(nextMove);
            //UnityEngine.Debug.Log(score);
            cloneMap.Dispose();
            if (score > max)
            {
                max = score;
                dir_move = direction;
            }
        }

        return dir_move;
    }

    int CalculatorMiniMaxMove(int2 myPosition, int2 yourPosition, NativeArray<int> map, bool isMaxPlayer, int depth)
    {
        FixedList32Bytes<int> arrNextMove = checkNextMove(myPosition, map, mapComponent.size);
        if (arrNextMove.Length == 0 || depth <= 0)
        {
            int score = 0;
            if(depth <= 0)
            {
                score = getScoreInMap(yourPosition, map, isMaxPlayer, true) + arrNextMove.Length + mRandom.NextInt(0, 2);
            }
            else
            {
                score = getScoreInMap(yourPosition, map, isMaxPlayer, false);
            }
            return score;
        }

        if (isMaxPlayer)
        {
            int max = -map.Length * map.Length;
            for (int i = 0; i < arrNextMove.Length; i++)
            {
                var cloneMap = new NativeArray<int>(map.Length, Allocator.Temp);
                map.CopyTo(cloneMap);
                var nextMove = moveTemp(myPosition, arrNextMove[i]);
                cloneMap[convertVectorToInt(nextMove)] = cloneMap[convertVectorToInt(myPosition)];
                int score = CalculatorMiniMaxMove(yourPosition ,nextMove, cloneMap, false, depth - 1);
                cloneMap.Dispose();
                if (score > max) { 
                    max = score;
                }
            }

            return max;
        }
        else
        {
            int min = map.Length * map.Length;
            for (int i = 0; i < arrNextMove.Length; i++)
            {
                var cloneMap = new NativeArray<int>(map.Length, Allocator.Temp);
                map.CopyTo(cloneMap);

                var nextMove = moveTemp(myPosition, arrNextMove[i]);
                cloneMap[convertVectorToInt(nextMove)] = cloneMap[convertVectorToInt(myPosition)];

                int score = CalculatorMiniMaxMove(yourPosition, nextMove, cloneMap, true, depth - 1);
                cloneMap.Dispose();
                if (score < min)
                {
                    min = score;
                }
            }

            return min;
        }
    }

    int2 moveTemp(int2 position, int direction)
    {
        if(direction == (int)Direction.Up)
        {
            return new int2(position.x, position.y - 1);
        }else if(direction == (int)Direction.Down)
        {
            return new int2(position.x, position.y + 1);
        }else if (direction == (int)Direction.Left)
        {
            return new int2(position.x - 1, position.y);
        }
        else
        {
            return new int2(position.x + 1, position.y);
        }
    }

    int getScoreInMap(int2 yourPosition, NativeArray<int> map, bool isMaxPlayer, bool isDepth)
    {
        int size = mapComponent.size;
        int score_player1 = 0;
        int score_player2 = 0;

        bool isPlayer1 = true;
        if (map[convertVectorToInt(yourPosition)] == (int)ColorCell.Player1)
        {
            isPlayer1 = false;
        }

        foreach (var cell in map)
        {
            if (cell == (int)ColorCell.Player1)
            {
                score_player1++;
            }
            if (cell == (int)ColorCell.Player2)
            {
                score_player2++;
            }
        }
        if (isDepth) {
            if (isPlayer1)
            {
                return score_player1;
            }
            else {
                return score_player2;
            }
        }

        var lstYourMextMove = checkNextMove(yourPosition, map, size);
        if (lstYourMextMove.Length == 0)
        {
            if (isPlayer1)
            {
                if (score_player1 > score_player2)
                {
                    return size * size * 10 - score_player1;
                }
                else if (score_player1 == score_player2)
                {
                    return score_player1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (score_player2 > score_player1)
                {
                    return size * size * 10 - score_player2;
                }
                else if (score_player2 == score_player1)
                {
                    return score_player2;
                }
                else
                {
                    return 0;
                }
            }
        }

        if (isMaxPlayer)
        {
            if (isPlayer1)
            {
                if (score_player1 > score_player2 + 1)
                {
                    return size * size * 10 - score_player1;
                }
                else if (score_player1 == score_player2 + 1)
                {
                    return score_player1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (score_player2 > score_player1 + 1)
                {
                    return size * size * 10 - score_player2;
                }
                else if (score_player2 == score_player1 + 1)
                {
                    return score_player2;
                }
                else
                {
                    return 0;
                }
            }
        }
        else
        {
            if (isPlayer1)
            {
                if (score_player1 + 1 > score_player2)
                {
                    return size * size * 10 - score_player1;
                }
                else if (score_player1 + 1 == score_player2)
                {
                    return score_player1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (score_player2 + 1 > score_player1)
                {
                    return size * size * 10 - score_player2;
                }
                else if (score_player2 + 1 == score_player1)
                {
                    return score_player2;
                }
                else
                {
                    return 0;
                }
            }
        }
    }

    FixedList32Bytes<int> checkNextMove(int2 position, NativeArray<int> map, int size)
    {
        FixedList32Bytes<int> allMove = new FixedList32Bytes<int>();

        //check have move UP
        if (position.y - 1 >= 0 && map[convertVectorToInt(new int2(position.x, position.y - 1))] == (int)ColorCell.Empty)
        {
            allMove.Add((int)Direction.Up);
        }
        //check have move DOWN
        if (position.y + 1 < size && map[convertVectorToInt(new int2(position.x, position.y + 1))] == (int)ColorCell.Empty)
        {
            allMove.Add((int)Direction.Down);
        }
        //check have move LEFT
        if (position.x - 1 >= 0 && map[convertVectorToInt(new int2(position.x - 1, position.y))] == (int)ColorCell.Empty)
        {
            allMove.Add((int)Direction.Left);
        }
        //check have move RIGHT
        if (position.x + 1 < size && map[convertVectorToInt(new int2(position.x + 1, position.y))] == (int)ColorCell.Empty)
        {
            allMove.Add((int)Direction.Right);
        }

        return allMove;
    }

    int2 convertInttoVector(int cellID)
    {
        int x = (int)math.floor(cellID / mapComponent.size);
        int y = cellID % mapComponent.size;
        return new int2(x, y);
    }

    int convertVectorToInt(int2 cellID)
    {
        return cellID.x * mapComponent.size + cellID.y;
    }

}