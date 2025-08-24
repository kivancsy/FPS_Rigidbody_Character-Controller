using UnityEngine;

public class Shotty : Weapon
{
    [Header("Shotgun Settings")] [SerializeField]
    private int pelletCount = 10;

    [SerializeField] private float spreadAngle = 5f;
    [SerializeField] private float range = 50f;
    [SerializeField] ParticleSystem muzzleFlash;

    private AnimationTriggers animTriggers;

    void Awake()
    {
        animTriggers = GetComponent<AnimationTriggers>();
    }

    public override void Fire()
    {
        if (Time.time < lastFireTime + 1f / fireRate) return;
        if (ammoCapacity == 0) return;
        lastFireTime = Time.time;
        //animator.Play(SHOOT_ANIM, 0, 0f);
        animator.SetTrigger("Shot");
        Debug.Log("Shot trigger set.");
    }
}