using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
//This line should always be present at the top of scripts which use pathfinding
using Pathfinding;
[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AIPath (generic)")]
public class CharacterMain : MonoBehaviour{
    public float Sanity = 100f;
    public float Health = 100f;
    public bool Alive = true;
    public Vector3 CurrentDirection;
    public float CurrentMagnitude;
    public enum CharacterStates{
        Idle = 0,
        Alert = 1,
        Attacking =2,
        Panicked = 3,
        Moving = 4,
        Performing = 5
    }

    public enum ActionToPerform
    {
        Die = 0,
        GoCrazy = 1,
        Fixing = 2,
        Healing = 3,
        Motivating = 4,
        Attacking = 5,
        Confused = 6
    }
    //Array of positions to move to, acts like a queue.
    public List<GameObject> targetPositions;
    protected GameObject targetPosition;

    [HideInInspector]
    public Seeker seeker;
    [HideInInspector]
    public CharacterController2D controller;

    public CharacterStates Status;
    public ActionToPerform DoAction;

    /** Enables or disables searching for paths.
     * Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
     * \see #canMove
     */
    public bool canSearch = true;

    /** Enables or disables movement.
      * \see #canSearch */
    public bool canMove = true;

    /** Target point is Interpolated on the current segment in the path so that it has a distance of #forwardLook from the AI.
  * See the detailed description of AIPath for an illustrative image */
    public float forwardLook = 1;

    /** Distance to the end point to consider the end of path to be reached.
     * When this has been reached, the AI will not move anymore until the target changes and OnTargetReached will be called.
     */
    public float endReachedDistance = 0.2F;

    public float ScatterFactor = 1f;
    public float UseRadius = 1f;

    /** Determines how often it will search for new paths. 
	 * If you have fast moving targets or AIs, you might want to set it to a lower value.
	 * The value is in seconds between path requests.
	 */
	public float repathRate = 0.5F;

    public float thinkRate = 0.1f;

    private Collider2D[] surroundingObjects;
    	
	
	/** Holds if the end-of-path is reached
	 * \see TargetReached */
	public bool targetReached = false;
	
	/** Only when the previous path has been returned should be search for a new path */
	public bool canSearchAgain = true;

    /** Do a closest point on path check when receiving path callback.
     * Usually the AI has moved a bit between requesting the path, and getting it back, and there is usually a small gap between the AI
     * and the closest node.
     * If this option is enabled, it will simulate, when the path callback is received, movement between the closest node and the current
     * AI position. This helps to reduce the moments when the AI just get a new path back, and thinks it ought to move backwards to the start of the new path
     * even though it really should just proceed forward.
     */
    public bool closestOnPathCheck = true;

	protected Vector3 lastFoundWaypointPosition;
	protected float lastFoundWaypointTime = -9999;

    /** Holds if the Start function has been run.
	* Used to test if coroutines should be started in OnEnable to prevent calculating paths
	* in the awake stage (or rather before start on frame 0).
	*/
    [HideInInspector]
	public bool startHasRun = false;

    	
	/** Time when the last path request was sent */
    [HideInInspector]
	public float lastRepath = -9999;
	

	/** Returns if the end-of-path has been reached
	 * \see targetReached */
	public bool TargetReached {
		get {
			return targetReached;
		}
	}
    
    //The calculated path
    public Path path;

    //The AI's speed per second
    public float speed = 100;

    protected float baseSpeed;

    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = 0.1f;

    //The waypoint we are currently moving towards
    private int currentWaypoint = 0;

    public virtual void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController2D>();
        startHasRun = true;
        baseSpeed = speed;
        OnEnable();
    }

    public virtual void Hurt()
    {
        Alive = false;
        Destroy(gameObject, 2f);
    }

    /** Run at start and when reenabled.
	 * Starts RepeatTrySearchPath.
	 * 
	 * \see Start
	 */
	protected virtual void OnEnable () {
		
		lastRepath = -9999;
		canSearchAgain = true;

		lastFoundWaypointPosition = transform.position;

		if (startHasRun) {
			//Make sure we receive callbacks when paths complete
			seeker.pathCallback += OnPathComplete;
			
			StartCoroutine (RepeatTrySearchPath ());
            StartCoroutine(IdleBehavior());
            StartCoroutine(AlertBehavior());
            StartCoroutine(PanicBehavior());
		}
	}
	
	public void OnDisable () {
		// Abort calculation of path
		if (seeker != null && !seeker.IsDone()) seeker.GetCurrentPath().Error();
		
		// Release current path
		if (path != null) path.Release (this);
		path = null;
		
		//Make sure we receive callbacks when paths complete
		seeker.pathCallback -= OnPathComplete;
	}

    /** Tries to search for a path every #repathRate seconds.
	* \see TrySearchPath 
	*/
	protected IEnumerator RepeatTrySearchPath () {
		while (true) {
			float v = TrySearchPath ();
			yield return new WaitForSeconds (v);
		}
	}

    protected virtual IEnumerator IdleBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Idle)
            {
                speed = baseSpeed;
                if (targetReached)
                {
                    yield return new WaitForSeconds(2f);
                    NextTarget();
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }


    protected virtual IEnumerator PanicBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Panicked)
            {
                speed = baseSpeed * 1.5f;
                if (targetReached)
                {
                    RandomTarget();
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }


    protected virtual IEnumerator AlertBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Alert)
            {
                speed = baseSpeed * 1.5f;
                if (targetReached)
                {
                    yield return new WaitForSeconds(1f);
                    RandomScatter();
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }

    protected virtual IEnumerator MoveBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Moving){
                speed = baseSpeed;
                if (targetReached)
                {
                    OnFinishMoveCommand();
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }

    protected virtual IEnumerator ActionBehavior()
    {
        while (Alive)
        {
            if (Status == CharacterStates.Performing)
            {
                speed = baseSpeed;
                if (targetReached)
                {
                    yield return HasFinishedPerforming(DoAction);
                    if (Status == CharacterStates.Performing) Status = CharacterStates.Idle;
                }
            }
            yield return new WaitForSeconds(thinkRate);
        }
    }

    protected virtual void DoAttack()
    {
        //set per character
    }

    public virtual void PerformAction(ActionToPerform action)
    {
        StartCoroutine(HasFinishedPerforming(action));
    }

    protected virtual IEnumerator HasFinishedPerforming(ActionToPerform action)
    {
        if (action == ActionToPerform.Fixing)
        {
            Physics2D.OverlapCircleNonAlloc( transform.position, UseRadius, surroundingObjects, LayerMask.NameToLayer("Usable"));
            if (surroundingObjects.Length > 0)
            {
                foreach (Collider2D phys in surroundingObjects)
                {
                    GameObject obj = phys.gameObject;
                    if (obj.tag == "Fixable")
                    {
                        obj.SendMessage("GetFixed");
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(2f);
        }
        else if (action == ActionToPerform.Healing)
        {
            Physics2D.OverlapCircleNonAlloc(transform.position, UseRadius, surroundingObjects, LayerMask.NameToLayer("Character"));
            if (surroundingObjects.Length > 0)
            {
                foreach (Collider2D phys in surroundingObjects)
                {
                    GameObject obj = phys.gameObject;
                    obj.SendMessage("Heal");
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
        else if (action == ActionToPerform.Motivating)
        {
            Physics2D.OverlapCircleNonAlloc(transform.position, UseRadius, surroundingObjects, LayerMask.NameToLayer("Character"));
            if (surroundingObjects.Length > 0)
            {
                foreach (Collider2D phys in surroundingObjects)
                {
                    GameObject obj = phys.gameObject;
                    obj.SendMessage("Motivate");
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
        else if (action == ActionToPerform.GoCrazy)
        {
            Status = CharacterStates.Panicked;
            yield break;
        }
        else if (action == ActionToPerform.Die)
        {
            Alive = false;
            yield break;
        }
        else if (action == ActionToPerform.Confused)
        {
            //lol, yet to be decided
        }
        else if (action == ActionToPerform.Attacking)
        {
            DoAttack();
            yield return new WaitForSeconds(0.5f);
        }
        Status = CharacterStates.Idle;
    }
	
	/** Tries to search for a path.
	 * Will search for a new path if there was a sufficient time since the last repath and both
	 * #canSearchAgain and #canSearch are true and there is a target.
	 * 
	 * \returns The time to wait until calling this function again (based on #repathRate) 
	 */
	public float TrySearchPath () {
 		if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch && targetPositions[0] != null) {
			SearchPath ();
			return repathRate;
		} else {
			//StartCoroutine (WaitForRepath ());
			float v = repathRate - (Time.time-lastRepath);
			return v < 0 ? 0 : v;
		}
	}

    public virtual void NextTarget()
    {
        targetPositions.Insert(targetPositions.Count, targetPositions[0]);
        targetPositions.RemoveAt(0);
        targetReached = false;
        OnNextTarget();
    }

    public virtual void RandomScatter()
    {
        Vector3 dir = Random.insideUnitCircle * ScatterFactor;
        GameObject randomWaypoint = new GameObject("Random Waypoint");
        randomWaypoint.transform.position = transform.position + dir;
        if (targetPositions[0] != null && targetPositions[0].name == "Random Waypoint") Destroy(randomWaypoint, 2f);
        targetPositions.RemoveAt(0);
        targetPositions.Insert(0, randomWaypoint);
        targetReached = false;
        OnNextTarget();
    }

    public virtual void RandomTarget()
    {
        targetPositions.Sort((x, y) => Random.value < 0.5f ? -1 : 1);
        targetReached = false;
        OnNextTarget();
    }

    public virtual void OnNextTarget()
    {
        //cool
    }

    public virtual void OnFinishMoveCommand()
    {
        //wow
    }

    	
	public virtual void OnTargetReached () {
		//End of path has been reached
		//If you want custom logic for when the AI has reached it's destination
		//add it here
		//You can also create a new script which inherits from this one
		//and override the function in that script
        Debug.Log("Target reached");
        //NextTarget();
	}

	
	/** Requests a path to the target */
	public virtual void SearchPath () {
		
		if (targetPositions[0] == null) throw new System.InvalidOperationException ("Target is null");
		
		lastRepath = Time.time;
		//This is where we should search to
		targetPosition = targetPositions[0];
		
		canSearchAgain = false;
		
		//Alternative way of requesting the path
		//ABPath p = ABPath.Construct (GetFeetPosition(),targetPoint,null);
		//seeker.StartPath (p);
		
		//We should search from the current position
        if (targetPosition) seeker.StartPath(transform.position, targetPosition.transform.position);
	}
    /** Called when a requested path has finished calculation.
      * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
      * Finally it is returned to the seeker which forwards it to this function.\n
      */
    public virtual void OnPathComplete(Path _p)
    {
        ABPath p = _p as ABPath;
        if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

        canSearchAgain = true;

        //Claim the new path
        p.Claim(this);

        // Path couldn't be calculated of some reason.
        // More info in p.errorLog (debug string)
        if (p.error)
        {
            p.Release(this);
            return;
        }

        //Release the previous path
        if (path != null) path.Release(this);

        //Replace the old path
        path = p;

        //Reset some variables
        currentWaypoint = 0;

        //The next row can be used to find out if the path could be found or not
        //If it couldn't (error == true), then a message has probably been logged to the console
        //however it can also be got using p.errorLog
        //if (p.error)

        if (closestOnPathCheck)
        {
            Vector3 p1 = Time.time - lastFoundWaypointTime < 0.3f ? lastFoundWaypointPosition : p.originalStartPoint;
            Vector3 p2 = transform.position;
            Vector3 dir = p2 - p1;
            float magn = dir.magnitude;
            dir /= magn;
            int steps = (int)(magn / nextWaypointDistance);


            for (int i = 0; i <= steps; i++)
            {
                CalculateVelocity(p1);
                p1 += dir;
            }

        }
    }
	
    public void FixedUpdate()
    {
        if (path == null || !Alive)
        {
            //We have no path to move after yet
            return;
        }
        Vector3 dir = CalculateVelocity(transform.position);
        CurrentDirection = dir;
        CurrentMagnitude = dir.sqrMagnitude;
        if (dir != Vector3.zero) controller.move(dir);
    }
	

    protected Vector3 CalculateVelocity ( Vector3 currentPosition ){
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0) return Vector3.zero; 
        List<Vector3> vPath = path.vectorPath;

        if (vPath.Count == 1)
        {
            vPath.Insert(0, currentPosition);
        }

        if (currentWaypoint >= vPath.Count) { currentWaypoint = vPath.Count -1; }

        if (currentWaypoint <= 1) currentWaypoint = 1;

        Vector3 dir = (vPath[currentWaypoint] - transform.position).normalized;
        dir.z = 0;
        if (targetPositions[0] == null || (currentWaypoint == vPath.Count - 1 && Vector3.Distance(transform.position, targetPositions[0].transform.position) <= endReachedDistance))
        {
            if (!targetReached) { targetReached = true; OnTargetReached(); }

            //Send a move request, this ensures gravity is applied
            return Vector3.zero;
        }
        dir *= speed * Time.fixedDeltaTime;
        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        if (vPath[currentWaypoint] != null && Vector3.Distance(transform.position, vPath[currentWaypoint]) < nextWaypointDistance)
        {
            currentWaypoint++;
        }
        return dir;
    }
}