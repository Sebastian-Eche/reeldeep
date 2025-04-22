using System;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public bool isHooked = false;
    public FishInfo fishInfo;
    private string fishSpecies;
    private float weight;
    private float length;
    private int rarity;
    public static event Action<Fish> OnFishHooked;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (fishInfo != null){
            weight = (float)Math.Round(UnityEngine.Random.Range(fishInfo.minWeight, fishInfo.maxWeight), 2); 
            length = (float)Math.Round(UnityEngine.Random.Range(fishInfo.minLength, fishInfo.maxLength), 2);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HookFish(){
        Debug.Log(fishInfo.fishSpecies + " is hooked");
        Debug.Log("Weight: " + weight + " Length: " + length);

        if(OnFishHooked != null){
            OnFishHooked.Invoke(this);
        }
        
    }

    public float GetWeight(){
        return weight;
    }

    public float GetLength(){
        return length;
    }

}
