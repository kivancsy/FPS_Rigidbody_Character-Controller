using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected Transform muzzle;
    [SerializeField] protected Animator animator;

    [SerializeField] protected int ammoCapacity;

    protected float lastFireTime;

    public abstract void Fire();
}