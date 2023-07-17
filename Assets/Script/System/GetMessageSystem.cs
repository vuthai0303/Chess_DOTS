using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[DisableAutoCreation]
[BurstCompile]
public partial struct GetMessageSystem : ISystem
{
    public void onCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateMessage>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var stateGameMessage in SystemAPI.Query<RefRO<GameStateMessage>>())
        {
            foreach (var stateGameComponent in SystemAPI.Query<RefRW<GameStateComponent>>())
            {
                stateGameComponent.ValueRW.state = stateGameMessage.ValueRO.state;
            }
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (changeStateGameComponent, entity) in SystemAPI.Query<RefRO<ChangeGameStateComponent>>().WithEntityAccess())
        {
            foreach (var stateGameComponent in SystemAPI.Query<RefRO<GameStateComponent>>())
            {
                if (changeStateGameComponent.ValueRO.newState == stateGameComponent.ValueRO.state)
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}