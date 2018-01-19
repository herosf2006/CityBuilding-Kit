using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public bool isClosed;
	public GameObject MainPanel;
	public Sprite[] sprResult;
	public Sprite[] sprDice;	
	public Image[] imgDice;
	public Animator aniController;

	int[] intRandom = new int[3];
	bool isActive;
	int intIndex;
	
	// Use this for initialization
	void Start () {
		Initialize();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Initialize() {
		Random.InitState(System.DateTime.Now.Millisecond);

        for(int i = 0; i < 3; i++) {			
			intIndex = Random.Range(0, 6);
			intRandom[i] = intIndex;
            // Debug.Log(i + "_" + intIndex);
			imgDice[i].sprite = sprDice[intIndex];
			imgDice[i+3].sprite = sprResult[intIndex];
		}

		// if (active) {
		// 	int intTemp = (intRandom[0] + intRandom[1] + intRandom[2])/6;
        //     imgDice[intTemp].sprite = sprDice[intTemp];
        //     imgDice[intTemp + 3].sprite = sprResult[intTemp];
		// }
		isActive = true;
		isClosed = false;
	}

	public void RandomResult() {
        Random.InitState(System.DateTime.Now.Millisecond);

        for (int i = 0; i < 3; i++)
        {
            int intIndex = Random.Range(0, 6);
            intRandom[i] = intIndex;
            
            imgDice[i].sprite = sprDice[intIndex];
            imgDice[i + 3].sprite = sprResult[intIndex];
        }

        if (isActive)
        {
            int intTemp = (intRandom[0] + intRandom[1] + intRandom[2]) % 6;
			Debug.Log(intRandom[0] + " " + intRandom[1] + " " + intRandom[2] + "=" + intTemp);
            imgDice[0].sprite = sprDice[intTemp];
            imgDice[3].sprite = sprResult[intTemp];
        }
	}

	public void BTNFUNCTION_ROLL() {
		if (isClosed) {
			Open();
		} else {
			Close();
		}
	}

	public void BTNFUNCTION_PLAY() {

		if (MainPanel.activeSelf) {
			MainPanel.SetActive(false);
            if (!isClosed)
            {
                Close();
            }
		} else {
			MainPanel.SetActive(true);
		}
	}

	void Close() {
		aniController.SetTrigger("Close");
		isClosed = true;
	}
	
	void Open() {
		aniController.SetTrigger("Open");

		delaySec(0.15f, () =>
		{
			if (isClosed) {
				RandomResult();
			}
			isClosed = false;
		});	
	}

    void delaySec(float second, System.Action callback)
    {
        StartCoroutine(delay(second, callback));
    }

    IEnumerator delay(float second, System.Action callback)
    {
        yield return new WaitForSeconds(second);
        callback();
    }
}
