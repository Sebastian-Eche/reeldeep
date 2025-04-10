using System;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public bool isHooked = false;
    public float weight;
    public float length;
    public int rarity;
    public static event Action<Fish> OnFishHooked;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HookFish(){
        Debug.Log(gameObject.name + " is hooked");

        if(OnFishHooked != null){
            OnFishHooked.Invoke(this);
        }
        
    }

}
