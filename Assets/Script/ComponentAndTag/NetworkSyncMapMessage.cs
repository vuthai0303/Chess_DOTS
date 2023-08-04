using CortexDeveloper.ECSMessages.Components;
using Unity.Collections;
using Unity.Entities;

public partial struct NetworkSyncMapMessage : IComponentData, IMessageComponent
{
    public NativeArray<int> maps;
}
