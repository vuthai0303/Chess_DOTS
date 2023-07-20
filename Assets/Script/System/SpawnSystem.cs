using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurnPlayComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        TurnPlayComponent turnPlayComponent;
        var isTurnPlayComponent = SystemAPI.TryGetSingleton<TurnPlayComponent>(out turnPlayComponent);
        if (!isTurnPlayComponent)
        {
            return;
        }

        MapComponent mapComponent;
        var isMapComponent = SystemAPI.TryGetSingleton<MapComponent>(out mapComponent);
        if (!isMapComponent)
        {
            return;
        }

        MapConfig mapConfig;
        var isMapConfig = SystemAPI.TryGetSingleton<MapConfig>(out mapConfig);
        if (!isMapConfig)
        {
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        new SpawnCellJob { ECB = ecb, mapComponent = mapComponent, mapConfig = mapConfig}.Schedule();
        
        new SpawnJob { ECB = ecb, TurnPlayComponent = turnPlayComponent }.Schedule();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

[BurstCompile]
public partial struct SpawnJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public TurnPlayComponent TurnPlayComponent;

    void Execute(RefRO<SpawnerObjectComponent> spawner, Entity entity)
    {
        var newEntity = ECB.Instantiate(spawner.ValueRO.targetSpawn);
        var position = spawner.ValueRO.position;
        var rotation = quaternion.identity;
        var scale = 1f;
        if(spawner.ValueRO.targetID == (int)TargetSpawn.Player1)
        {
            position.y = 1f;
            rotation = quaternion.LookRotation(new float3(0f, 0f, 1f), new float3(0f, -1f, 0f));
            scale = 1.2f;

            ECB.AddComponent(newEntity, new PlayerComponent { 
                cellID = spawner.ValueRO.cellID,
                playerID = (int)Player.Player1,
            });

            //set type moving
            ECB.AddComponent<ControlMoveTag>(newEntity);
            //ECB.AddComponent<AIMoveTag>(newEntity);

        }
        else if(spawner.ValueRO.targetID == (int)TargetSpawn.Player2)
        {
            position.y = 1f;
            rotation = quaternion.LookRotation(new float3(0f, 0f, 1f), new float3(0f, -1f, 0f));
            scale = 1.2f;

            ECB.AddComponent(newEntity, new PlayerComponent
            {
                cellID = spawner.ValueRO.cellID,
                playerID = (int)Player.Player2,
            });

            //set type moving
            ECB.AddComponent<AIMoveTag>(newEntity);

        }
        else
        {
            ECB.AddComponent<WallComponent>(newEntity);
        }
        ECB.SetComponent(newEntity, new LocalTransform
        {
            Position = position,
            Rotation = rotation,
            Scale = scale,
        });

        //ECB.RemoveComponent<SpawnerComponent>(entity);
        ECB.DestroyEntity(entity);
    }
}

//[BurstCompile]
public partial struct SpawnCellJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public MapComponent mapComponent;
    public MapConfig mapConfig;

    void Execute(RefRO<SpawnerCellComponent> spawner, Entity entity)
    {
        var newEntity = ECB.Instantiate(spawner.ValueRO.cellPrefab);
        float3 position = spawner.ValueRO.position;
        int cellID = spawner.ValueRO.cellID;
        ECB.SetComponent(newEntity, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f,
        });
        //UnityEngine.Debug.Log(spawner.ValueRO.cellID);
        ECB.AddComponent(newEntity, new CellComponent { cellID = cellID, value = (int)ColorCell.Empty });

        ECB.DestroyEntity(entity);
    }
}