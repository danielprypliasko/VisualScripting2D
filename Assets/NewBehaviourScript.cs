using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class NewBehaviourScript : MonoBehaviour
{
    //public Transform transform;
    private Rigidbody2D rb;
    private float friction = 0.1f;
    private float speed = 35.0f;
    public InputSystem_Actions inputSystemActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Hello, World!");
        //transform = GetComponent<Transform>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputSystemActions = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        inputSystemActions.Enable();
    }
    private void OnDisable()
    {
        inputSystemActions.Disable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        Vector2 movement = inputSystemActions.Player.Move.ReadValue<Vector2>();
        Vector2 velocity = movement * speed * Time.fixedDeltaTime;

        rb.AddForce(velocity, ForceMode2D.Impulse);
        rb.AddForce(-rb.linearVelocity * friction, ForceMode2D.Impulse);
        //transform.Translate(movement*10.0f*Time.deltaTime);
    }
}
