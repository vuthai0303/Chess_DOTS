using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class CellAuthoring : MonoBehaviour
{
    public Material Player1_material;
    public Material Player2_material;
}

public class CellAuthoringBake : Baker<CellAuthoring>
{
    Dictionary<Material, BatchMaterialID> m_MaterialMapping; 

    public override void Bake(CellAuthoring authoring)
    {
        m_MaterialMapping = new Dictionary<Material, BatchMaterialID>();
        Entity e = GetEntity(TransformUsageFlags.Dynamic);
        BatchMaterialID player1 = registerMaterial(authoring.Player1_material);
        BatchMaterialID player2 = registerMaterial(authoring.Player2_material);
        AddComponent(e, new MaterialMeshConfig
        {
            Player1_material = player1,
            Player2_material = player2,
        });
    }

    public BatchMaterialID registerMaterial (Material material)
    {
        var entityGraphicSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
        if (!m_MaterialMapping.ContainsKey(material))
        {
            m_MaterialMapping.Add(material, entityGraphicSystem.RegisterMaterial(material));
        }
        return m_MaterialMapping[material];
    }

}
