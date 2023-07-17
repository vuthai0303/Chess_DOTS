using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct CreateMapSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (mapComponent, e) in SystemAPI.Query<RefRW<MapComponent>>().WithEntityAccess())
        {
            if(mapComponent.ValueRO.isCreateMap)
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
                    var entity = ecb.Instantiate(mapComponent.ValueRO.cell);
                    float3 position = new float3((float)(-minH + (r * step)), 0.5f, (float)(minV - (step * c)));
                    int cellID = r * size + c;
                    ecb.SetComponent(entity, new LocalTransform
                    {
                        Position = position,
                        Rotation = quaternion.identity,
                        Scale = 1f,
                    });
                    ecb.AddComponent(entity, new CellComponent { cellID =cellID, value = (int)ColorCell.Empty });
                    if (targetSpawn.ContainsKey(cellID))
                    {
                        switch (targetSpawn[cellID])
                        {
                            case (int)TargetSpawn.Player1:
                                mapComponent.ValueRW.maps[cellID] = (int)ColorCell.Player1;
                                ecb.AddComponent(entity, new SpawnerComponent
                                {
                                    targetSpawn = mapComponent.ValueRO.Player1,
                                    targetID = (int)TargetSpawn.Player1
                                });
                                break;
                            case (int)TargetSpawn.Player2:
                                mapComponent.ValueRW.maps[cellID] = (int)ColorCell.Player2;
                                ecb.AddComponent(entity, new SpawnerComponent
                                {
                                    targetSpawn = mapComponent.ValueRO.Player2,
                                    targetID = (int)TargetSpawn.Player2
                                });
                                break;
                            case (int)TargetSpawn.Wall:
                                mapComponent.ValueRW.maps[cellID] = (int)ColorCell.Wall;
                                ecb.AddComponent(entity, new SpawnerComponent
                                {
                                    targetSpawn = mapComponent.ValueRO.Wall,
                                    targetID = (int)TargetSpawn.Wall
                                });
                                break;
                        }
                    }
                }
            }

            ecb.AddComponent(e, new ScoreComponent { score_player1 = 1, score_player2 = 1 });
            //random player is first
            Random rand = new Random(10);
            ecb.AddComponent(e, new TurnPlayComponent
            {
                value = rand.NextInt(1, 3) == 1 ? (int)Player.Player1 : (int)Player.Player2
            });
            mapComponent.ValueRW.isCreateMap = true;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    

    NativeHashMap<int, int> createSpawn(RefRW<MapComponent> mapComponent)
    {
        int size = mapComponent.ValueRO.size;
        int numWallOfPlayer = mapComponent.ValueRO.numWallOfPlayer;
        NativeHashMap<int, int> targetSpawn = new NativeHashMap<int, int>(numWallOfPlayer * 2 + 2, Allocator.Temp);
        Random rand = new Random(10);

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
