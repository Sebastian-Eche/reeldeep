using UnityEngine;

public class FishMannager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject fishEncyclopediaUI;
        private bool isFishEncyclopediaOpen = false;

        public void ToggleFishEncyclopedia()
        {
            isFishEncyclopediaOpen = !isFishEncyclopediaOpen;
            fishEncyclopediaUI.SetActive(isFishEncyclopediaOpen);
        }
}
