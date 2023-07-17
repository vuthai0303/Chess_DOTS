using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateAfter(typeof(UpdateMapSystem))]
public partial struct ScoreSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapComponent>();
        state.RequireForUpdate<ScoreComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        MapComponent map;
        var isMapComponent = SystemAPI.TryGetSingleton<MapComponent>(out map);
        if (!isMapComponent)
        {
            return;
        }

        new CalculatorScoreJob {mapComponent = map }.Schedule();
        state.Dependency.Complete();

    }
}

[BurstCompile]
public partial struct CalculatorScoreJob : IJobEntity
{
    public MapComponent mapComponent;

    void Execute(RefRW<ScoreComponent> scoreComponent)
    {
        int score1 = 0;
        int score2 = 0;
        foreach(var cell in mapComponent.maps)
        {
            if(cell == (int)ColorCell.Player1)
            {
                score1 += 1;
            }
            else if (cell == (int)ColorCell.Player2)
            {
                score2 += 1;
            }
        }
        scoreComponent.ValueRW.score_player1 = score1;
        scoreComponent.ValueRW.score_player2 = score2;
    }
}