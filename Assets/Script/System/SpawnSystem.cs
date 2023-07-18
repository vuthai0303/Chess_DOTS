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
        state.RequireForUpdate<SpawnerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        TurnPlayComponent turnPlayComponent;
        var isTurnPlayComponent = SystemAPI.TryGetSingleton<TurnPlayComponent>(out turnPlayComponent);
        if (!isTurnPlayComponent)
        {
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        
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

    void Execute(RefRO<SpawnerComponent> spawner, RefRO<LocalTransform> localTransform, RefRO<CellComponent> cellComponent, Entity entity)
    {
        var newEntity = ECB.Instantiate(spawner.ValueRO.targetSpawn);
        var position = localTransform.ValueRO.Position;
        var rotation = quaternion.identity;
        var scale = 1f;
        if(spawner.ValueRO.targetID == (int)TargetSpawn.Player1)
        {
            position.y = 1f;
            rotation = quaternion.LookRotation(new float3(0f, 0f, 1f), new float3(0f, -1f, 0f));
            scale = 1.2f;

            ECB.AddComponent(newEntity, new PlayerComponent { 
                cellID = cellComponent.ValueRO.cellID,
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
                cellID = cellComponent.ValueRO.cellID,
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

        ECB.RemoveComponent<SpawnerComponent>(entity);
    }
}