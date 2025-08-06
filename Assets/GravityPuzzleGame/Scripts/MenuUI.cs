using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;

public class MenuUI : MonoBehaviour 
{
	//Pages
	public GameObject menuPage;		//The page containing the menu.
	public GameObject levelsPage;   //The page containing the levels.

	public GameObject shopPage; // The page containing the shop.

	//Levels
	public GameObject levelHolder;	//The gameobject holding all the level buttons.
	public GameObject levelButton;  //The level button prefab.

	public TMP_Text goldText;

	public GameObject shopItemHolder;
	public GameObject itemPrefab;


	void Start()
	{
		//If level 0 is locked, set it to be unlocked.
		if (PlayerPrefs.GetInt("Level0") == 0)
			PlayerPrefs.SetInt("Level0", 1);

		// Load player's gold
		goldText.text = "GOLD: " + CurrencyManager.GetGold().ToString();
		//Load the level buttons.
		LoadLevels();

		// Load the items button
		LoadShopItems();
	}

	void Update ()
	{
		//Used for debugging. Deletes all the player prefs.
		//AKA makes the levels all locked as if you have never played them.
		if(Input.GetKeyDown(KeyCode.RightBracket))
			PlayerPrefs.DeleteAll();
	}

	//Called when the menu buttons are pressed to change the page.
	//"page" is the page that the player wants to change to.
	public void SetPage(string page)
	{
		if (page == "menu")
		{
			menuPage.SetActive(true);
			levelsPage.SetActive(false);
			shopPage.SetActive(false);
		}
		if (page == "levels")
		{
			menuPage.SetActive(false);
			levelsPage.SetActive(true);
			shopPage.SetActive(false);
		}
		if (page == "shop")
		{
			menuPage.SetActive(false);
			levelsPage.SetActive(false);
			shopPage.SetActive(true);
		}
	}

	//This loads the level buttons.
	//It loops through each level, creating a button on the "Levels" page, and making it so that when you click it,
	//it calls the PlayLevel function, which starts the level.
	public void LoadLevels ()
	{
		for(int x = 0; x < Resources.LoadAll<TextAsset>("Levels").Length; x++){
			GameObject level = Instantiate(levelButton, levelHolder.transform.position, Quaternion.identity) as GameObject;
			level.transform.SetParent(levelHolder.transform, false);
			level.transform.Find("Text").GetComponent<TMP_Text>().text = "LEVEL " + x;
			int e = x;
			level.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(delegate {PlayLevel(e);}));
			level.transform.localScale = Vector3.one;

			int l = PlayerPrefs.GetInt("Level" + x);

			if(l == 0){
				level.transform.GetComponent<UnityEngine.UI.Image>().color = new Color(0.5f, 0.0f, 0.0f, 1.0f);
				level.transform.GetComponent<UnityEngine.UI.Button>().interactable = false;
			}
			if(l == 2){
				level.transform.GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.5f, 0.0f, 1.0f);
			}
		}
	}

	public void LoadShopItems()
	{
		TextAsset[] itemFiles = Resources.LoadAll<TextAsset>("Items");
		foreach (TextAsset file in itemFiles)
		{
			ShopItemData data = JsonUtility.FromJson<ShopItemData>(file.text);
			GameObject itemObj = Instantiate(itemPrefab, shopItemHolder.transform);

			// Change image's color
			UnityEngine.UI.Image img = itemObj.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
			Color color;
			if (ColorUtility.TryParseHtmlString(data.color, out color))
				img.color = color;

			// Set up price and button buy/use
			UnityEngine.UI.Button btn = itemObj.transform.Find("Button").GetComponent<UnityEngine.UI.Button>();
			TMP_Text btnText = btn.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
			bool purchased = PlayerPrefs.GetInt("Purchased_" + data.itemID, 0) == 1;
			if (!purchased)
			{
				btnText.text = data.price + " GOLD";
				btn.onClick.AddListener(() => BuyItem(data));
			}
			else
			{
				btnText.text = "USE";
				btn.onClick.AddListener(() => UseItem(data));
			}
		}
	}
	//Called when a level button gets pressed.
	//"level" is the variable that the button sends over, telling the function which level to load.
	public void PlayLevel(int level)
	{
		//We set the playerpref "SelectedLevel" to  be the "level", then we load up the Game.unity scene.
		PlayerPrefs.SetInt("SelectedLevel", level);
		SceneManager.LoadScene(1);
	}

	// Method to handle buying an item
	public void BuyItem(ShopItemData data)
	{
		int gold = CurrencyManager.GetGold();
		if (gold >= data.price)
		{
			CurrencyManager.SetGold(gold - data.price);
			PlayerPrefs.SetInt("Purchased_" + data.itemID, 1);
			// Optionally, refresh shop UI or give feedback to player
			goldText.text = "GOLD: " + CurrencyManager.GetGold().ToString();
			// You may want to reload shop items or update the button state here
		}
		else
		{
			// Optionally, show "not enough gold" message
			Debug.Log("Not enough gold to purchase item.");
		}
	}

	// Method to handle using an item
	public void UseItem(ShopItemData data)
	{
		// Implement logic for using the item here
		// For example, set the selected item ID in PlayerPrefs
		PlayerPrefs.SetString("SelectedItem", data.itemID);
		// Optionally, update UI to reflect the selected item
	}

	//Called when the "QUIT" button is pressed.
	//It quits the application to the desktop.
	public void QuitGame ()
	{
		Application.Quit();
	}
}
