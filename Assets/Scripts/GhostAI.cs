using UnityEngine;
using UnityEngine.AI;
using StealthGame;

public enum GhostState { Wander, Investigate, Chase, Spook, Flee, Hide, Hidden }

public class GhostAI : MonoBehaviour
{
    [Header("Scene References")]
    public Transform player;
    public GameEnding gameEnding;
    public ScaredMeter scaredMeter;

    [Header("Vision")]
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float sightAngle = 120f;
    [SerializeField] private float nearbyRadius = 8f;
    [SerializeField] private float hiddenNearbyRadius = 14f;
    [SerializeField] private float throughWallHearingRadius = 4f;
    [SerializeField] private LayerMask sightBlockers;
    [SerializeField] private PlayerNoise playerNoise;

    [Header("Speeds")]
    [SerializeField] private float wanderSpeed = 1.5f;
    [SerializeField] private float investigateSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float fleeWallSpeed = 1.0f;
    [SerializeField] private float fleeTurnSpeed = 360f;
    [SerializeField] private float hideSpeed = 5.0f;
    [SerializeField] private float hiddenSpeed = 1.0f;

    [Header("Behavior")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float investigateTimeout = 6f;
    //[SerializeField] private float catchDistance = 1.2f;
    [SerializeField] private float fleeDistance = 18f;
    [SerializeField] private float fleeRecoverDistance = 4f;
    [SerializeField] private float lowHealthFleePct = 0.25f;
    [SerializeField] private float vacuumLatchTime = 0.2f;
    [SerializeField] private float hideBiasRange = 10f;

    [Header("Spook / Ectoplasm")]
    [SerializeField] private float spookRange = 6f;
    [SerializeField] private float spookWindupTime = 0.6f;
    [SerializeField] private float attackCooldown = 12f;
    [SerializeField] private GameObject ectoplasmPrefab;
    [SerializeField] private Transform throwOrigin;

    // Runtime
    private NavMeshAgent agent;
    private Animator animator;
    private Vacuumable vacuumable;
    private GhostState currentState;

    private float lastVacuumedTime;
    private float lastAttackTime;
    private float investigateTimer;
    private float spookTimer;
    private bool hasThrownThisSpook;
    private Vector3 lastKnownPlayerPos;
    private Vector3 fleeTarget;

    private float destinationThreshold = 0.1f;

    // ── Computed helpers ────────────────────────────────────────────

    private bool isBeingVacuumed => (Time.time - lastVacuumedTime) < vacuumLatchTime;

    public void NotifyVacuumed() => lastVacuumedTime = Time.time;

    private bool LowHealth()
    {
        if (vacuumable == null || !vacuumable.hasHealth) return false;
        return vacuumable.health < vacuumable.maxHealth * lowHealthFleePct;
    }

    // ── Unity lifecycle ──────────────────────────────────────────────

    void Start()
    {
        agent     = GetComponent<NavMeshAgent>();
        animator  = GetComponentInChildren<Animator>();
        vacuumable = GetComponent<Vacuumable>();

        lastVacuumedTime = -vacuumLatchTime;
        lastAttackTime   = -attackCooldown;

        EnterState(GhostState.Wander);
    }

    void Update()
    {
        // FLEE intentionally disables the NavMeshAgent so the ghost can phase through walls.
        // Skip the on-navmesh guard for that state; otherwise UpdateFlee() would never run.
        if (currentState != GhostState.Flee && (!agent.enabled || !agent.isOnNavMesh))
        {
            Debug.LogWarning("Ghost is NOT on the NavMesh!");
            return;
        }

        switch (currentState)
        {
            case GhostState.Wander:      UpdateWander();      break;
            case GhostState.Investigate: UpdateInvestigate(); break;
            case GhostState.Chase:       UpdateChase();       break;
            case GhostState.Spook:       UpdateSpook();       break;
            case GhostState.Flee:        UpdateFlee();        break;
            case GhostState.Hide:        UpdateHide();        break;
            case GhostState.Hidden:      UpdateHidden();      break;
        }
    }

    // ── State entry — one-time setup when switching states ───────────

    private void EnterState(GhostState newState)
    {
        currentState = newState;
        agent.isStopped = false;

        switch (newState)
        {
            case GhostState.Wander:
                agent.speed = wanderSpeed;
                SetNewWanderDestination();
                break;

            case GhostState.Investigate:
                agent.speed = investigateSpeed;
                investigateTimer = 0f;
                agent.SetDestination(lastKnownPlayerPos);
                break;

            case GhostState.Chase:
                agent.speed = chaseSpeed;
                if (player != null) agent.SetDestination(player.position);
                break;

            case GhostState.Spook:
                agent.isStopped = true;
                spookTimer = 0f;
                hasThrownThisSpook = false;
                break;

            case GhostState.Flee:
                agent.enabled = false;
                fleeTarget = FindFleePoint(fleeDistance);
                break;

            case GhostState.Hide:
                agent.enabled = true;
                agent.speed = hideSpeed;
                NavMeshHit hideHit;
                Vector3 hidePoint = FindFleePoint(fleeDistance * 1.5f);
                if (NavMesh.SamplePosition(hidePoint, out hideHit, fleeDistance, NavMesh.AllAreas))
                    agent.SetDestination(hideHit.position);
                break;

            case GhostState.Hidden:
                agent.speed = hiddenSpeed;
                SetNewWanderDestination();
                break;
        }
    }

    // ── State updates ────────────────────────────────────────────────

    private void UpdateWander()
    {
        if (isBeingVacuumed)  { EnterState(GhostState.Flee);        return; }
        if (LowHealth())      { EnterState(GhostState.Hide);        return; }
        if (CanSeePlayer())   { EnterState(GhostState.Chase);       return; }
        if (IsPlayerNearby()) { lastKnownPlayerPos = player.position; EnterState(GhostState.Investigate); return; }

        if (!agent.pathPending && agent.remainingDistance <= destinationThreshold)
            SetNewWanderDestination();
    }

    private void UpdateInvestigate()
    {
        if (isBeingVacuumed) { EnterState(GhostState.Flee);  return; }
        if (LowHealth())     { EnterState(GhostState.Hide);  return; }
        if (CanSeePlayer())  { EnterState(GhostState.Chase); return; }

        investigateTimer += Time.deltaTime;
        if (investigateTimer >= investigateTimeout)
            EnterState(GhostState.Wander);
    }

    private void UpdateChase()
    {
        if (isBeingVacuumed) { EnterState(GhostState.Flee); return; }
        if (LowHealth())     { EnterState(GhostState.Hide); return; }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < spookRange && CanSeePlayer() && (Time.time - lastAttackTime) >= attackCooldown)
        {
            EnterState(GhostState.Spook);
            return;
        }

        if (!CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            EnterState(IsPlayerNearby() ? GhostState.Investigate : GhostState.Wander);
            return;
        }

        agent.SetDestination(player.position);
    }

