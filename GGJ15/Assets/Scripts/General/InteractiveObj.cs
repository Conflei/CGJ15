using UnityEngine;
using System.Collections;


public class InteractiveObj : MonoBehaviour {

	public AudioClip soundFX;

	public Color overColor;

	public bool status = false;

	public SpriteRenderer selfSprite;

	public Sprite active;

	public Sprite inactive;

	private bool over = false;


	void Awake(){
		selfSprite.color = Color.gray;
		if (!status) {
						selfSprite.sprite = inactive;
				} else {
						selfSprite.sprite = active;		
		}
	}

	public void OnMouseOver(){

		if (!over) {
			Debug.Log("Se puede interactuar");
						over = true;
						selfSprite.color = Color.white;
				}
	}

	public void OnMouseExit(){
		selfSprite.color = Color.gray;
		over = false;
	}

	public void OnMouseDown(){
		status = !status;
		if (status)
						selfSprite.sprite = active;
				else
						selfSprite.sprite = inactive;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
