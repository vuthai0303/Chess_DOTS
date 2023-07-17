using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
[UpdateAfter(typeof(ControlMoveSystem))]
[UpdateBefore(typeof(RenderMapSystem))]
public partial struct UpdateMapSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapComponent>();
        state.RequireForUpdate<PlayerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var mapComponent in SystemAPI.Query<RefRW<MapComponent>>())
        {
            foreach (var playerComponent in SystemAPI.Query<RefRO<PlayerComponent>>())
            {
                if (playerComponent.ValueRO.playerID == (int)Player.Player1)
                {
                    mapComponent.ValueRW.maps[playerComponent.ValueRO.cellID] = (int)ColorCell.Player1;
                }
                else
                {
                    mapComponent.ValueRW.maps[playerComponent.ValueRO.cellID] = (int)ColorCell.Player2;
                }

            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}