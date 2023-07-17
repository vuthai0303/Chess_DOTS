using Unity.Entities;

public partial struct ScoreComponent : IComponentData
{
    public int score_player1;
    public int score_player2;
}