using UnityEngine;
using Mirror;
public class PlayerPointer : NetworkBehaviour
{
    public GameObject pointer;  // The GameObject that will point towards the mouse
    public float pointerDistanceFromCenter = 0.6f; // How far away the pointer is from the center of the player
    
    void Update()
    {   

        // Only run this code if this is the local player in the network
        if (isLocalPlayer)
        {

            // Get the mouse position in world coordinates
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            // Calculate the direction vector from the player to the mouse
            Vector3 toMouse = mousePosition - transform.position;

            // Normalize the direction vector
            toMouse.Normalize();

            // Calculate the position for the pointer GameObject
            Vector3 pointerPosition = transform.position + toMouse * pointerDistanceFromCenter;

            // Set the pointer's position
            pointer.transform.position = pointerPosition;

            // Calculate the rotation angle for the pointer
            float angle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg - 90; // Subtract 90 if the sprite is oriented upwards

            // Rotate the pointer to face the mouse
            pointer.transform.rotation = Quaternion.Euler(0, 0, angle+90);
        }
    }
}
