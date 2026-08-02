using UnityEngine;
using TMPro;

public class GameSystem : MonoBehaviour
{
    public float time;
    public TextMeshProUGUI timeText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time -= Time.fixedDeltaTime;
        timeText.text = "TIME  :  " + time;
    }
}
