using UnityEngine;
using System.Collections;

public class CameraFX : MonoBehaviour {

	public bool testingFX = false;

	public enum MyColors
		{
		Blue,
		Red,
		Yellow,
		Black,
		White
		}

	public Color[] ColorsArray = {
		Color.blue,
		Color.red,
		Color.yellow,
		Color.black,
		Color.white
	};

	public SpriteRenderer PreScreen;

	private Color blueColor = Color.blue;
	public Color whiteColor = Color.white;

	public Sprite whiteNoise;
	public float whiteNoiseTime = 1f;
	public Sprite defaultSprite;

	public float zoomSpeed = 1f;
	private tk2dCamera CameraScript;

	private bool shaking = false;
	private Vector3 originalPosition;

	private Vector3 ShakeRange = new Vector3(0.3f,0.3f,0.3f);
	private float ShakeTimer = 0f;
	public float ShakeTime = 0.75f;
	private float ShakeSpeed = 50f;

	private GrayscaleEffect Grayscale;
	private bool onGray;
	private float grayTimer;

	public GameObject Stick;
	private float stickOriginalSize;
	private float lastStickSize;
	private float originalZoomFactor;

	// Use this for initialization
	void Start () {
		grayTimer = 0;
		onGray = false;
		Grayscale = this.GetComponent<GrayscaleEffect>();
		CameraScript = this.GetComponent<tk2dCamera>();
		stickOriginalSize = lastStickSize = Stick.transform.localScale.x;
		originalZoomFactor = CameraScript.ZoomFactor;
	}
	
	// Update is called once per frame
	void Update () {
		if (testingFX) {
						if (Input.GetKey (KeyCode.O))
								FadeIn (whiteColor, 1f);
						if (Input.GetKey (KeyCode.P))
								FadeOut (1f);

						if (Input.GetKey (KeyCode.K))
								CameraScript.ZoomFactor += Time.deltaTime * zoomSpeed;
						if (Input.GetKey (KeyCode.L))
								CameraScript.ZoomFactor -= Time.deltaTime * zoomSpeed;

						if (Input.GetKey (KeyCode.M))
								StartCoroutine(Flash());

						if (Input.GetKey (KeyCode.U))
								WhiteNoise (whiteNoiseTime);

						if(Input.GetKey(KeyCode.C)) CameraShake();

						if(Input.GetKey(KeyCode.G)) ToGray();
			
			if(shaking){
				if(ShakeTimer > ShakeTime*Time.timeScale){
					shaking = false;
					Camera.main.transform.position = new Vector3(originalPosition.x, originalPosition.y,
					                                             originalPosition.z);
				}else{
					ShakeTimer += Time.deltaTime;
					Camera.main.transform.position = originalPosition + Vector3.Scale(SmoothRandom.GetVector2(ShakeSpeed--), ShakeRange);
					ShakeSpeed *= -1;
					ShakeRange = new Vector3(ShakeRange.x*-1, ShakeRange.y);
				}
			}

			if(lastStickSize != Stick.transform.localScale.x){
				lastStickSize = Stick.transform.localScale.x;
				CalculateZoom();
			}
		}
		if(grayTimer >= 0f) grayTimer -= Time.deltaTime;
	}

	public void CalculateZoom(){
		//CameraScript.ZoomFactor = (stickOriginalSize * originalZoomFactor) / lastStickSize; 
	}

	public void FadeIn(Color color, float time)
	{
		StartCoroutine (FadeInWorker (color, time));
	}

	public IEnumerator FadeInWorker(Color color, float time)
	{
		PreScreen.color = color;
		PreScreen.color = new Color (PreScreen.color.r, PreScreen.color.g, PreScreen.color.b, 0f);
		yield return null;
		PreScreen.enabled = true;
		Color startColor = PreScreen.color;	
		Color endColor = new Color (startColor.r, startColor.g, startColor.b, 1f);
		yield return null;

		for (float t = 0f; t <= time; t += Time.deltaTime) {
			Color temp = Color.Lerp (startColor, endColor, t/time);
			PreScreen.color = temp;
			yield return null;
		}
	}

	public void FadeOut(float time)
	{
		StartCoroutine (FadeOutWorker (time));
	}
	
	public IEnumerator FadeOutWorker(float time)
	{
		PreScreen.color = new Color (PreScreen.color.r, PreScreen.color.g, PreScreen.color.b, 1f);
		Color startColor = PreScreen.color;	
		Color endColor = new Color (startColor.r, startColor.g, startColor.b, 0f);
		yield return null;
		
		for (float t = 0f; t <= time; t += Time.deltaTime) {
			Color temp = Color.Lerp (startColor, endColor, t/time);
			PreScreen.color = temp;
			yield return null;
		}

		PreScreen.enabled = false;
	}

	public void WhiteNoise(float time)
	{
		StartCoroutine (WhiteNoiseWorker (time));
	}
	public IEnumerator WhiteNoiseWorker(float time)
	{
		PreScreen.color = new Color (PreScreen.color.r, PreScreen.color.g, PreScreen.color.b, 1f);
		Sprite lastSprite = PreScreen.sprite;
		PreScreen.sprite = whiteNoise;
		PreScreen.enabled = true;
		yield return StartCoroutine (RandomRotate (time));
		PreScreen.enabled = false;
		PreScreen.sprite = defaultSprite;
		PreScreen.gameObject.transform.eulerAngles = new Vector3 (0,0,0);
	}

	public IEnumerator RandomRotate(float time)
	{
		Transform PSTransform = PreScreen.gameObject.transform;
		for (int i = 0; i<12; i++) {
			PSTransform.eulerAngles = new Vector3 (0,0 , Random.Range (0f, 180f));
			yield return new WaitForSeconds (time/12f);	
		}
	}

	public void CameraShake(){
		shaking = true;
		ShakeTimer = 0f;
		originalPosition = Camera.main.transform.position;
	}

	public IEnumerator Flash(){
		yield return StartCoroutine (FadeInWorker (whiteColor, 0.15f));
		yield return new WaitForSeconds (0.5f);
		yield return StartCoroutine (FadeOutWorker (1f));
	}

	public void ToGray(){
		if (grayTimer <= 0f) {
						onGray = !onGray;
						Grayscale.enabled = onGray;
						grayTimer = 0.2f;
				}
	}
}
