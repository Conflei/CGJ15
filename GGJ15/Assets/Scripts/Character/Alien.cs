using UnityEngine;
using System.Collections;

public class Alien : CharacterMain {

    public float DownTime = 15f;
    public float HearRadius = 10f;
    public float SeeRadius = 5f;
    public float AttackRadius = 1f;
    private Seeker seeker;
    private CharacterController2D controller;
    protected bool startHasRun = true;
    protected int lastRepath = -9999;
    public GameObject[] FleePositions;
    public GameObject Target;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, HearRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, SeeRadius);
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

        if (startHasRun)
        {
            //Make sure we receive callbacks when paths complete
            seeker.pathCallback += OnPathComplete;

            StartCoroutine(RepeatTrySearchPath());
            StartCoroutine(IdleBehavior());
            StartCoroutine(AlertBehavior());
            StartCoroutine(PanicBehavior());
            StartCoroutine(SenseBehavior());
        }
    }

    protected IEnumerator SenseBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Idle)
            {
                float LeastDistance = 99999f;
                Physics2D.OverlapCircleNonAlloc(transform.position, SeeRadius, surroundingObjects, LayerMask.NameToLayer("Character"));
                if (surroundingObjects.Length > 0f)
                {
                    foreach (Collider2D phys in surroundingObjects)
                    {
                        if (LeastDistance < (phys.transform.position - transform.position).magnitude)
                        {
                            LeastDistance = (phys.transform.position - transform.position).magnitude;
                            Target = phys.gameObject;
                        }
                    }
                }
                if (Target == null){
                    Physics2D.OverlapCircleNonAlloc(transform.position, HearRadius, surroundingObjects, LayerMask.NameToLayer("Character"));
                    if (surroundingObjects.Length > 0f)
                    {
                        foreach (Collider2D phys in surroundingObjects)
                        {
                            if (LeastDistance < (phys.transform.position - transform.position).magnitude)
                            {
                                LeastDistance = (phys.transform.position - transform.position).magnitude;
                                Target = phys.gameObject;
                            }
                        }
                    }
                    if (Target != null && Random.Range(1, (int)LeastDistance) != 1) Target = null; 
                }
                //If after both checks we have a target then change state
                if (Target != null)
                {
                    targetPositions.Clear();
                    targetPositions.Insert(0, Target);
                    Status = CharacterStates.Alert;
                }
                else yield return new WaitForSeconds(thinkRate);
            }
        }
    }

    protected override void DoAttack()
    {
        Physics2D.OverlapCircleNonAlloc(transform.position, UseRadius, surroundingObjects, LayerMask.NameToLayer("Character"));
        if (surroundingObjects.Length > 0f)
        {
            foreach (Collider2D phys in surroundingObjects)
            {
                if (Target == phys.gameObject)
                {
                    phys.gameObject.SendMessage("Hurt");
                    Target = null;
                    Status = CharacterStates.Panicked;
                    break;
                }
            }
        }
    }

    protected override IEnumerator AlertBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Alert)
            {
                if ((Target.transform.position - transform.position).magnitude >= HearRadius)
                {
                    Status = CharacterStates.Idle;
                    Target = null;
                    continue;
                }
                speed = baseSpeed * 2f;
                if (targetReached)
                {
                    Status = CharacterStates.Performing;
                    DoAction = ActionToPerform.Attacking;
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }


}
