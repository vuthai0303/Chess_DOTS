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

        AddComponent(e, new MapConfig
        {
            cell = GetEntity(authoring.cellPrefab, TransformUsageFlags.Dynamic),
            size = authoring.size,
            numWallOfPlayer = authoring.numWallOfPlayer,
            Player1 = GetEntity(authoring.Player1, TransformUsageFlags.Dynamic),
            Player2 = GetEntity(authoring.Player2, TransformUsageFlags.Dynamic),
            Wall = GetEntity(authoring.Wall, TransformUsageFlags.None),
        });
        AddComponent(e, new GameStateComponent { state = (int)GameState.Menu });
    }
}
