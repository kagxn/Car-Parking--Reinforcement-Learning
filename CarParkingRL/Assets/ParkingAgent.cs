using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ParkingAgent : Agent
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;      // Car forward/backward movement speed
    public float turnSpeed = 200f;    // Car turning speed

    [Header("Target (Parking Spot) Settings")]
    public Transform targetTransform;    // Target parking spot's Transform
    public Collider parkingSpotCollider;   // Parking spot's collider (should be set as Trigger)

    [Header("Parking Area Settings")]
    public Collider parkingAreaCollider;    // Collider for the parking area (used for wheels)
    public float wheelAreaRewardPerSecond = 0.1f; // Reward per second when wheels are inside
    public float requiredWheelsInsideTime = 3f;   // Time (in seconds) wheels must remain inside
    public float parkingAreaTouchReward = 0.5f;     // One-time reward for first contact with Parking Area

    [Header("Episode Settings")]
    public float maxEpisodeDuration = 30f; // Maximum episode duration (30 seconds)
    public float farDistanceThreshold = 10f; // Distance threshold beyond which penalty is applied

    [Header("Reward Parameters")]
    public float distanceRewardScale = 0.1f; // Scaling factor for approaching reward
    public float parkingTouchReward = 0.5f;    // One-time reward for first contact with Parking Spot
    public float insideRewardPerSecond = 0.2f; // Reward per second when the car's center is inside the Parking Spot
    public float successReward = 2.0f;         // Successful park reward
    public float requiredInsideTime = 3f;      // Required time (in seconds) for the car's center to remain inside the Parking Spot

    private Rigidbody rb;
    private float previousDistance;   // Distance to target from previous step
    private float episodeStartTime;   // Time when episode starts
    private float insideTime = 0f;      // Time the car's center remains inside the Parking Spot
    private float wheelsInsideTime = 0f; // Time the wheels remain inside the Parking Area

    private bool parkingTouchRewardGiven = false;      // For one-time reward when first contacting the Parking Spot
    private bool parkingAreaTouchRewardGiven = false;    // For one-time reward when first contacting the Parking Area

    public static int episodeCount = 0;
    public static int successfulParkCount = 0;

    public GameObject[] wheelObjects; // GameObjects representing the wheels (assign in Inspector)

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset physical values
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Set a random starting position and rotation
        float range = 3.5f;
        transform.localPosition = new Vector3(Random.Range(-range, 1.2f), 0f, Random.Range(-range, range));
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Calculate distance to target
        previousDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

        // Record episode start time
        episodeStartTime = Time.time;
        insideTime = 0f;
        wheelsInsideTime = 0f;
        parkingTouchRewardGiven = false;
        parkingAreaTouchRewardGiven = false;

        // Increment episode count via TrainingUI
        TrainingUI ui = FindObjectOfType<TrainingUI>();
        if (ui != null)
        {
            ui.IncrementEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(Vector3.Distance(transform.localPosition, targetTransform.localPosition));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 1. Check episode duration limit (30 seconds)
        if (Time.time - episodeStartTime > maxEpisodeDuration)
        {
            SetReward(-0.5f);
            EndEpisode();
            Debug.Log("Episode duration exceeded.");
            return;
        }

        // 2. Get actions and apply movement
        float moveInput = actionBuffers.ContinuousActions[0];
        float turnInput = actionBuffers.ContinuousActions[1];
        rb.AddForce(transform.forward * moveInput * moveSpeed);
        transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);

        // 3. Approaching reward: Calculate based on distance difference
        float currentDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float distanceDelta = previousDistance - currentDistance; // Positive means approaching, negative means moving away
        AddReward(distanceDelta * distanceRewardScale);
        previousDistance = currentDistance;

        // If too far away, apply penalty and end episode
        if (currentDistance > farDistanceThreshold)
        {
            SetReward(-1.0f);
            EndEpisode();
            Debug.Log("Too far away.");
            return;
        }

        // 4. Check if the car's center (transform.position) is inside the Parking Spot
        if (parkingSpotCollider.bounds.Contains(transform.position))
        {
            insideTime += Time.deltaTime;
            AddReward(insideRewardPerSecond * Time.deltaTime);
        }
        else
        {
            insideTime = 0f;
        }

        // 5. Check if the wheels are inside the Parking Area
        Bounds areaBounds = parkingAreaCollider.bounds;
        Vector2 areaMin = new Vector2(areaBounds.min.x, areaBounds.min.z);
        Vector2 areaMax = new Vector2(areaBounds.max.x, areaBounds.max.z);

        int wheelsInsideCount = 0;
        foreach (GameObject wheel in wheelObjects)
        {
            if (wheel == null)
                continue;
            Vector2 wheelPos = new Vector2(wheel.transform.position.x, wheel.transform.position.z);
            if (wheelPos.x >= areaMin.x && wheelPos.x <= areaMax.x &&
                wheelPos.y >= areaMin.y && wheelPos.y <= areaMax.y)
            {
                wheelsInsideCount++;
            }
        }

        // If all wheels are inside, accumulate time and apply per-second reward
        if (wheelsInsideCount == wheelObjects.Length)
        {
            wheelsInsideTime += Time.deltaTime;
            AddReward(wheelAreaRewardPerSecond * Time.deltaTime);
        }
        else
        {
            wheelsInsideTime = 0f;
        }

        // 6. Successful park: Both the car's center must remain inside the Parking Spot for requiredInsideTime
        // and all wheels must remain inside the Parking Area for requiredWheelsInsideTime.
        if (insideTime >= requiredInsideTime && wheelsInsideTime >= requiredWheelsInsideTime)
        {
            SetReward(successReward);
            TrainingUI ui = FindObjectOfType<TrainingUI>();
            if (ui != null)
            {
                ui.IncrementSuccessfulParks();
            }
            EndEpisode();
            Debug.Log("Successful park: Both the car's center remained in the Parking Spot and all wheels in the Parking Area for sufficient time.");
            return;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");   // Vertical axis input
        continuousActions[1] = Input.GetAxis("Horizontal"); // Horizontal axis input
    }

    private void OnTriggerEnter(Collider other)
    {
        // One-time reward: First contact with the Parking Spot
        if (other.CompareTag("ParkingSpot") && !parkingTouchRewardGiven)
        {
            AddReward(parkingTouchReward);
            parkingTouchRewardGiven = true;
            Debug.Log("First contact with Parking Spot: reward given.");
        }
        // One-time reward: First contact with the Parking Area
        if (other.CompareTag("ParkingArea") && !parkingAreaTouchRewardGiven)
        {
            AddReward(parkingAreaTouchReward);
            parkingAreaTouchRewardGiven = true;
            Debug.Log("First contact with Parking Area: reward given.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-1.0f);
            // Call ResetManager if available to reset environment
            ResetManager resetManager = FindObjectOfType<ResetManager>();
            if (resetManager != null)
            {
                resetManager.ResetAll();
            }
            EndEpisode();
            Debug.Log("Collision with Obstacle: penalty applied and environment reset.");
        }
    }
}
