using UnityEngine;
using System.Collections;


//transdata <--> statsbattle transfers
public class LoadTransData : MonoBehaviour {			//passes info received when loading the battle map 

	private Component transData, statsBattle;

	void Start () {

		transData = GameObject.Find("TransData").GetComponent<TransData>();	
		statsBattle = GameObject.Find("StatsBattle").GetComponent<StatsBattle>();

		LoadMultiplayerMap();

	}

	private void  LoadMultiplayerMap()//IEnumerator
	{

		((StatsBattle)statsBattle).availableUnits = ((TransData)transData).returnedFromBattleUnits;
		((StatsBattle)statsBattle).UpdateUnitsNo ();
		//conversion from battle to existing(all)
		//they were sent here as battle units, but here they become all available units
	}

}
