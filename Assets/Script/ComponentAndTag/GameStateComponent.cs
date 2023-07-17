using Unity.Entities;

public partial struct GameStateComponent : IComponentData
{
    public int state;
}

public partial struct ChangeGameStateComponent : IComponentData
{
    public int newState;
}