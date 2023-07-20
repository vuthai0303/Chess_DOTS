using Unity.Entities;
using Unity.Mathematics;

public partial struct SpawnerObjectComponent : IComponentData
{
    public Entity targetSpawn;
    public float3 position;
    public int targetID;
    public int cellID;
}

public partial struct SpawnerCellComponent : IComponentData
{
    public Entity cellPrefab;
    public float3 position;
    public int cellID;
}