    private void UpdateSpook()
    {
        spookTimer += Time.deltaTime;

        // Always face the player while winding up
        if (player != null)
        {
            Vector3 look = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up);
            if (look != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(look);
        }

        // Throw once the windup is done
        if (spookTimer >= spookWindupTime && !hasThrownThisSpook)
        {
            ThrowEctoplasm();
            hasThrownThisSpook = true;
        }

        // A short moment after the throw, retreat
        if (spookTimer >= spookWindupTime + 0.3f)
        {
            lastAttackTime = Time.time;
            EnterState(GhostState.Flee);
        }
    }

    private void UpdateFlee()
    {
        // Face the flee direction first — NavMeshAgent isn't rotating us in this state.
        // Flatten to XZ so the ghost doesn't tilt up/down at off-axis targets.
        Vector3 flatDir = Vector3.ProjectOnPlane(fleeTarget - transform.position, Vector3.up);
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, fleeTurnSpeed * Time.deltaTime);
        }

        // Move directly through walls — NavMeshAgent is disabled in this state
        transform.position = Vector3.MoveTowards(transform.position, fleeTarget, fleeWallSpeed * Time.deltaTime);

        // Arrived at flee target but still too close to the player — pick a new one and keep running
        if (Vector3.Distance(transform.position, fleeTarget) < 1f)
            fleeTarget = FindFleePoint(fleeDistance);

        // Once far enough from the player, snap back onto the NavMesh and pick next state
        if (Vector3.Distance(transform.position, player.position) >= fleeRecoverDistance)
        {
            // Move transform to the nearest NavMesh point BEFORE enabling the agent.
            // Enabling while inside a wall causes "not close enough to NavMesh" errors.
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 30f, NavMesh.AllAreas))
                transform.position = navHit.position;

            agent.enabled = true;
            agent.Warp(transform.position);
            EnterState(LowHealth() ? GhostState.Hide : (CanSeePlayer() ? GhostState.Chase : GhostState.Wander));
        }
    }

    private void UpdateHide()
    {
        // Travelling to the hiding spot on the normal NavMesh — wait until we arrive
        if (!agent.pathPending && agent.remainingDistance <= destinationThreshold)
            EnterState(GhostState.Hidden);
    }

    private void UpdateHidden()
    {
        // Extended hearing range — if player gets too close, flee to a new hiding spot
        if (IsPlayerNearby(hiddenNearbyRadius))
        {
            EnterState(GhostState.Flee); // Flee if when hidden player is still nearby.
            return;
        }

        // Slow cautious wander while hiding
        if (!agent.pathPending && agent.remainingDistance <= destinationThreshold)
            SetNewWanderDestination();
    }

    // ── Vision ───────────────────────────────────────────────────────

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > sightRange) return false;
        if (Vector3.Angle(transform.forward, toPlayer) > sightAngle * 0.5f) return false;

        Vector3 origin  = transform.position + Vector3.up * 1.5f;
        Vector3 target  = player.position + Vector3.up;
        float   rayDist = Vector3.Distance(origin, target);
        Vector3 dir     = (target - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, rayDist,
                            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            bool hitPlayer = hit.transform == player || hit.transform.IsChildOf(player);
            if (!hitPlayer)
            {
                Debug.DrawLine(origin, hit.point, Color.red, 0.5f);
                return false;
            }
        }
        Debug.DrawLine(origin, target, Color.green, 0.5f);
        return true;
    }

    private bool IsPlayerNearby(float radius = -1f)
    {
        if (player == null) return false;
        if (playerNoise != null && !playerNoise.isEmittingNoise) return false;

        float r = radius < 0 ? nearbyRadius : radius;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > r) return false;

        // Reduce effective range if a wall/door is in between
        Vector3 dir = (player.position - transform.position + Vector3.up).normalized;
        float checkDist = dist;
        if (Physics.Raycast(transform.position + Vector3.up, dir, checkDist,
                            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return dist <= throughWallHearingRadius;
        }
        return true;
    }

    // ── Movement helpers ─────────────────────────────────────────────

    private void SetNewWanderDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private Vector3 FindFleePoint(float radius)
    {
        Vector3 away = (transform.position - player.position).normalized;
        NavMeshHit hit;
        for (int i = 0; i < 8; i++)
        {
            Vector3 dir = (away + Random.insideUnitSphere * Random.Range(-this.hideBiasRange, this.hideBiasRange)).normalized;
            Vector3 candidate = transform.position + dir * radius;
            if (NavMesh.SamplePosition(candidate, out hit, radius * 0.5f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position + away * radius;
    }

    private void ThrowEctoplasm()
    {
        if (ectoplasmPrefab == null || throwOrigin == null) return;
        GameObject obj = Instantiate(ectoplasmPrefab, throwOrigin.position, Quaternion.identity);
        EctoplasmProjectile proj = obj.GetComponent<EctoplasmProjectile>();
        if (proj != null)
        {
            proj.targetMeter = scaredMeter;
            proj.Launch(player.position);
        }
    }
}
