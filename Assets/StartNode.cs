using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StartNode : Node<object, int>
{

    private int wattage = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created  
    void Start()
    {

    }

    // Update is called once per frame  
    void Update()
    {

    }

    public override int FeedOutputs(int watts)
    {
        return watts;
    }

    public override object TakeInputs()
    {
        return new object();
    }
}
