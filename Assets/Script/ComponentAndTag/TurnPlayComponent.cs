using Unity.Entities;

public partial struct TurnPlayComponent : IComponentData
{
    public int value;
}

public partial struct SwitchTurnComponent : IComponentData
{
    public int newTurn;
}