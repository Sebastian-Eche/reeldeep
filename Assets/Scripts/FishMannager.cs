using System.Collections.Generic;
using UnityEngine;

public class FishMannager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject fishEncyclopediaUI;
    public GameObject fishSlotPrefab;
    public Transform fishSlotParent;

    private bool isFishEncyclopediaOpen = false;
    private HashSet<FishInfo> caughtFish = new HashSet<FishInfo>();
    public FishDescription fishDescriptionPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleFishEncyclopedia();
        }
    }

    public void ToggleFishEncyclopedia()
    {
        isFishEncyclopediaOpen = !isFishEncyclopediaOpen;
        fishEncyclopediaUI.SetActive(isFishEncyclopediaOpen);

        // Pause/unpause game when encyclopedia is toggled
        Time.timeScale = isFishEncyclopediaOpen ? 0 : 1;
    }

   public void AddCaughtFish(FishInfo info)
{
    if (caughtFish.Contains(info))
        return;

    GameObject slot = Instantiate(fishSlotPrefab, fishSlotParent);
    FishSlot slotScript = slot.GetComponent<FishSlot>();

    if (slotScript != null)
    {
        
        slotScript.Initialize(info, fishDescriptionPanel);
    }
    else
    {
        Debug.LogWarning("FishSlot script is missing on the prefab!");
    }

    caughtFish.Add(info);
}


    
}
