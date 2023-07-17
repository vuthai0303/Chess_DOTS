using Unity.Collections;
using Unity.Entities;

public partial struct SpawnerComponent : IComponentData
{
    public Entity targetSpawn;
    public int targetID;
}
