using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class NewBehaviourScript : MonoBehaviour
{
    public Transform transform;
    public InputSystem_Actions inputSystemActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Hello, World!");
        transform = GetComponent<Transform>();
    }

    void Awake()
    {
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
    void Update()
    {
        
        Vector2 movement = inputSystemActions.Player.Move.ReadValue<Vector2>();
        transform.Translate(movement*10.0f*Time.deltaTime);
    }
}
