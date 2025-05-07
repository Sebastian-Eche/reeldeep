using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishDescription : MonoBehaviour
{
    public Image fishImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text statsText;

 public void SetFishInfo(FishInfo info)
{
    Debug.Log($"Setting info panel for: {info.fishSpecies}");

    // Make sure the panel is visible
    gameObject.SetActive(true);

    fishImage.sprite = info.fishSprite;
    nameText.text = info.fishSpecies;
    descriptionText.text = $"Habitat: {info.habitat}";
    statsText.text = $"Length: {info.minLength}-{info.maxLength} ft\n" +
                     $"Weight: {info.minWeight}-{info.maxWeight} lbs\n" +
                     $"Rarity: {info.rarity}";
}

}
