using Unity.Collections;
using Unity.Entities;

public partial struct MapComponent : IComponentData
{
    public int size;
    //public NativeArray<int> maps;
    public FixedList512Bytes<int> maps;
    public Entity cell;
    public Entity Player1;
    public Entity Player2;
    public Entity Wall;
    public int numWallOfPlayer;
    public bool isCreateMap;
}
