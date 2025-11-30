using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;   
    public Vector2 direction;   
    public float life = 2f;    

    void Start()
    {
        // destroy bullet so it doesn't sit in the scene forever
        Destroy(gameObject, life);
    }

    void Update()
    {
        // just move the bullet in whatever direction it was given
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
