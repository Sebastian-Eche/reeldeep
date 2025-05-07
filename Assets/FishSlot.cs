using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishSlot : MonoBehaviour
{
    public Image fishImage;
    public TMP_Text nameText;

    private FishInfo currentFish;
    private FishDescription descriptionPanel;

    public void Initialize(FishInfo info, FishDescription description)
    {
        currentFish = info;
        descriptionPanel = description;

        fishImage.sprite = info.fishSprite;
        nameText.text = info.fishSpecies;
    }

    public void OnClick()
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.SetFishInfo(currentFish);
        }
    }
}
