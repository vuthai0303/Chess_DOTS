using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(CreateMapSystem))]
public partial struct CreateSpawnObjectSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        MapConfig mapConfig;
        var isMapConfig = SystemAPI.TryGetSingleton<MapConfig>(out mapConfig);
        if (!isMapConfig)
        {
            return;
        }

        foreach (var mapComponent in SystemAPI.Query<RefRW<MapComponent>>())
        {
            if (!mapComponent.ValueRO.isCreateMap || mapComponent.ValueRO.isCreateObject)
            {
                return;
            }

            int size = mapComponent.ValueRO.size;
            float step = 1.5f;
            float minH = (float)(step * (size - 1)) / 2f;
            float minV = (float)(step * (size - 1)) / 2f;
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    var entity = ecb.CreateEntity();
                    float3 position = new float3((float)(-minH + (r * step)), 0.5f, (float)(minV - (step * c)));
                    int cellID = r * size + c;

                    switch (mapComponent.ValueRO.maps[cellID])
                    {
                        case (int)TargetSpawn.Player1:
                            ecb.AddComponent(entity, new SpawnerObjectComponent
                            {
                                targetSpawn = mapConfig.Player1,
                                targetID = (int)TargetSpawn.Player1,
                                cellID = cellID,
                                position = position,
                            });
                            break;
                        case (int)TargetSpawn.Player2:
                            ecb.AddComponent(entity, new SpawnerObjectComponent
                            {
                                targetSpawn = mapConfig.Player2,
                                targetID = (int)TargetSpawn.Player2,
                                cellID = cellID,
                                position = position,
                            });
                            break;
                        case (int)TargetSpawn.Wall:
                            ecb.AddComponent(entity, new SpawnerObjectComponent
                            {
                                targetSpawn = mapConfig.Wall,
                                targetID = (int)TargetSpawn.Wall,
                                cellID = cellID,
                                position = position,
                            });
                            break;
                    }
                }
            }
            mapComponent.ValueRW.isCreateObject = true;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}