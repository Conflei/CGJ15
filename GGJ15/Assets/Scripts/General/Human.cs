using UnityEngine;
using System.Collections;

public class Human : MonoBehaviour {

	public bool alive;

	public float life;

	public float sanity;

	// Use this for initialization
	void Start () {
		alive = true;
		life = 100f;
		sanity = 100f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void ModifyHealth(int lifePoints){
		Camera.main.SendMessage ("RefreshValues");
		life += lifePoints;
		if (life > 100)
						life = 100;
		if (life <= 0) {
						life = 0;
						alive = false;
				}
	}

	void ModifySanity(int sanityPoints){
		Camera.main.SendMessage ("RefreshValues");
		sanity += sanityPoints;
		if (sanity > 100)
			sanity = 100;
		if (sanity < 0)
			sanity = 0;
	}
}
