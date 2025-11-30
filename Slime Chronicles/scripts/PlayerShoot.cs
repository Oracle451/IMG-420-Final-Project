using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;      
    public Transform firePoint;         
    public float fireRate = 0.25f;   

    private float nextShootTime = 0f;

    void Update()
    {
        // left click shoots 
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextShootTime)
            {
                Shoot();
                nextShootTime = Time.time + fireRate;
            }
        }
    }

    void Shoot()
    {
        // spawn the bullet at the firepoint
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // figure out where mouse is in world space
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouse - firePoint.position).normalized;

        // give bullet a direction to travel
        Bullet bulletScript = b.GetComponent<Bullet>();
        bulletScript.direction = dir;
    }
}
