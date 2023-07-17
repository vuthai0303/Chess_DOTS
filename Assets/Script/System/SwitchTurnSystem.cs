using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SwitchTurnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurnPlayComponent>();
        state.RequireForUpdate<SwitchTurnComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var turnPlayComponent in SystemAPI.Query<RefRW<TurnPlayComponent>>())
        {
            foreach (var (switchTurnComponent, entity) in SystemAPI.Query<RefRO<SwitchTurnComponent>>().WithEntityAccess())
            {
                turnPlayComponent.ValueRW.value = switchTurnComponent.ValueRO.newTurn;
                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}