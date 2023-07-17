using Unity.Entities;

public partial struct MoveComponent : IComponentData
{
    public int direction;
    public float rangeMove;
    public float speed;
}

public partial struct ControlMoveTag : IComponentData
{
}

public partial struct AIMoveTag : IComponentData
{
}