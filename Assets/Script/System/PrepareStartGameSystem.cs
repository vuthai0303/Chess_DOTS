using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct PrepareStartGameSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateMessage>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var stateGameMessage in SystemAPI.Query<RefRO<GameStateMessage>>())
        {
            if(stateGameMessage.ValueRO.state == (int)GameState.Menu || stateGameMessage.ValueRO.state == (int)GameState.RestartGame)
            {
                // prepare start game
                new DestroyMapJob { ECB = ecb }.Schedule();
                new DestroyPlayerJob { ECB = ecb }.Schedule();
                new DestroyWallJob { ECB = ecb }.Schedule();
                new resetMapJob { ECB = ecb }.Schedule();
            }
        }

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

[BurstCompile]
public partial struct DestroyMapJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    void Execute(RefRO<CellComponent> cellComponent, Entity e)
    {
        ECB.DestroyEntity(e);
    }
}

[BurstCompile]
public partial struct DestroyPlayerJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    void Execute(RefRO<PlayerComponent> playerComponent, Entity e)
    {
        ECB.DestroyEntity(e);
    }
}

[BurstCompile]
public partial struct DestroyWallJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    void Execute(RefRO<WallComponent> wallComponent, Entity e)
    {
        ECB.DestroyEntity(e);
    }
}

[BurstCompile]
public partial struct resetMapJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    void Execute(RefRW<MapComponent> mapComponent, Entity e)
    {
        mapComponent.ValueRW.isCreateMap = false;
        for (int i = 0; i < mapComponent.ValueRO.maps.Length; i++)
        {
            mapComponent.ValueRW.maps[i] = (int)ColorCell.Empty;
        }
        ECB.RemoveComponent<ScoreComponent>(e);
        ECB.RemoveComponent<TurnPlayComponent>(e);
        ECB.RemoveComponent<WinnerComponent>(e);
    }
}