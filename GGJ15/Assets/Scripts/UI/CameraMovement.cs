using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public float velocity = .1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

			if(Input.mousePosition.x > Screen.width-30f) GoRight();
			if(Input.mousePosition.x < 30f) GoLeft();

			if(Input.mousePosition.y > Screen.height-30f) Up();
			if(Input.mousePosition.y < 30f) Down();

	}

	public void Up(){
		transform.Translate (0f, 1*velocity*Time.deltaTime, 0f);
	}

	public void Down(){
		transform.Translate (0f, 1*-1*velocity*Time.deltaTime, 0f);
	}

	public void GoRight(){
		transform.Translate (1*velocity*Time.deltaTime, 0f, 0f);
	}

	public void GoLeft(){
		transform.Translate (1*-1*velocity*Time.deltaTime, 0f, 0f);
	}
}
