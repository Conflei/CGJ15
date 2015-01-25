using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Alien : CharacterMain
{

    public float DownTime = 15f;
    public float HearRadius = 10f;
    public float SeeRadius = 5f;
    public float AttackRadius = 1f;
    public float LastTimeRoaming = -9f;
    public List<GameObject> FleePositions;
    public GameObject Target;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, HearRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SeeRadius);
        if (Target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Target.transform.position, 0.5f);
        }
    }

    public void Start()
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

    protected IEnumerator SenseBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Idle && Target == null)
            {
                float LeastDistance = 99999f;
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (Vector3.Distance(transform.position, obj.transform.position) <= SeeRadius)
                    {
                        if (LeastDistance >= Vector3.Distance(obj.transform.position, transform.position))
                        {
                            LeastDistance = Vector3.Distance(obj.transform.position, transform.position);
                            Target = obj.gameObject;
                        }
                    }
                }
                if (Target == null)
                {
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
                    {
                        if (Vector3.Distance(transform.position, obj.transform.position) <= HearRadius && Random.value <= 0.25f)
                        {
                            Target = obj.gameObject;
                        }
                    }
                }
                //If after both checks we have a target then change state
                if (Target != null)
                {
                    targetPositions.Clear();
                    targetPositions.Insert(0, Target);
                    if (Status == CharacterStates.Idle) Status = CharacterStates.Alert;
                }
            }
            yield return new WaitForSeconds(thinkRate * 2f);
        }
    }

    protected override void DoAttack()
    {
        if (Target == null) return;
        if (Target.GetComponent<CharacterMain>() != null) Target.GetComponent<CharacterMain>().Hurt();
        Target = null;
        Status = CharacterStates.Panicked;
        targetPositions.Clear();
        targetPositions.Insert(0, FleePositions[Random.Range(0, FleePositions.Count - 1)]);
    }


    public override void OnTargetReached()
    {
        //End of path has been reached
        //If you want custom logic for when the AI has reached it's destination
        //add it here
        //You can also create a new script which inherits from this one
        //and override the function in that script
        if (Target != null) DoAttack();
        //NextTarget();
    }

    protected override IEnumerator AlertBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Alert)
            {
                speed = baseSpeed * 2f;
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }

    protected override IEnumerator PanicBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Panicked)
            {
                speed = baseSpeed * 1.5f;
                if (targetReached || (Target.transform.position - transform.position).sqrMagnitude >= HearRadius)
                {
                    yield return new WaitForSeconds(10f);
                    if (Status == CharacterStates.Panicked) Status = CharacterStates.Idle;
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }

    protected override IEnumerator IdleBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Idle)
            {
                speed = baseSpeed / 2f;
                if (targetReached || LastTimeRoaming <= Time.time + 10f)
                {
                    yield return new WaitForSeconds(2f);
                    LastTimeRoaming = Time.time + 10f;
                    if (Status == CharacterStates.Idle) RandomScatter();
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }


}
