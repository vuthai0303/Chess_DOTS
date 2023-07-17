using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct MovingSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapComponent>();
        state.RequireForUpdate<TurnPlayComponent>();
        state.RequireForUpdate<MoveComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        MapComponent map;
        var isMapComponent = SystemAPI.TryGetSingleton<MapComponent>(out map);
        if (!isMapComponent)
        {
            return;
        }

        TurnPlayComponent turnPlayComponent;
        var isTurnPlayComponent = SystemAPI.TryGetSingleton<TurnPlayComponent>(out turnPlayComponent);
        if (!isTurnPlayComponent)
        {
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        new MovingJob { ECB = ecb, mapComponent = map , turnPlayComponent = turnPlayComponent }.Schedule();
        state.Dependency.Complete();

        

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

[BurstCompile]
public partial struct MovingJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public MapComponent mapComponent;
    public TurnPlayComponent turnPlayComponent;

    void Execute(RefRW<MoveComponent> move, RefRW<PlayerComponent> playerComponent, RefRW<LocalTransform> localTranform ,Entity entity)
    {
        if (turnPlayComponent.value != playerComponent.ValueRO.playerID)
        {
            return;
        }

        float3 direction;
        float3 movement;
        float deltaTime = 0.01f;
        switch (move.ValueRO.direction)
        {
            case (int) Direction.Up:
                direction = new float3(0f, 0f, 1f);
                movement = direction * move.ValueRO.speed * deltaTime;
                localTranform.ValueRW.Position += movement;
                move.ValueRW.rangeMove += math.abs(movement.z);
                if (move.ValueRO.rangeMove > 1.5f)
                {
                    localTranform.ValueRW.Position.z -= (move.ValueRO.rangeMove - 1.5f);
                    var newEntity = ECB.CreateEntity();
                    ECB.AddComponent(newEntity, new SwitchTurnComponent { newTurn = playerComponent.ValueRO.playerID == (int)Player.Player1 ? (int)Player.Player2 : (int)Player.Player1 });
                    playerComponent.ValueRW.cellID -= 1;
                    ECB.RemoveComponent<MoveComponent>(entity);
                    return;
                }
                break;
            case (int) Direction.Down:
                direction = new float3(0f, 0f, -1f);
                movement = direction * move.ValueRO.speed * deltaTime;
                localTranform.ValueRW.Position += movement;
                move.ValueRW.rangeMove += math.abs(movement.z);
                if (move.ValueRO.rangeMove > 1.5f)
                {
                    localTranform.ValueRW.Position.z += (move.ValueRO.rangeMove - 1.5f);
                    var newEntity = ECB.CreateEntity();
                    ECB.AddComponent(newEntity, new SwitchTurnComponent { newTurn = playerComponent.ValueRO.playerID == (int)Player.Player1 ? (int)Player.Player2 : (int)Player.Player1 });
                    playerComponent.ValueRW.cellID += 1;
                    ECB.RemoveComponent<MoveComponent>(entity);
                    return;
                }
                break;
            case (int)Direction.Right:
                direction = new float3(1f, 0f, 0f);
                movement = direction * move.ValueRO.speed * deltaTime;
                localTranform.ValueRW.Position += movement;
                move.ValueRW.rangeMove += math.abs(movement.x);
                if (move.ValueRO.rangeMove > 1.5f)
                {
                    localTranform.ValueRW.Position.x -= (move.ValueRO.rangeMove - 1.5f);
                    var newEntity = ECB.CreateEntity();
                    ECB.AddComponent(newEntity, new SwitchTurnComponent { newTurn = playerComponent.ValueRO.playerID == (int)Player.Player1 ? (int)Player.Player2 : (int)Player.Player1 });
                    playerComponent.ValueRW.cellID += mapComponent.size;
                    ECB.RemoveComponent<MoveComponent>(entity);
                    return;
                }
                break;
            case (int)Direction.Left:
                direction = new float3(-1f, 0f, 0f);
                movement = direction * move.ValueRO.speed * deltaTime;
                localTranform.ValueRW.Position += movement;
                move.ValueRW.rangeMove += math.abs(movement.x);
                if (move.ValueRO.rangeMove > 1.5f)
                {
                    localTranform.ValueRW.Position.x += (move.ValueRO.rangeMove - 1.5f);
                    var newEntity = ECB.CreateEntity();
                    ECB.AddComponent(newEntity, new SwitchTurnComponent { newTurn = playerComponent.ValueRO.playerID == (int)Player.Player1 ? (int)Player.Player2 : (int)Player.Player1 });
                    playerComponent.ValueRW.cellID -= mapComponent.size;
                    ECB.RemoveComponent<MoveComponent>(entity);
                    return;
                }
                break;
        }
    }
}