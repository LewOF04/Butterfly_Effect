using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D body;
    public float speed;
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");

        //updates the velocity of the x direction based on the input
        if (Mathf.Abs(xInput) > 0)
        {
            body.linearVelocity = new Vector2(xInput * speed, body.linearVelocity.y);
        }
    }
}
