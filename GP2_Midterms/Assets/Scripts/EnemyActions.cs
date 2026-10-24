using UnityEngine;

public class EnemyActions : EnemyBase
{   
    protected override void moveTowardsTarget()
    {
        Vector2 directionToPlayer = (Vector2)playerTarget.position - rb.position;

        if (directionToPlayer.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector2 direction = directionToPlayer.normalized;

        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

        UpdateFacingSprite(direction);
    }
}
