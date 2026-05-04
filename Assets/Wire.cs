using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Wire : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Node<dynamic, dynamic> inputNode;
    public Node<dynamic, dynamic> outputNode;

    void TakeInput() {
        var data = inputNode.TakeInputs();
        FeedOutput(data);
    }


    void FeedOutput<T>( T data) {
        outputNode.FeedOutputs(data);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
