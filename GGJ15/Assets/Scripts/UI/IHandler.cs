using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class IHandler : MonoBehaviour {

	public Texture2D customCursor;

	public enum monsterEmo
		{
		searching,
		hidden,
		stalking,
		attacking
		}

	public Text minuteCounter;
	public int totalMinutes = 4;

	public Text cicleCounter;

	public GameObject[] borderAlert;

	public Image[] monsterEmotions;

	public Image monsterActualImage;
	public Image monsterBG;
	public bool monsterAlert;

	public monsterEmo actualEmotion;

	public Human Med;
	public Slider medLife;
	public Slider medSanity;

	public Human Scientist;
	public Slider sciLife;
	public Slider sciSanity;

	public Human Eng;
	public Slider engLife;
	public Slider engSanity;

	public Human Com;
	public Slider comLife;
	public Slider comSanity;

	// Use this for initialization
	void Start () {
		Cursor.SetCursor (customCursor, Vector2.one / 2f, CursorMode.Auto);
		actualEmotion = monsterEmo.searching;
		monsterAlert = false;
		BeginCicle ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.A))
						MonsterAlert ();
	}

	void RefreshValues(){
		medLife.value = Med.life / 100f;
		medSanity.value = Med.sanity / 100f;

		sciLife.value = Scientist.life / 100f;
		sciSanity.value = Scientist.sanity / 100f;

		engLife.value = Eng.life / 100f;
		engSanity.value = Eng.sanity / 100f;

		comLife.value = Com.life / 100f;
		comSanity.value = Com.sanity / 100f;
	}

	public void MonsterAlert(){
		monsterAlert = true;
		StartCoroutine (MonsterAlertWorker ());
		StartCoroutine (BorderAlert ());
	}


	public IEnumerator MonsterAlertWorker(){
		int cicle = 1;
		Color white = Color.white;
		Color red = Color.red;
		Color startColor, endColor;
		float time = .4f;
		while (monsterAlert) {
			startColor = monsterBG.color;

			if(cicle == 1){
				//Blanco a rojo}
				endColor = red;
				for (float t = 0f; t <= time; t += Time.deltaTime) {
					Color temp = Color.Lerp (startColor, endColor, t/time);
					monsterBG.color = temp;
					yield return null;
				}
				cicle = 2;
			}else{
				//Rojo a blanco
				endColor = white;
				for (float t = 0f; t <= time; t += Time.deltaTime) {
					Color temp = Color.Lerp (startColor, endColor, t/time);
					monsterBG.color = temp;
					yield return null;
				}
				cicle = 1;
			}
		}
	}

	public IEnumerator BorderAlert(){
		Color white = new Color (1f, 1f, 1f, 0f);
		Color red = Color.red;
		Color startColor, endColor;
		float time = .4f;
			startColor = white;
		foreach (var border in borderAlert)
						border.SetActive (true);

				//Blanco a rojo}
				endColor = red;
				for (float t = 0f; t <= time; t += Time.deltaTime) {
					Color temp = Color.Lerp (startColor, endColor, t/time);
					foreach(var border in borderAlert)		
					border.GetComponent<Image>().color = temp;
					yield return null;
				}
		yield return new WaitForSeconds (1f);
				//Rojo a blanco
				endColor = white;
				startColor = red;
			
				for (float t = 0f; t <= time; t += Time.deltaTime) {
					Color temp = Color.Lerp (startColor, endColor, t/time);
					foreach(var border in borderAlert)		
					border.GetComponent<Image>().color = temp;
					yield return null;

				}
		foreach (var border in borderAlert)
			border.SetActive (false);
	}

	public void SetMonsterEmotion(monsterEmo emo){
		actualEmotion = emo;
		//monsterActualImage = monsterEmotions [emo]; 
	}

	public void BeginCicle(){
		StartCoroutine (BeginCicleWorker ());
	}

	public IEnumerator BeginCicleWorker(){
		int totalSecs = totalMinutes * 60;
		int actualSecs = 59;


		minuteCounter.text = totalMinutes+":00";
		yield return new WaitForSeconds (.5f);
		totalMinutes--;
		while (totalSecs > 0) {
			if(actualSecs<10)minuteCounter.text = "0"+totalMinutes+":0"+actualSecs;
			minuteCounter.text = "0"+totalMinutes+":"+actualSecs;
			yield return new WaitForSeconds (1.1f);
			totalSecs--;
			actualSecs --;
			if(actualSecs == -1){
				actualSecs = 59;
				totalMinutes--;
			}
		}




	}

}
