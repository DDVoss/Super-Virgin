using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2f;

    [Header("Detection")]
    [SerializeField] private Transform groundCheck;          // slightly in front of feet
    [SerializeField] private Transform wallCheck;            // slightly in front of body
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float enemyCheckExtraDistance = 0.05f; // how far in front of the collider to check

    private Rigidbody2D rb;
    private Collider2D col;

    private int direction = 1; // 1 = right, -1 = left
    private bool wasEnemyAhead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (groundCheck == null || wallCheck == null || col == null)
            return;

        // Ledge check (stable on tilemaps)
        bool groundAhead = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Wall check (ray forward)
        bool wallAhead = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * direction,
            wallCheckDistance,
            groundLayer
        );

        // Enemy check: circle in front of THIS enemy's collider (reliable height)
        Bounds b = col.bounds;
        Vector2 enemyCheckPos = new Vector2(
            b.center.x + direction * (b.extents.x + enemyCheckExtraDistance),
            b.center.y
        );

        // radius based on collider height so it actually hits other enemies
        float enemyCheckRadius = Mathf.Max(0.05f, b.extents.y * 0.35f);

        Collider2D hitEnemy = Physics2D.OverlapCircle(enemyCheckPos, enemyCheckRadius, enemyLayer);

        // Ignore self if overlap happens to touch our own collider
        bool enemyAhead = hitEnemy != null && hitEnemy != col;

        // Flip rules
        if (!groundAhead || wallAhead)
        {
            Flip();
            wasEnemyAhead = false;
        }
        else
        {
            // Flip only when an enemy first appears ahead (prevents flip-flip jitter)
            if (enemyAhead && !wasEnemyAhead)
                Flip();

            wasEnemyAhead = enemyAhead;
        }

        // Move
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (wallCheck != null)
        {
            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + (Vector3)(Vector2.right * direction * wallCheckDistance)
            );
        }

        // Enemy check gizmo (only works in play mode if collider exists)
        var c = GetComponent<Collider2D>();
        if (c != null)
        {
            Bounds b = c.bounds;
            Vector2 enemyCheckPos = new Vector2(
                b.center.x + direction * (b.extents.x + enemyCheckExtraDistance),
                b.center.y
            );
            float enemyCheckRadius = Mathf.Max(0.05f, b.extents.y * 0.35f);

            Gizmos.DrawWireSphere(enemyCheckPos, enemyCheckRadius);
        }
    }
}