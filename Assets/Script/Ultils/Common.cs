public enum ColorCell
{
    Empty = 0,
    Player1 = 1,
    Player2 = 2,
    Wall = 3,
}

public enum Direction
{
    Up = 0,
    Left = 1,
    Down = 2,
    Right = 3,
}

public enum Player
{
    Player1 = 1,
    Player2 = 2,
}

public enum TargetSpawn
{
    Player1 = 1,
    Player2 = 2,
    Wall = 3,
}

public enum GameState
{
    Menu = 0,
    GameLoop = 1,
    EndGame = 2,
    RestartGame = 3,
    GameLoopNetwork = 4,
    LoadingMap = 5,
}

public enum StateEndGame
{
    NoPlayer = 0,
    Player1 = 1,
    Player2 = 2,
}