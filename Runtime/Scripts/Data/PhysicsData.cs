using UdonSharp;

public class PhysicsData : UdonSharpBehaviour
{
    public float dragOnAir = 0.1f;
    public float dragOnWater = 5f;
    public float angularDragOnAir = 0.01f;
    public float angularDragOnWater = 1f;
    public float playerMass = 50f;
    public float playerHeight = 1.65f;
    public float playerRadius = 0.11f;
}
