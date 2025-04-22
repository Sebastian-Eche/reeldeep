using UnityEngine;

public class FishMannager : MonoBehaviour
{

    public GameObject fishSlotPrefab; 
    public Transform fishSlotParent; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject fishEncyclopediaUI;
        private bool isFishEncyclopediaOpen = false;



// this is to toggle encylopedia with button. 
        public void ToggleFishEncyclopedia()
        {
            isFishEncyclopediaOpen = !isFishEncyclopediaOpen;
            fishEncyclopediaUI.SetActive(isFishEncyclopediaOpen);
        }


// use this so when user presses on the I key it will open the Fish Encylopedia 
    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.I) && isFishEncyclopediaOpen)
        {
            Time.timeScale = 1;
            fishEncyclopediaUI.SetActive(false);
            isFishEncyclopediaOpen = false;

        }
       else if (Input.GetKeyDown(KeyCode.I) && !isFishEncyclopediaOpen)
        {
            Time.timeScale= 0;
            isFishEncyclopediaOpen = !isFishEncyclopediaOpen;
            fishEncyclopediaUI.SetActive(isFishEncyclopediaOpen);
        }
    }

}
