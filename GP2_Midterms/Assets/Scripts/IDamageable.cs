using UnityEngine;

public interface IDamageable
{
    //If a colored enemy is hit by a bullet of the SAME color, the interface is triggered, and both the enemy and the bullet are destroyed.
    //If an enemy is hit by a bullet of a DIFFERENT color, only the bullet is destroyed (the enemy ignores the damage).
    void TakeDamage(ColorSet bulletColor);
}
