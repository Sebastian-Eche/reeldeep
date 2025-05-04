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
    [SerializeField] private float weightInStomach;
    public static event Action<Fish> OnFishHooked;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer currFishSprite = gameObject.GetComponent<SpriteRenderer>();
        currFishSprite.sprite = fishInfo.fishSprite;
        

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

    public void  AddWeightToStomach(float weight){
        weightInStomach += weight;
    }

    public float GetWeightInStomach(){
        return weightInStomach;
    }

    private void SetRarityColor(){
        switch (fishInfo.rarity){
            case FishInfo.Rarity.Common:
                break;
            case FishInfo.Rarity.Uncommon:
                break;
            case FishInfo.Rarity.Rare:
                break;
            case FishInfo.Rarity.Legendary:
                break;
        }
    }

}
