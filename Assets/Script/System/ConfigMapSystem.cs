using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct ConfigMapSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var mapComponent in SystemAPI.Query<RefRW<MapComponent>>())
        {
            return;
        }

        foreach (var (mapConfig, e) in SystemAPI.Query<RefRW<MapConfig>>().WithEntityAccess())
        {
            var map = new NativeArray<int>(mapConfig.ValueRO.size * mapConfig.ValueRO.size, Allocator.Persistent);
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = (int)ColorCell.Empty;
            }

            ecb.AddComponent(e, new MapComponent
            {
                size = mapConfig.ValueRO.size,
                maps = map,
                numWallOfPlayer = mapConfig.ValueRO.numWallOfPlayer,
                isCreateMap = false,
            });
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
