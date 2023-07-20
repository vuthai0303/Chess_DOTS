using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateBefore(typeof(MovingSystem))]
public partial struct RandomMoveSystem : ISystem
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

        new RandomMoveJob
        { 
            ECB = ecb, 
            mapComponent = map,
            turnPlaycomponent = turnPlayComponent,
            mRandom = Unity.Mathematics.Random.CreateFromIndex((uint)System.DateTime.Now.TimeOfDay.TotalSeconds),
        }.Schedule();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

[BurstCompile]
public partial struct RandomMoveJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public MapComponent mapComponent;
    public TurnPlayComponent turnPlaycomponent;
    public Unity.Mathematics.Random mRandom;

    void Execute(RefRO<RandomMoveTag> randomMove, RefRO<PlayerComponent> playerComponent, Entity entity)
    {
        if(turnPlaycomponent.value != playerComponent.ValueRO.playerID)
        {
            return;
        }

        int2 myPosition = convertInttoVector(playerComponent.ValueRO.cellID);

        int nextMove = getRandomNextMove(myPosition, mapComponent.maps);
        if (nextMove == -1){
            return;
        }

        ECB.AddComponent(entity, new MoveComponent { direction = nextMove, speed = 10f, rangeMove = 0f });
    }

    int getRandomNextMove(int2 position, NativeArray<int> map)
    {
        FixedList32Bytes<int> arrNextMove = checkNextMove(position, map, mapComponent.size);
        if(arrNextMove.Length == 0)
        {
            return -1;
        }
        int random = mRandom.NextInt(0, arrNextMove.Length * 10);
        return arrNextMove[ (int)math.floor(random / 10)];
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