using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [ServerRpc]
    public void randomPositionServerRPC()
    {
        var oldPosition = transform.position;
        transform.position = GetRandomPositionOnXYPlane();
        var newPosition = transform.position;
        print($"{nameof(randomPositionServerRPC)}() -> {nameof(OwnerClientId)}: {OwnerClientId} --- {nameof(oldPosition)}: {oldPosition} --- {nameof(newPosition)}: {newPosition}");
    }

    private static Vector3 GetRandomPositionOnXYPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
    }
}
