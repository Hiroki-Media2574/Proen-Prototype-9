using UnityEngine;
using TMPro;

public class NPCController : MonoBehaviour
{
    public Transform target;
    public float maxDistance = 0.0f;
    public TextMeshProUGUI MessageText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxDistance))
        {
            // 当たったオブジェクトの名前をコンソールに表示
            Debug.Log("当たったオブジェクト: " + hit.collider.name);
            transform.LookAt(target);
        }
        //transform.LookAt(target);
    }
}
