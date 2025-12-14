using UnityEngine;

public interface ITargetable
{
    Transform transform { get; }
    bool IsDead();
    void TakeDamage(int damage);
}