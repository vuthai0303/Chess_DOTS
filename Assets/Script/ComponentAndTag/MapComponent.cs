using Unity.Collections;
using Unity.Entities;

public partial struct MapConfig : IComponentData
{
    public int size;
    public int numWallOfPlayer;
    public Entity cell;
    public Entity Player1;
    public Entity Player2;
    public Entity Wall;
}

public partial struct MapComponent : IComponentData
{
    public int size;
    public NativeArray<int> maps;
    public int numWallOfPlayer;
    public bool isCreateMap;
}
