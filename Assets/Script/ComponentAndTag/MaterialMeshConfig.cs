using Unity.Collections;
using Unity.Entities;
using UnityEngine.Rendering;

public partial struct MaterialMeshConfig : IComponentData
{
    public BatchMaterialID Player1_material;
    public BatchMaterialID Player2_material;
}
