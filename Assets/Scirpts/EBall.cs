using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EBall : NetworkBehaviour
{
    public float speed = 20f;          // Speed of the ball
    public float explosionRadius = 5f; // Radius of the explosion
    public float explosionForce = 700f; // Force of the explosion
    public GameObject explosionEffect; // Assign a prefab for explosion effects if you have one
    public AudioSource audioSource;
    private void Start()
    {
        // Add initial force to the ball (only on the server)
        if (isServer)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                //Debug.Log(transform.up);
                //rb.velocity = transform.up * speed;
            }
            else
            {
                Debug.LogError("No Rigidbody2D found on ExplosiveBall. Cannot apply initial movement.");
            }
        }
    }

    [ServerCallback] // This ensures the OnTriggerEnter2D runs only on the server
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Perform the explosion
        Explode();
        RPCPlayExplosion();
        // Destroy this GameObject
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void Explode()
    {
        
        // Spawn an explosion effect if you have one
        if (explosionEffect)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, transform.rotation);
            NetworkServer.Spawn(explosion);
            Destroy(explosion, 2f); // Assuming the effect lasts for 2 seconds
        }

        // Find all the surrounding objects and apply force to them
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D nearbyObject in colliders)
        {
            Rigidbody2D rb = nearbyObject.GetComponent<Rigidbody2D>();
            NetworkIdentity networkIdentity = nearbyObject.GetComponent<NetworkIdentity>();
            if (rb != null && networkIdentity != null)
            {
                Vector2 direction = rb.transform.position - transform.position;
                rb.AddForce(direction.normalized * explosionForce);
                if(nearbyObject.gameObject.tag == "Player" )
                {
                    RpcAddForce(networkIdentity, direction.normalized * explosionForce);
                }
            }
        }
        NetworkServer.Destroy(gameObject);

    }
    [ClientRpc]
    void RpcAddForce(NetworkIdentity networkIdentity, Vector2 velocity)
    {
        Debug.Log("RPC Player hit");
        if (networkIdentity.isOwned)
        {
            Rigidbody2D rb = networkIdentity.GetComponent<Rigidbody2D>();
            Debug.Log("RPC isLocalPlayer");
            if(rb != null)
            {
                Debug.LogFormat("RPC add force {0}", velocity);
                rb.AddForce(velocity);
            }
        }
    }

    [ClientRpc]
    public void RPCPlayExplosion()
    {
        Debug.Log("RPC Explode Sound");
        audioSource.Play();
    }

}
