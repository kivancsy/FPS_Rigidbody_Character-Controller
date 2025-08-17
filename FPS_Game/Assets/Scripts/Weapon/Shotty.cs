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

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 direction = GetSpreadDirection(muzzle.forward, spreadAngle);

            if (Physics.Raycast(muzzle.position, direction, out RaycastHit hit, range))
            {
                //Damage Apply Here
            }

            Debug.DrawLine(muzzle.position, hit.point, Color.red, 1f);
        }


        if (animTriggers.triggerCalled)
            OnShootAnimationFinished();
    }

    private void OnShootAnimationFinished()
    {
        //animator.SetBool("isShot", false);
        Debug.Log("Shotty animation finished.");
    }

    private Vector3 GetSpreadDirection(Vector3 forward, float angle)
    {
        float randomYaw = Random.Range(-angle, angle);
        float randomPitch = Random.Range(-angle, angle);

        Quaternion rotation = Quaternion.Euler(randomPitch, randomYaw, 0f);
        return rotation * forward;
    }
}