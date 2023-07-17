using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(MovingSystem))]
public partial struct ControlMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var moveComponent in SystemAPI.Query<RefRO<MoveComponent>>())
        {
            return;
        }

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

        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        int direction_player1 = -1;
        int direction_player2 = -1;
        if(turnPlayComponent.value == (int)Player.Player1)
        {
            //setting input Player1
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction_player1 = (int)Direction.Up;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction_player1 = (int)Direction.Down;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                direction_player1 = (int)Direction.Left;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                direction_player1 = (int)Direction.Right;
            }
        }
        else
        {
            //setting input Player2
            if (Input.GetKeyDown(KeyCode.W))
            {
                direction_player2 = (int)Direction.Up;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                direction_player2 = (int)Direction.Down;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                direction_player2 = (int)Direction.Left;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                direction_player2 = (int)Direction.Right;
            }
        }

        new ControlMoveJob { 
            ECB = ecb, 
            mapComponent = map,
            turnPlaycomponent = turnPlayComponent,
            Horizontal = Horizontal,
            Vertical = Vertical,
            direction_player1 = direction_player1,
            direction_player2 = direction_player2,
        }.Schedule();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

//[BurstCompile]
public partial struct ControlMoveJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public MapComponent mapComponent;
    public TurnPlayComponent turnPlaycomponent;
    public float Horizontal;
    public float Vertical;
    public int direction_player1;
    public int direction_player2;

    void Execute(RefRO<ControlMoveTag> controlMove, RefRO<PlayerComponent> playerComponent, Entity entity)
    {
        if (turnPlaycomponent.value != playerComponent.ValueRO.playerID)
        {
            return;
        }
        if (turnPlaycomponent.value == (int)Player.Player1)
        {
            if (direction_player1 == (int)Direction.Right)
            {
                // go RIGHT
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Right))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player1, speed = 10f, rangeMove = 0f });
                }
            }
            else if (direction_player1 == (int)Direction.Left)
            {
                // go LEFT
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Left))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player1, speed = 10f, rangeMove = 0f });
                }
            }
            else if (direction_player1 == (int)Direction.Up)
            {
                //go UP
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Up))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player1, speed = 10f, rangeMove = 0f });
                }
            }
            else if (direction_player1 == (int)Direction.Down)
            {
                // go DOWN
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Down))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player1, speed = 10f, rangeMove = 0f });
                }
            }
        }
        else
        {
            if (direction_player2 == (int)Direction.Right)
            {
                // go RIGHT
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Right))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player2, speed = 10f, rangeMove = 0f });
                }
            }
            else if (direction_player2 == (int)Direction.Left)
            {
                // go LEFT
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Left))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player2, speed = 10f, rangeMove = 0f });
                }
            }
            else if (direction_player2 == (int)Direction.Up)
            {
                //go UP
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Up))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player2, speed = 10f, rangeMove = 0f });
                }
            }
            else if (direction_player2 == (int)Direction.Down)
            {
                // go DOWN
                if (isCheckValidMove(convertInttoVector(playerComponent.ValueRO.cellID), (int)Direction.Down))
                {
                    ECB.AddComponent(entity, new MoveComponent { direction = direction_player2, speed = 10f, rangeMove = 0f });
                }
            }
        }
    }

    bool isCheckValidMove(int2 cellPosition, int moveDirection)
    {
        int2 nextCell = cellPosition;
        
        switch (moveDirection)
        {
            case (int)Direction.Up:
                nextCell.y -= 1;
                if (nextCell.y < 0 || mapComponent.maps[convertVectorToInt(nextCell)] != (int)ColorCell.Empty)
                {
                    return false;
                }
                return true;

            case (int)Direction.Down:
                nextCell.y += 1;
                if (nextCell.y >= mapComponent.size || mapComponent.maps[convertVectorToInt(nextCell)] != (int)ColorCell.Empty)
                {
                    return false;
                }
                return true;

            case (int)Direction.Right:
                nextCell.x += 1;
                if (nextCell.x >= mapComponent.size || mapComponent.maps[convertVectorToInt(nextCell)] != (int)ColorCell.Empty)
                {
                    return false;
                }
                return true;

            case (int)Direction.Left:
                nextCell.x -= 1;
                if (nextCell.x < 0 || mapComponent.maps[convertVectorToInt(nextCell)] != (int)ColorCell.Empty)
                {
                    return false;
                }
                return true;
        }

        return false;
    }

    int2 convertInttoVector(int cellID)
    {
        int x = (int)math.floor(cellID / mapComponent.size);
        int y = cellID % mapComponent.size;
        return new int2(x, y);
    }

    int convertVectorToInt(int2 cellID)
    {
        return cellID.x * mapComponent.size + cellID.y;
    }

}