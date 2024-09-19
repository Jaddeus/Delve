using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SharkAI : Entity
{
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float deceleration = 3f;
    public float rotationSpeed = 2f;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float damage = 20f;

    private enum SharkState { Patrolling, Chasing, Attacking }
    private SharkState currentState = SharkState.Patrolling;
    private Vector3 patrolTarget;
    private Transform player;
    private bool canAttack = true;
    private Vector3 currentVelocity;
    private Quaternion targetRotation;
    private Rigidbody rb;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        SetNewPatrolTarget();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity for underwater movement
        rb.drag = 0.5f; // Add some drag to simulate water resistance
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case SharkState.Patrolling:
                MoveTowards(patrolTarget, maxSpeed * 0.5f);
                if (Vector3.Distance(rb.position, patrolTarget) < 0.1f)
                {
                    SetNewPatrolTarget();
                }
                break;
            case SharkState.Chasing:
                MoveTowards(player.position, maxSpeed);
                break;
            case SharkState.Attacking:
                AttackPlayer();
                break;
        }

        CheckPlayerDistance();
        ApplyMovement();
    }

    private void MoveTowards(Vector3 target, float speed)
    {
        Vector3 directionToTarget = (target - rb.position).normalized;
        Vector3 targetVelocity = directionToTarget * speed;

        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (currentVelocity.magnitude > 0.1f)
        {
            targetRotation = Quaternion.LookRotation(currentVelocity);
        }
    }

    private void ApplyMovement()
    {
        rb.velocity = currentVelocity;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

        if (currentState == SharkState.Attacking)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }
    }

    private void SetNewPatrolTarget()
    {
        patrolTarget = new Vector3(
            Random.Range(-50f, 50f),
            Random.Range(-10f, 0f),
            Random.Range(-50f, 50f)
        );
    }

    private void AttackPlayer()
    {
        if (canAttack)
        {
            Debug.Log($"Shark attacks player for {damage} damage!");
            // Implement actual damage to player here
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void CheckPlayerDistance()
    {
        float distanceToPlayer = Vector3.Distance(rb.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = SharkState.Attacking;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = SharkState.Chasing;
        }
        else
        {
            currentState = SharkState.Patrolling;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && canAttack)
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
                StartCoroutine(AttackCooldown());
            }
        }
    }
}