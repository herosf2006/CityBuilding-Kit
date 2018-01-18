using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.IO;
using System.Text;

public class Store : MonoBehaviour {			//ingame purchase for gold, mana, crystals
	
	public TextAsset StoreXML;
	private List<Dictionary<string,string>> store = new List<Dictionary<string,string>>();
	private Dictionary<string,string> dictionary;

	private const int 
		crystalNo = 6,
		resourceNo = 3;

	private float 
		price,
		purchaseFraction,
		posX = 0;
		
	public int //make private
		quantity,	
		crystalSelection,							//the button you pressed - 0,1,...itemsNo
		goldSelection,
		manaSelection,
		existingGold,
		existingMana,
		missingGold,
		missingMana,
		maxGold,
		maxMana,
		btCellWidth = 220; 							//position increment for the buttons as they are disabled/enabled 
		
	private int[]
		pricesGold = new int[resourceNo],			//stored prices in crystals for ingame resources		
		pricesMana = new int[resourceNo],
		quantityGold = new int[resourceNo],
		quantityMana = new int[resourceNo];
		
	public int exchangeRate = 1000;					//crystal vs gold/mana

	public UIGrid variableGrid;

	public UILabel[] 
		titleCrystalsLb = new UILabel[crystalNo],	
		quantityCrystalsLb = new UILabel[crystalNo],		
		priceCrystalsLb = new UILabel[crystalNo],
		
		quantityGoldLb = new UILabel[resourceNo],	
		priceGoldLb = new UILabel[resourceNo],	
		quantityManaLb = new UILabel[resourceNo],
		priceManaLb = new UILabel[resourceNo]; 

	public GameObject[] 								// the buttons that get deactivated - you fill the vault, 
		goldIngameBt = new GameObject[resourceNo],		// you can no longer buy any gold with crystals
		manaIngameBt = new GameObject[resourceNo];

	private Component stats, statusMsg;						//the script for the  the heads-up display 

	void Start () {

		stats = GameObject.Find("Stats").GetComponent<Stats>();	//reads the Stats script
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		GetPricesXML ();
		InitiateCrystalLabels();
	}

	private void GetPricesXML()
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(StoreXML.text); 
		XmlNodeList priceList = xmlDoc.GetElementsByTagName("Item");

