using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using TMPro;
public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject menu;

    private PlayerStats[] playerStats;
    [SerializeField] Text[] nameText, hpTexts, faimTexts, froidTexts; 
    [SerializeField] Image[] characterImage;
    [SerializeField] GameObject[] characterPanel;
    public static MenuManager instance;

    [SerializeField] GameObject itemSlotContainer;
    [SerializeField] Transform itemSlotContainerParent;

    [SerializeField] Transform ItemDescMenu;
    [SerializeField] public GameObject ItemNameText;
    [SerializeField] public GameObject ItemDescText;

    public Text itemName, itemDescription;

    void Start()
    {
        instance = this;
        UpdateStats();
        UpdateItemsInventory();
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.M))
        {

            
            
            if (menu.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.Locked;
                menu.SetActive(false);
                GameManager.instance.gameMenuOpened = false;
            }
            else
            {
                UpdateStats();
                Cursor.lockState = CursorLockMode.None;
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
            froidTexts[0].text = "Froid : " + playerStats[0].currentFroid + " / " + playerStats[0].maxFroid;
            faimTexts[0].text = "Faim : " + playerStats[0].currentFaim + " / " + playerStats[0].MaxFaim;




    }


    public void UpdateItemsInventory()
    {

        if (itemSlotContainerParent.childCount > 0)
        { 
            foreach (Transform itemSlot in itemSlotContainerParent)
               {
            Destroy(itemSlot.gameObject);
               }
        }

        foreach (ItemsManager item in Inventory.instance.GetItemList())
        {
                RectTransform itemSlot = Instantiate(itemSlotContainer, itemSlotContainerParent).GetComponent<RectTransform>();

            itemSlot.name = item.itemName+"Conteneur";
            itemSlot.GetComponentInChildren<ImageManager>().Apply(item.itemsImage);
            Text itemsAmountText = itemSlot.Find("Amount Text").GetComponent<Text>();
            itemSlot.GetComponentInChildren<ItemsManager>().itemName = item.itemName;
            itemSlot.GetComponentInChildren<ItemsManager>().itemtype = item.itemtype;
            itemSlot.GetComponentInChildren<ItemsManager>().itemDescription = item.itemDescription;
            itemSlot.GetComponentInChildren<ItemsManager>().itemsImage = item.itemsImage;
            itemSlot.GetComponentInChildren<ItemsManager>().Model = item.Model;
            //itemSlot.Find("3D").GetComponent<Image>().enabled = false;


            //ImageManager.instance.Apply(item.itemsImage);


            if (item.amount > 1)
            {
                itemsAmountText.text = item.amount.ToString();
            }

            else
            {
                itemsAmountText.text = "";
            }
            
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
