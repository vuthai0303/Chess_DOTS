using CortexDeveloper.ECSMessages.Components;
using Unity.Entities;

public partial struct GameStateMessage : IComponentData, IMessageComponent
{
    public int state;
}
