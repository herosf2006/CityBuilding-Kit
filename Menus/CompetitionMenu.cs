using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
						//Use this as a startup point for xml linked menu panels; 
						//currently not wired anywhere
public class CompetitionMenu : MonoBehaviour {


	public TextAsset CompetitionsXML;
	private List<Dictionary<string,string>> competitions = new List<Dictionary<string,string>>();
	private Dictionary<string,string> dictionary;

	private const int competitionsNo = 4; 
	public UILabel[] Descriptions = new UILabel[competitionsNo];


	// Use this for initialization
	void Start () {
		GetCompetitions();
		UpdateLabels ();
	}

	private void GetCompetitions()
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(CompetitionsXML.text); 
		XmlNodeList competitionsList = xmlDoc.GetElementsByTagName("Competition");

		foreach (XmlNode competitionInfo in competitionsList) 
		{
			XmlNodeList competitionsContent = competitionInfo.ChildNodes;	
			dictionary = new Dictionary<string, string> ();
		
			foreach (XmlNode competitionItems in competitionsContent) 
			{ 
				if (competitionItems.Name == "Name") 
				{
					dictionary.Add ("Name", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "Description") 
				{
					dictionary.Add ("Description", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "Time") 
				{
					dictionary.Add ("Time", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "ObjPrereq") 
				{
					dictionary.Add ("ObjPrereq", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "GoldSpent") 
				{
					dictionary.Add ("GoldSpent", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "ManaSpent") 
				{
					dictionary.Add ("ManaSpent", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "CrystalSpent") 
				{
					dictionary.Add ("CrystalSpent", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "ObjSpent") 
				{
					dictionary.Add ("ObjSpent", competitionItems.InnerText); 
				}	
				if (competitionItems.Name == "ObjAmount") 
				{
					dictionary.Add ("ObjAmount", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "Obj2Spent") 
				{
					dictionary.Add ("Obj2Spent", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "Obj2Amount") 
				{
					dictionary.Add ("Obj2Amount", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "Obj3Spent") 
				{
					dictionary.Add ("Obj3Spent", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "Obj3Amount") 
				{
					dictionary.Add ("Obj3Amount", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "XPAward") 
				{
					dictionary.Add ("XPAward", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "GoldAward") 
				{
					dictionary.Add ("GoldAward", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "ManaAward") 
				{
					dictionary.Add ("ManaAward", competitionItems.InnerText); 
				}
				if (competitionItems.Name == "SuccessChance") 
				{
					dictionary.Add ("SuccessChance", competitionItems.InnerText); 
				}
			}

			competitions.Add(dictionary);
		}
	}

	private void UpdateLabels()
	{
		for (int i = 0; i < competitionsNo; i++) 
		{
			Descriptions[i].text =competitions[i]["Description"];
		}
	}

}
