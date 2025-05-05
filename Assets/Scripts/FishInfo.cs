using UnityEngine;

[CreateAssetMenu(fileName = "FishInfo", menuName = "Scriptable Objects/FishInfo")]
public class FishInfo : ScriptableObject
{
    public enum Rarity {
        Common, Uncommon, Rare, Legendary
    }
    public Rarity rarity;
    public string fishSpecies;
    public float minWeight;
    public float maxWeight; //weight in lbs
    public float minLength;
    public float maxLength; //length in feet and inches
    public float maxStomachWeight;
    public string habitat; //kelp forest, shallows, intertidal zone, coral reef, caves
    public bool isPrey;
    public Sprite fishSprite;
}
