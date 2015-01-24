using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatMaster : MonoBehaviour {

	public Text chatBox;

	public string[] scaryMessages;

	public string[] casualMessages;

	public string[] angryMessages;

	public string[] engineerMessages;

	public string[] medicMessages;

	public string[] scientistMessages;

	public string[] commanderMessages;

	public List<string> engineer = new List<string> ();

	public List<string> medic = new List<string> ();

	public List<string> scientist = new List<string> (); 

	public List<string> commander = new List<string> ();

	// Use this for initialization
	void Start () {
		StartCoroutine (BeginCasualChat ());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public IEnumerator BeginCasualChat(){
		int casMsg;
		while(true){
			casMsg = (int)Random.Range(0, casualMessages.Length);
			switch ((int)Random.Range (1, 5)) {

			case 1:
				chatBox.text ="Cientifico: " +casualMessages[casMsg];
				scientist.Add(chatBox.text);
				break;
		
			case 2:
				chatBox.text ="Comandante: " +casualMessages[casMsg];
				commander.Add(chatBox.text);
				break;
					
			case 3:
				chatBox.text ="Ingeniero: " +casualMessages[casMsg];
				engineer.Add(chatBox.text);
				break;

			case 4:
				chatBox.text ="Medico: " +casualMessages[casMsg];
				medic.Add(chatBox.text);
				break;
			
			case 5: 
				Debug.Log("5");
				break;
			}
			yield return new WaitForSeconds(Random.Range(2,8));
		}

	}
}