		foreach (XmlNode priceInfo in priceList)
		{
			XmlNodeList priceContent = priceInfo.ChildNodes;	
			dictionary = new Dictionary<string, string>();
			
			foreach (XmlNode priceItems in priceContent) // levels items nodes.
			{
				if(priceItems.Name == "Name")
				{
					dictionary.Add("Name",priceItems.InnerText); // put this in the dictionary.
				}
				if(priceItems.Name == "Quantity")
				{
					dictionary.Add("Quantity",priceItems.InnerText); // put this in the dictionary.
				}
				if(priceItems.Name == "Price")
				{
					dictionary.Add("Price",priceItems.InnerText); // put this in the dictionary.
				}
			}
			store.Add(dictionary);
		}
	}
	private void InitiateCrystalLabels()//first time initializations, to update prices per quantities
	{
		for (int i = 0; i < crystalNo; i++) 
		{			
			titleCrystalsLb [i].text = store [i] ["Name"];
			quantityCrystalsLb [i].text = store [i] ["Quantity"];
			priceCrystalsLb [i].text = store [i] ["Price"] + " $";
		}
	}

	// variableGrid.Reposition ()doesn't work properly if the panel is disabled; also triggered by store tab left
	public void UpdateResources()
	{				
		UpdateMissingIngameResources ();
		IngameResourcesBtTogle (); //fill 1/4 1/2 full buttons toggle
		UpdateIngameResourceLbValues (); // needs 1047 gold to reach 1/4, 3047 to reach 1/2 etc
	}

	private void UpdateMissingIngameResources()
	{
		existingGold = ((Stats)stats).gold + ((Stats)stats).deltaGoldPlus - ((Stats)stats).deltaGoldMinus;
		existingMana = ((Stats)stats).mana + ((Stats)stats).deltaManaPlus - ((Stats)stats).deltaManaMinus;
		missingGold = ((Stats)stats).maxGold - existingGold;
		missingMana = ((Stats)stats).maxMana - existingMana;
		maxGold = ((Stats)stats).maxGold;
		maxMana = ((Stats)stats).maxMana;
	}

	private void IngameResourcesBtTogle()
	{			
		//0-1/4, 1-1/2, 2-1  goldIngameBt  manaIngameBt
		for (int i = 0; i < resourceNo; i++) {
			goldIngameBt [i].SetActive (false);
			manaIngameBt [i].SetActive (false);
		}
		posX = 0;

		//0-1/4, 1-1/2, 2-1  goldIngameBt  manaIngameBt
		if (existingGold == maxGold){}
		else if (existingGold >= maxGold / 2) 	{ ToggleButtons ("gold", 2); } 	//fill to 1/2 
		else if (existingGold >= maxGold / 4) 	{ ToggleButtons ("gold", 1, 2); }	//fill to 1/4
		else  									{ ToggleButtons ("gold", 0, 1, 2); } 

		if (existingMana == maxMana){}
		else if (existingMana >= maxMana / 2) 	{ToggleButtons ("mana", 2); } 	//fill to 1/2 
		else if (existingMana >= maxMana / 4) 	{ToggleButtons ("mana", 1, 2); } 	//fill to 1/4 
		else 									{ToggleButtons ("mana", 0, 1, 2); }

		variableGrid.Reposition (); //doesn't work properly if the panel is disabled
	}

	private void ToggleButtons(string type, params int[] buttons)
	{		
		switch (type) 
		{
		case "gold":
			for (int i = 0; i < buttons.Length; i++) 
			{
				goldIngameBt [buttons[i]].SetActive (true);
				Vector3 pos = goldIngameBt [buttons [i]].transform.position;

				goldIngameBt [buttons [i]].transform.position = new Vector3 (posX , pos.y, pos.z);//pos.y, pos.z
				posX += 220;
			}
			break;

		case "mana":
			for (int i = 0; i < buttons.Length; i++) 
			{
				manaIngameBt [buttons[i]].SetActive (true);
				Vector3 pos = manaIngameBt [buttons [i]].transform.position;
				manaIngameBt [buttons [i]].transform.position = new Vector3 (posX , pos.y, pos.z);//pos.y, pos.z
				posX += 220;
			}
			break;
		}
	}



	/*
	private void TweenButtons()
	{
		int	positionIndex = 0;

		//btCellWidth = 220; //x increment

		for (int i = 0; i < goldIngameBt.Length; i++) 
		{
			if (goldIngameBt [i].activeSelf) 
			{	
				goldIngameBt [i].GetComponent<SpringPosition> ().target = new Vector3 (positionIndex * btCellWidth, 0, 0);
				goldIngameBt [i].GetComponent<SpringPosition> ().enabled = true;

				positionIndex++;
			}
		}
		for (int i = 0; i < manaIngameBt.Length; i++) 
		{
			if (manaIngameBt [i].activeSelf) 
			{
				manaIngameBt [i].GetComponent<SpringPosition> ().target = new Vector3 (positionIndex * btCellWidth, 0, 0);
				manaIngameBt [i].GetComponent<SpringPosition> ().enabled = true;

				positionIndex++;
			}
		}
	}
	*/

	private void UpdateIngameResourceLbValues()
	{		
		int fraction = 4; 
		for (int i = 0; i < resourceNo; i++) 
		{
			int 
			gold = (int)maxGold / fraction - existingGold,
			mana = (int)maxMana / fraction - existingMana,
			priceGold = 1 + (int)gold / exchangeRate,			//1+ to avoid a 0 crystals price
			priceMana = 1+ (int) mana / exchangeRate;

			quantityGoldLb [i].text = (gold).ToString ();
			quantityManaLb[i].text = (mana).ToString ();

			//stores the values for payments
			if (missingGold > 0)
			{ 
				quantityGold [i] = gold;
				priceGoldLb [i].text = priceGold.ToString (); 
				pricesGold [i] = priceGold;	
			} 
			else 
			{ 
				quantityGold [i] = 0;
				priceGoldLb [i].text = " "; 
				pricesGold [i] = 0;
			}

			if (missingMana > 0) 
			{ 
				quantityMana [i] = mana;
				priceManaLb [i].text = priceMana.ToString (); 
				pricesMana [i] = priceMana;
			} 
			else 
			{ 
				quantityMana [i] = 0;
				priceManaLb [i].text = " "; 
				pricesMana [i] = 0;
			}

			if (fraction > 2)
				fraction -= 2;
			else
				fraction -= 1;
		}
	}

	//Buy Crystals
	public void OnBuyCrystals0() { crystalSelection = 0; BuyCrystals (); }
	public void OnBuyCrystals1() { crystalSelection = 1; BuyCrystals (); }
	public void OnBuyCrystals2() { crystalSelection = 2; BuyCrystals (); }
	public void OnBuyCrystals3() { crystalSelection = 3; BuyCrystals (); }
	public void OnBuyCrystals4() { crystalSelection = 4; BuyCrystals (); }
	public void OnBuyCrystals5() { crystalSelection = 5; BuyCrystals (); }

	public void OnBuyGold0() { goldSelection = 0; purchaseFraction = 0.25f; BuyIngameResources ("Gold"); }
	public void OnBuyGold1() { goldSelection = 1; purchaseFraction = 0.5f; BuyIngameResources ("Gold"); }
	public void OnBuyGold2() { goldSelection = 2; purchaseFraction = 1; BuyIngameResources ("Gold"); }

	public void OnBuyMana0() { manaSelection = 0; purchaseFraction = 0.25f; BuyIngameResources ("Mana"); }
	public void OnBuyMana1() { manaSelection = 1; purchaseFraction = 0.5f; BuyIngameResources ("Mana"); }
	public void OnBuyMana2() { manaSelection = 2; purchaseFraction = 1; BuyIngameResources ("Mana"); }


	private void BuyCrystals()//you just pressed a buy button for one of the items; which one is passed by crystalSelection
	{
		price = float.Parse (store [crystalSelection] ["Price"]);//update total price
		quantity = int.Parse (store [crystalSelection] ["Quantity"]);//update quantity displayed on the button

		int 
		maxCrystals = ((Stats)stats).maxCrystals,
		currentCrystals = ((Stats)stats).crystals;//has 3 max 5, you buy 10

		if ((currentCrystals + quantity) > maxCrystals) 
		{
			((Stats)stats).maxCrystals += (quantity - (maxCrystals -currentCrystals));//5+(10-2)=13
		}

		//((Stats)stats).crystals += quantity;
		((Stats)stats).AddResources(0,0,quantity);
		((Stats)stats).UpdateUI();
		((Stats)stats).UpdateCreatorMenus();
	}

	private void BuyIngameResources(string resType)
	{
		UpdateResources ();
		switch (resType) 
		{
		case "Gold":
			if (missingGold == 0) {
				MessengerDisplay ("Vaults are full.");
				return;
			}
			if (pricesGold [goldSelection] > ((Stats)stats).crystals) {
				MessengerDisplay ("You have only " + ((Stats)stats).crystals.ToString () + " crystals.");
				return;
			}
			//((Stats)stats).gold += quantityGold [goldSelection];	//(int)(maxGold * purchaseFraction);
			((Stats)stats).AddResources(quantityGold [goldSelection],0,0);
			//((Stats)stats).crystals -= pricesGold [goldSelection];
			((Stats)stats).SubstractResources(0,0,pricesGold [goldSelection]);
			break;

		case "Mana":
			if (missingMana == 0) {
				MessengerDisplay ("Barrels are full.");
				return;
			}
			if (pricesMana [manaSelection] > ((Stats)stats).crystals) {
				MessengerDisplay ("You have only " + ((Stats)stats).crystals.ToString () + " crystals.");
				return;
			}
			//((Stats)stats).mana += quantityMana [manaSelection];	//(int)(maxMana * purchaseFraction);
			((Stats)stats).AddResources(0, quantityMana [manaSelection], 0);
			//((Stats)stats).crystals -= pricesMana [manaSelection];
			((Stats)stats).SubstractResources(0,0,pricesMana [goldSelection]);
			break;					
		}
		UpdateResources ();
		((Stats)stats).UpdateUI();
		((Stats)stats).UpdateCreatorMenus();
	}

	private void MessengerDisplay(string s)
	{
		((Messenger)statusMsg).DisplayMessage(s);
	}
}