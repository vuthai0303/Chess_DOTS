using Unity.Collections;
using Unity.Entities;

public partial struct CellComponent : IComponentData
{
    public int cellID;
    public int value;
}
