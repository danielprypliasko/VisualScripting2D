using System.Collections.Generic;
using UnityEngine;

public abstract class Node<Inputs, Outputs> : MonoBehaviour
{


    public abstract Inputs TakeInputs();
    public abstract Outputs FeedOutputs(Outputs a);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
