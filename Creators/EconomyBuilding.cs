using UnityEngine;
using System.Collections;

public class EconomyBuilding : MonoBehaviour {

		
	public int structureIndex, ProdPerHour, StoreCap;								
	public string  StructureType, ProdType, StoreType, StoreResource;

	public float storedGold, storedMana, storedSoldiers;

	public void ModifyGoldAmount(float f)
	{
		storedGold += f;
	}
	public void ModifyManaAmount(float f)
	{
		storedMana += f;
	}

}
