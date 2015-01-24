using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class IHandler : MonoBehaviour {

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

	public Image[] monsterEmotions;

	public Image monsterActualImage;

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
		actualEmotion = monsterEmo.searching;
		BeginCicle ();
	}
	
	// Update is called once per frame
	void Update () {

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
