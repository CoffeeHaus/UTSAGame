using UnityEngine;
using Mirror;

public class DeathBall : NetworkBehaviour
{
    [SyncVar]
    public uint ownerId; // NetworkID of the player who spawned this DeathBall
    [SyncVar]
    public Color ballColor; // ballColor of the player who spawned this DeathBall
    public void Initialize(uint owner)
    {
        this.ownerId = owner;
    }
}
