using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(ConfigMapSystem))]
public partial struct CreateMapSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        MapConfig mapConfig;
        var isMapConfig = SystemAPI.TryGetSingleton<MapConfig>(out mapConfig);
        if (!isMapConfig)
        {
            return;
        }

        foreach (var (mapComponent, e) in SystemAPI.Query<RefRW<MapComponent>>().WithEntityAccess())
        {
            if (mapComponent.ValueRO.isCreateMap)
            {
                return;
            }

            NativeHashMap<int, int> targetSpawn = createSpawn(mapComponent);
            float step = 1.5f;
            int size = mapComponent.ValueRO.size;
            float minH = (float)(step * (size - 1)) / 2f;
            float minV = (float)(step * (size - 1)) / 2f;
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    var entity = ecb.CreateEntity();
                    float3 position = new float3((float)(-minH + (r * step)), 0.5f, (float)(minV - (step * c)));
                    int cellID = r * size + c;
                    ecb.AddComponent(entity, new SpawnerCellComponent
                    {
                        cellID = cellID,
                        position = position,
                        cellPrefab = mapConfig.cell
                    });

                    if (targetSpawn.ContainsKey(cellID))
                    {
                        var newEntity = ecb.CreateEntity();
                        switch (targetSpawn[cellID])
                        {
                            case (int)TargetSpawn.Player1:
                                mapComponent.ValueRW.maps[cellID] = (int)ColorCell.Player1;
                                ecb.AddComponent(newEntity, new SpawnerObjectComponent
                                {
                                    targetSpawn = mapConfig.Player1,
                                    targetID = (int)TargetSpawn.Player1,
                                    cellID = cellID,
                                    position = position,
                                });
                                break;
                            case (int)TargetSpawn.Player2:
                                mapComponent.ValueRW.maps[cellID] = (int)ColorCell.Player2;
                                ecb.AddComponent(newEntity, new SpawnerObjectComponent
                                {
                                    targetSpawn = mapConfig.Player2,
                                    targetID = (int)TargetSpawn.Player2,
                                    cellID = cellID,
                                    position = position,
                                });
                                break;
                            case (int)TargetSpawn.Wall:
                                mapComponent.ValueRW.maps[cellID] = (int)ColorCell.Wall;
                                ecb.AddComponent(newEntity, new SpawnerObjectComponent
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
            }

            ecb.AddComponent(e, new ScoreComponent { score_player1 = 1, score_player2 = 1 });
            //random player is first
            Random rand = Random.CreateFromIndex((uint)System.DateTime.Now.TimeOfDay.TotalSeconds);
            ecb.AddComponent(e, new TurnPlayComponent
            {
                value = rand.NextInt(0, 10) >= 5 ? (int)Player.Player1 : (int)Player.Player2
            });
            mapComponent.ValueRW.isCreateMap = true;
        }

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    NativeHashMap<int, int> createSpawn(RefRW<MapComponent> mapComponent)
    {
        int size = mapComponent.ValueRO.size;
        int numWallOfPlayer = mapComponent.ValueRO.numWallOfPlayer;
        var targetSpawn = new NativeHashMap<int, int>(numWallOfPlayer * 2 + 2, Allocator.Temp);
        Random rand = Random.CreateFromIndex((uint)System.DateTime.Now.TimeOfDay.TotalSeconds);

        ////add spawn player
        targetSpawn.Add(0, (int)TargetSpawn.Player2);
        targetSpawn.Add(size * size - 1, (int)TargetSpawn.Player1);

        if (numWallOfPlayer > 10)
        {
            return targetSpawn;
        }
        //add spawn random wall
        for (int i = 0; i < numWallOfPlayer; i++)
        {
            int cellID = rand.NextInt(0, (int)math.ceil((size * size) / 2f));
            while (targetSpawn.ContainsKey(cellID))
            {
                cellID = rand.NextInt(0, size * size);
            }
            targetSpawn.Add(cellID, (int)TargetSpawn.Wall);
            targetSpawn.Add((size * size - 1) - cellID, (int)TargetSpawn.Wall);
        }

        return targetSpawn;
    }
}