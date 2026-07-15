using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localScale = new Vector3(transform.localScale.x / transform.parent.localScale.x, transform.localScale.y / transform.parent.localScale.y, transform.localScale.z / transform.parent.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
