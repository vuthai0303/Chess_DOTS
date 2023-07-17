using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class MapAuthoring : MonoBehaviour
{
    public int size = 10;
    public int numWallOfPlayer = 2;
    public GameObject cellPrefab;
    public GameObject Player1;
    public GameObject Player2;
    public GameObject Wall;
}

public class MapAuthoringBake : Baker<MapAuthoring>
{
    public override void Bake(MapAuthoring authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.None);
        var maps = new FixedList512Bytes<int>();
        for (int i = 0; i < authoring.size * authoring.size; i++)
        {
            maps.Add((int)ColorCell.Empty);
        }

        AddComponent(e, new MapComponent
        {
            cell = GetEntity(authoring.cellPrefab, TransformUsageFlags.Dynamic),
            size = authoring.size,
            maps = maps,
            numWallOfPlayer = authoring.numWallOfPlayer,
            Player1 = GetEntity(authoring.Player1, TransformUsageFlags.Dynamic),
            Player2 = GetEntity(authoring.Player2, TransformUsageFlags.Dynamic),
            Wall = GetEntity(authoring.Wall, TransformUsageFlags.None),
            isCreateMap = false,
        });
        AddComponent(e, new GameStateComponent { state = (int)GameState.Menu });
    }
}
