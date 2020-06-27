using UdonSharp;
using UnityEngine;

public class UniversalGravitation : UdonSharpBehaviour
{
    public GameObject target;
    public GameObject effector;
    public float coefficient = 6.67430e-5f;
    private Rigidbody[] targetsCache;
    private Rigidbody[] effectorsCache;

    private void Start()
    {
        Recache();
    }

    public void Recache()
    {
        targetsCache = target.GetComponentsInChildren<Rigidbody>();
        effectorsCache = effector.GetComponentsInChildren<Rigidbody>();
    }

    public void Affect()
    {
        foreach (Rigidbody m in targetsCache)
        foreach (Rigidbody o in effectorsCache)
        {
            Vector3 distance_vector = o.position - m.position;
            float power = coefficient * o.mass * m.mass / distance_vector.sqrMagnitude;
            m.AddForce(power * distance_vector.normalized);
        }
    }
}
