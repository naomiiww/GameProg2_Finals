using UnityEngine;

public class EnemyActions : EnemyBase
{
    public void Awake()
    {
        Debug.Log("Enemies are spawning!");
    }
    
    public float TravelTime = 3;

    private float timer;
    private bool toB = true;

    void Update()
    {
        Vector3 pointA = new Vector3(-15, 4.14f, 0.31686f);
        Vector3 pointB = new Vector3(15, 4.14f, 0.31686f);
        Vector3 start, towards;

        timer += Time.deltaTime;
        float time = timer / TravelTime;

        if (toB)
        {
            start = pointA;
            towards = pointB;
        }
        else
        {
            start = pointB;
            towards = pointA;
        }

        transform.position = Vector3.Lerp(start, towards, time);

        if (time > 1f)
        {
            timer = 0;
            toB = !toB;
        }
    }

    private void OnMouseHover()
    {
        Debug.Log("Enemy has been hovered over!");
    }

    private void OnMouseDown()
    {
        Debug.Log("Enemy has been hit!");
        GameManager.Instance.AddScore(5);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "DanZone1" || collision.gameObject.name == "DanZone2")
        {
            Debug.Log("Enemy has entered the hazard zone!");
            GameManager.Instance.SubScore(2);
        }
    }
}
