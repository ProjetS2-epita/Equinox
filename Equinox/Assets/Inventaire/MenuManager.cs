using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using TMPro;
public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject menu;

    private PlayerStats[] playerStats;
    [SerializeField] Text[] nameText, hpTexts, faimTexts, soifTexts;
    [SerializeField] Image[] characterImage;
    [SerializeField] GameObject[] characterPanel;
    public static MenuManager instance;

    [SerializeField] GameObject itemSlotContainer;
    [SerializeField] Transform itemSlotContainerParent;

    void Start()
    {
        instance = this;
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.M))
        {


            UpdateStats();
            if (menu.activeInHierarchy)
            {
                menu.SetActive(false);
                GameManager.instance.gameMenuOpened = false;
            }
            else
            {
                UpdateItemsInventory();
                menu.SetActive(true);
                GameManager.instance.gameMenuOpened = true;
            }

            
        }
    }

    public void CloseMenu()
    {
        menu.SetActive(false);
        GameManager.instance.gameMenuOpened = false;
    }


    public void UpdateStats()
    {
        playerStats = GameManager.instance.GetPlayerStats();


        
            characterPanel[0].SetActive(true);
            nameText[0].text = playerStats[0].playerName;
            hpTexts[0].text = "HP : " + playerStats[0].currentHP + " / " + playerStats[0].maxHP;
            soifTexts[0].text = "Soif : " + playerStats[0].currentSoif + " / " + playerStats[0].maxSoif;
            faimTexts[0].text = "Faim : " + playerStats[0].currentFaim + " / " + playerStats[0].MaxFaim;




    }


    public void UpdateItemsInventory()
    {
        foreach (Transform itemSlot in itemSlotContainerParent)
        {
            Destroy(itemSlot.gameObject);
        }

        foreach (ItemsManager item in Inventory.instance.GetItemList())
        {
            RectTransform itemSlotNew = Instantiate(itemSlotContainer, itemSlotContainerParent).GetComponent<RectTransform>();
    Sprite itemImage = itemSlotNew.Find("Item Image").GetComponent<Sprite>();
    itemImage = item.itemsImage;
        }

 }


public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting...");
    }
   


   
    //[SerializeField] public int defense = 0;
    // Update is called once per frame

}
