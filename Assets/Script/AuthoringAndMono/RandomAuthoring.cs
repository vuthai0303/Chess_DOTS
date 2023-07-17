using Unity.Entities;
using UnityEngine;

public class RamdomAuthoring : MonoBehaviour
{
    
}

public class RandomAuthoringBake : Baker<MapAuthoring>
{
    public override void Bake(MapAuthoring authoring)
    {
        Entity e = GetEntity(TransformUsageFlags.None);
        Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)Random.Range(1, 100000));

        AddComponent(e, new RandomComponent
        {
            mRandom = random,
        });
    }
}
