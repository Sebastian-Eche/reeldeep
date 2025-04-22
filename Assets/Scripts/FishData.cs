using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName;
    public string description;
    public Sprite fishSprite;

    public float baseWeight;
    public float baseLength;
    public int rarity;
}
