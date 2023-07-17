using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[BurstCompile]
public partial struct RenderMapSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapComponent>();
        state.RequireForUpdate<CellComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        MapComponent map;
        var isMapComponent = SystemAPI.TryGetSingleton<MapComponent>(out map);
        if (!isMapComponent)
        {
            return;
        }

        new RenderMapJob { ECB = ecb, mapComponent = map }.Schedule();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

[BurstCompile]
public partial struct RenderMapJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public MapComponent mapComponent;

    void Execute(RefRW<CellComponent> cellComponent, RefRW<MaterialMeshInfo> materialMeshInfo, RefRO<MaterialMeshConfig> materialMeshConfig)
    {
        switch (mapComponent.maps[cellComponent.ValueRO.cellID])
        {
            case (int)ColorCell.Player1:
                cellComponent.ValueRW.value = (int) ColorCell.Player1;
                materialMeshInfo.ValueRW.MaterialID = materialMeshConfig.ValueRO.Player1_material;
                
                break;
            case (int)ColorCell.Player2:
                cellComponent.ValueRW.value = (int)ColorCell.Player2;
                materialMeshInfo.ValueRW.MaterialID = materialMeshConfig.ValueRO.Player2_material;
                break;
            case (int)ColorCell.Wall:
                cellComponent.ValueRW.value = (int)ColorCell.Wall;
                break;
        }
    }
}