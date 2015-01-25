using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scientist : CharacterMain
{
    public float HearRadius = 10f;
    public List<GameObject> SafeZones;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, HearRadius);
    }

    public override void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController2D>();
        startHasRun = true;
        baseSpeed = speed;
        OnEnable();
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

    protected IEnumerator SenseBehavior()
    {
        while (Alive)
        {
            yield return new WaitForSeconds(thinkRate);
        }
    }
}
