using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour {

	public GameObject onScreenPos;
	public GameObject outScreenPos;
	public Text allComments;
	public Text title;


	// Use this for initialization
	void Start () {
		gameObject.transform.position = outScreenPos.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DisableInfo(){
		iTween.MoveTo(gameObject,iTween.Hash("position",outScreenPos.transform.position,"time",1f));
	}

	public void EnableInfo(int character = 1){
		List<string> comments;
		switch (character) {
		case 1:
			title.text = "Comandante";
			allComments.text = "";
			comments = GameObject.FindGameObjectWithTag("UI").GetComponent<ChatMaster>().commander;
			foreach(string comment in comments){
				allComments.text += ",>>>"+comment;
				allComments.text = allComments.text.Replace(",", "\n");
			}
			break;
		case 2:
			title.text = "Doctor";
			allComments.text = "";
			comments = GameObject.FindGameObjectWithTag("UI").GetComponent<ChatMaster>().medic;
			foreach(string comment in comments){
				allComments.text += ",>>>"+comment;
				allComments.text = allComments.text.Replace(",", "\n");
			}
			break;
		case 3:
			title.text = "Ingeniero";
			allComments.text = "";
			comments = GameObject.FindGameObjectWithTag("UI").GetComponent<ChatMaster>().engineer;
			foreach(string comment in comments){
				allComments.text += ",>>>"+comment;
				allComments.text = allComments.text.Replace(",", "\n");
			}
			break;
		case 4:
			title.text = "Cientifico";
			allComments.text = "";
			comments = GameObject.FindGameObjectWithTag("UI").GetComponent<ChatMaster>().scientist;
			foreach(string comment in comments){
				allComments.text += ",>>>"+comment;
				allComments.text = allComments.text.Replace(",", "\n");
			}
			break;
		}

		iTween.MoveTo(gameObject,iTween.Hash("position",onScreenPos.transform.position,"time",1f));
	}
}
