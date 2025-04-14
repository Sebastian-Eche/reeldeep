using UnityEngine;

[CreateAssetMenu(fileName = "FishInfo", menuName = "Scriptable Objects/FishInfo")]
public class FishInfo : ScriptableObject
{
    public string fishSpecies;
    public float minWeight;
    public float maxWeight; //weight in lbs
    public float minLength;
    public float maxLength; //length in feet and inches
    public int rarity; // common = 0, uncommon = 1, rare = 2, legendary = 3
    public string habitat; //kelp forest, shallows, intertidal zone, coral reef, caves
    public bool isPrey;
    public Sprite fishSprite;
}
