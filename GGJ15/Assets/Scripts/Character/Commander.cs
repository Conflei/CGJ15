using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Commander : CharacterMain{

    public LayerMask layerMask;
    public float HearRadius = 10f;
    public List<GameObject> SafeZones;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, HearRadius);
        Gizmos.color = Color.red;
        if (GameObject.FindGameObjectWithTag("Alien") != null) Gizmos.DrawLine(transform.position, GameObject.FindGameObjectWithTag("Alien").transform.position);
    }

    public override void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController2D>();
        startHasRun = true;
        baseSpeed = speed;
        OnEnable();
    }

    protected override void OnEnable()
    {
        lastRepath = -9999;
        canSearchAgain = true;

        lastFoundWaypointPosition = transform.position;
        RandomScatter();

        if (startHasRun)
        {
            //Make sure we receive callbacks when paths complete
            seeker.pathCallback += OnPathComplete;

            StartCoroutine(RepeatTrySearchPath());
            StartCoroutine(IdleBehavior());
            StartCoroutine(AlertBehavior());
            StartCoroutine(PanicBehavior());
            StartCoroutine(SenseBehavior());
            StartCoroutine(ActionBehavior());
        }
    }

    public GameObject FindClosestSafeZone()
    {
        float LeastDistance = 99999f;
        GameObject best = null;
        foreach (GameObject obj in SafeZones)
        {
            if (LeastDistance >= Vector3.Distance(obj.transform.position, transform.position))
            {
                LeastDistance = Vector3.Distance(obj.transform.position, transform.position);
                best = obj;
            }
        }
        return best;
    }

    protected IEnumerator SenseBehavior()
    {
        while (Alive)
        {
            if (GameObject.FindGameObjectWithTag("Alien") != null){
                GameObject allen = GameObject.FindGameObjectWithTag("Alien");
                RaycastHit2D hit = Physics2D.Raycast(transform.position, (allen.transform.position - transform.position).normalized, HearRadius, layerMask);
                if (hit.collider)
                {
                    if (hit.collider.gameObject.tag == "Alien")
                    {
                        Sanity -= 25;
                        if (Status == CharacterStates.Idle) Status = CharacterStates.Alert;
                    }
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }
}
