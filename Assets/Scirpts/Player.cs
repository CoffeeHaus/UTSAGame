using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class Player : NetworkBehaviour
{
    public float moveSpeed = 5.0f;
    public float jumpForce = 7.0f;
    public float maxSpeed = 7.0f;
    public LayerMask groundLayer;  // Assign a LayerMask to determine what is considered "ground"
    public Transform groundCheck;  // Assign a Transform where the ground check should occur
    public Vector2 boxSize;  // Define the size of the ground check box
    public GameObject deathBall;
    public GameObject explodeBullet;
    public Transform bulletSpawnPoint;
    public float slowBulletSpeed = 4f;
    public float fastBulletSpeed = 10f; 

    public GameObject Body;

    public int maxBallCount = 20; // Set maximum number of balls allowed
    private Queue<GameObject> spawnedBalls = new Queue<GameObject>();
    public Rigidbody2D rb;
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool isShot = false;
    private GameObject firedBullet;
    [SyncVar] private GameObject myBullet;  
    public SpriteRenderer bodySpriteRenderer;
    private AudioSource audioSource;
    public AudioClip fireSound;
    public TextMeshProUGUI textMeshPro;

    private float lastShootTime;
    public float shootCooldown = .5f;

    [SyncVar(hook = nameof(OnSyncNameChanged))]
    public string playerName;

    [SyncVar(hook = nameof(OnSyncPositionChanged))]
    public Vector2 syncPosition;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        bodySpriteRenderer.color = GetComponent<PlayerColor>().playerColor;

        if (!isLocalPlayer && rb != null)
        {
            rb.isKinematic = true;
        }
    }
    public override void OnStartClient()
    {
        rb = GetComponent<Rigidbody2D>();
                if (!isLocalPlayer && rb != null)
            {
                rb.isKinematic = true;
            }
    }

    private void OnSyncNameChanged(string oldName, string newName)
    {
        textMeshPro.name = newName;
    }
    public void SetName(string name)
    {
        if (isLocalPlayer)
        {
            CmdSetName(name);
        }
    }

    [Command]
    private void CmdSetName(string name)
    {
        playerName = name;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.tag == "DeathBall")
        {
            
            DeathBall deathBall = collision.gameObject.GetComponent<DeathBall>();
            if (deathBall.ownerId != netId)
            {
                Debug.Log("HitKillPlayer");
                    if (isLocalPlayer)
                        {
                            Debug.Log("Local Player Hit DeathBall");
                            CmdPlayerDied(this.gameObject);
                        }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer) return;

        // Check if the player is grounded using a rectangular overlap box
        isGrounded = Physics2D.OverlapBox(groundCheck.position, boxSize, 0f, groundLayer);

        float moveY = 0;
        // Jumping
        if (!isJumping && Input.GetButtonDown("Jump") && isGrounded && Time.time > lastShootTime + shootCooldown)
        {
            isJumping = true;
            lastShootTime = Time.time;
        }

        float moveX = Input.GetAxis("Horizontal");// + rb.velocity.x;
        //Debug.LogFormat(" Move x {0}  {1}", moveX, moveY);
        //Debug.LogFormat("Velocity {0}", rb.velocity);
        Vector2 move = new Vector2(moveX, moveY);

        if(moveX < 0 && rb.velocity.x < -maxSpeed)
        {
            move = new Vector2(0, 0);
        }
        else if(moveX > 0 && rb.velocity.x > maxSpeed)
        {
            move = new Vector2(0, 0);
        }

        CmdMove(move, isJumping);
        isJumping = false;


        

        if (Input.GetMouseButtonDown(0) && NetworkClient.isConnected && NetworkClient.ready) // Left Click
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 shootingDirection = (mousePosition - transform.position).normalized;

            CmdShootDeathBall(shootingDirection, slowBulletSpeed);
        }

        if (Input.GetMouseButtonDown(1) && NetworkClient.isConnected && NetworkClient.ready) // Right Click
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 shootingDirection = (mousePosition - transform.position).normalized;

            if(isShot)
            {
                Debug.Log("detonate bullet");
                isShot = false;
                CmdExplodeBullet(myBullet);
            }
            else
            {
                Debug.Log("fire bullet");
                isShot = true;
                CmdShootBullet(shootingDirection, fastBulletSpeed);
            }
            
        }
        syncPosition = rb.position;

    }


    public Vector3 GetRespawnPoint()
    {
        // Find all respawn points in the scene
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("spawn");


        // If at least one respawn point exists, select one randomly
        if (respawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, respawnPoints.Length);
            return respawnPoints[randomIndex].transform.position;
        }
        else
        {
            // Fallback to the origin point if no respawn points exist
            return Vector3.zero;
        }
    }

    // Draw the ground check box in the Scene view for easier debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (groundCheck != null)
        {
            Gizmos.DrawWireCube(groundCheck.position, boxSize);
        }
    }

    public override void OnStartLocalPlayer()
    {

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(this.transform);
        }
    }

    [ClientRpc]
    public void RpcRespawn(Vector3 spawnpoint, GameObject player)
    {
        Debug.Log("RpcRespawn");
            Debug.LogFormat("Moving {1} to SpawnPoint {0}", spawnpoint, player);
            player.transform.position = spawnpoint;
    }

    [Command]
    public void CmdPlayerDied(GameObject player)
    {
        Debug.Log("[Server] CMD Player Died");
        Vector3 point = GetRespawnPoint();
        Debug.LogFormat("[Server] Respawn point {0}", point);
        RpcRespawn(point, player);
    }
    [Command]
    public void CmdShootDeathBall(Vector3 direction, float speed)
    {   
        GameObject ball = Instantiate(deathBall, transform.position, Quaternion.identity);

        // Spawn bullet on the network so it appears on clients
        NetworkServer.Spawn(ball);
        ball.GetComponent<DeathBall>().ownerId = netId;
        ball.GetComponent<DeathBall>().ballColor = GetComponent<PlayerColor>().playerColor;
        spawnedBalls.Enqueue(ball); // Add the new ball to the queue

        // Check if the limit is exceeded
        if (spawnedBalls.Count > maxBallCount)
        {
            GameObject oldestBall = spawnedBalls.Dequeue(); // Remove the oldest ball from the queue
            NetworkServer.Destroy(oldestBall); // Destroy the oldest ball
        }
        // Instantiate bullet on the server
        
        // Add force to the bullet
        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }

        RpcChangeBallColorForLocalPlayer(ball);


    }

    [Command]
    public void CmdShootBullet(Vector3 direction, float speed)
    {
        // Instantiate bullet on the server
        GameObject bullet = Instantiate(explodeBullet, transform.position, Quaternion.identity);
        
        // Spawn bullet on the network so it appears on clients
        NetworkServer.Spawn(bullet);

        firedBullet = bullet;
        // Add force to the bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        RpcChangeBallColorForLocalPlayer(bullet);
        RpcSetMyBullet(bullet);
    }
    
    [ClientRpc]
    public void RpcChangeBallColorForLocalPlayer(GameObject ball)
    {

        ball.GetComponent<SpriteRenderer>().color = this.GetComponent<PlayerColor>().playerColor;

    }

    [Command]
    public void CmdExplodeBullet(GameObject Bullet)
    {
        if(Bullet != null){
        Debug.Log("[Server] explode bullet");
        Bullet.GetComponent<EBall>().Explode();
        }
    }

    [ClientRpc]
    
    public void RpcSetMyBullet(GameObject bullet)
    {
        if (isLocalPlayer)
        {
            myBullet = bullet;
        }
        
    }

    [ClientRpc]
    public void RpcSetColor()
    {
        this.GetComponent<SpriteRenderer>().color = GetComponent<PlayerColor>().playerColor;
        
    }

    [Command]
    private void CmdMove(Vector2 move, bool isJump)
    {
        // Perform movement and logic on the server
        RpcPerformMovement(move * moveSpeed, isJump);
        syncPosition = rb.position;
    }

    [ClientRpc]
    private void RpcPerformMovement(Vector2 move, bool isJump)
    {
        // Perform movement and logic on all clients
        if (rb == null)
        {
            Debug.Log("RB NULL!");
            return;
        }
        if(isLocalPlayer)
        {
            rb.AddForce(move);
            if(isJump)rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
        else
        {
            rb.position = syncPosition;
        }
    }
    [ClientRpc]
    private void RpcPerformExplode(Vector3 force)
    {
        // Perform movement and logic on all clients
        if (rb == null)
        {
            Debug.Log("RB NULL!");
            return;
        }
        if(isLocalPlayer)
        {
            rb.AddForce(force);
        }
    }
    
    private void OnSyncPositionChanged(Vector2 oldPosition, Vector2 newPosition)
    {
            //Debug.Log("Otherplayer move");
            rb.position = newPosition;
        
    }

}
