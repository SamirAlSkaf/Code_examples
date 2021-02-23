using UnityEngine;
using MLAgents;

public class InsecureLamp : MonoBehaviour
{
    Light lamp;
    private float time;

    public float rangeMin = 4.0f;
    public float rangeMax = 10.0f;
    public float intensityMin = 2.0f;
    public float intensityMax = 8.0f;
    public float timeMin = 2.0f;
    public float timeMax = 8.0f;

    void Start()
    {
        lamp = this.GetComponent<Light>();
    }

    void Update()
    {
        if (time <= 0.0f)
        {
            lamp.intensity = Random.Range(intensityMin, intensityMax);
            lamp.range = Random.Range(rangeMin, rangeMax);
            time = Random.Range(timeMin, timeMax);
        }

        time -= Time.deltaTime;
    }
}
