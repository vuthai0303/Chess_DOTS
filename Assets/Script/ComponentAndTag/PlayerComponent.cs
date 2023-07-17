using Unity.Entities;

public partial struct PlayerComponent : IComponentData
{
    public int cellID;
    public int playerID;
}