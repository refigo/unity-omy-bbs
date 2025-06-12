using System;
using UnityEngine;
using System.Collections;

public class UR3ePickAndPlace : MonoBehaviour, ICafeObjectParent {


    public event EventHandler OnPickAndPlaceDone;


    [SerializeField] private StoragyCoffeeDelivery storagyCoffeeDelivery;
    [SerializeField] private Transform cafeObjectGrabPoint;
    [SerializeField] private CafeObject cafeObjectToGet;


    private enum RobotState
    {
        Idle,
        MovingToHome,
        MovingToPick,
        Gripping,
        MovingToPickAbove,
        MovingToPlaceAbove,
        MovingToPlace,
        Releasing,
        ReturnToHome
    }


    private RobotState currentState = RobotState.Idle;
    private float moveSpeed = 50.0f; // angle / seconds
    private ArticulationBody[] joints;
    private float[] homePosition = { 90f, -49f, -100f, 150f, -90f, 0f };
    private float[] pickPosition = { 33.5f, -115.5f, -116f, 235f, -90f, 0f };
    private float[] pickPositionAbove = { 33.5f, -90f, -120f, 220f, -90f, 0f };
    private float[] placePositionAbove = { -70f, -110f, -70f, 195f, -90f, 0f };
    private float[] placePosition = { -70f, -150f, -80f, 230f, -90f, 0f };
    private CafeObject cafeObject;
    
    
    private void Start()
    {
        storagyCoffeeDelivery.OnStoragyArrivedAtBBS += StoragyCoffeeDelivery_OnStoragyArrivedAtBBS;
        
        joints = GetComponentsInChildren<ArticulationBody>();
        SetJointAngle(1, homePosition[0]);
        SetJointAngle(2, homePosition[1]);
        SetJointAngle(3, homePosition[2]);
        SetJointAngle(4, homePosition[3]);
        SetJointAngle(5, homePosition[4]);
        SetJointAngle(6, homePosition[5]);

        // StartCoroutine(PickAndPlaceSequence());
    }

    private void StoragyCoffeeDelivery_OnStoragyArrivedAtBBS(object sender, System.EventArgs e)
    {
        Debug.Log("StoragyCoffeeDelivery_OnStoragyArrivedAtBBS");
        StartCoroutine(PickAndPlaceSequence());
    }

    void SetJointAngle(int jointIndex, float targetAngle) {
        if (jointIndex < joints.Length) {
            var drive = joints[jointIndex].xDrive;
            drive.target = targetAngle;
            joints[jointIndex].xDrive = drive;
        }
    }

    IEnumerator PickAndPlaceSequence()
    {
        while (true)
        {
            switch (currentState)
            {
                case RobotState.Idle:
                    currentState = RobotState.MovingToHome;
                    break;

                case RobotState.MovingToHome:
                    if (MoveToTarget(homePosition))
                    {
                        yield return new WaitForSeconds(1.0f);
                        currentState = RobotState.MovingToPick;
                    }
                    break;

                case RobotState.MovingToPick:
                    if (MoveToTarget(pickPosition))
                    {
                        Debug.Log("MovingToPick");
                        yield return new WaitForSeconds(1.0f);
                        currentState = RobotState.Gripping;
                    }
                    break;

                case RobotState.Gripping:
                    // 여기에 그리퍼 제어 코드 추가
                    cafeObjectToGet.SetCafeObjectParent(this);

                    yield return new WaitForSeconds(1.0f);
                    currentState = RobotState.MovingToPickAbove;
                    break;
                
                case RobotState.MovingToPickAbove:

                    if (MoveToTarget(pickPositionAbove)) {
                    yield return new WaitForSeconds(1.0f);
                        currentState = RobotState.MovingToPlaceAbove;
                    }
                    break;

                case RobotState.MovingToPlaceAbove:
                    if (MoveToTarget(placePositionAbove)) {
                        yield return new WaitForSeconds(1.0f);
                        currentState = RobotState.MovingToPlace;
                    }
                    break;

                case RobotState.MovingToPlace:
                    if (MoveToTarget(placePosition))
                    {
                        yield return new WaitForSeconds(1.0f);
                        cafeObject.SetCafeObjectParent(storagyCoffeeDelivery);
                        OnPickAndPlaceDone?.Invoke(this, EventArgs.Empty);
                        currentState = RobotState.Releasing;
                    }
                    break;

                case RobotState.Releasing:
                    // 여기에 그리퍼 해제 코드 추가
                    yield return new WaitForSeconds(1.0f);
                    currentState = RobotState.ReturnToHome;
                    break;

                case RobotState.ReturnToHome:
                    if (MoveToTarget(homePosition))
                    {
                        yield return new WaitForSeconds(2.0f);
                        // currentState = RobotState.MovingToPick;
                    }
                    break;
            }
            yield return null;
        }
    }

    private bool MoveToTarget(float[] targetAngles)
    {
        bool allJointsReached = true;
        
        for (int i = 1; i < 7; i++)
        {
            // Debug: Fast way to move to target angle
            // SetJointAngle(i, targetAngles[i - 1]);
            // continue;
            var drive = joints[i].xDrive;
            float currentAngle = drive.target;
            float targetAngle = targetAngles[i - 1];
            
            if (Mathf.Abs(currentAngle - targetAngle) > 0.1f)
            {
                float newAngle = Mathf.MoveTowards(currentAngle, targetAngle, moveSpeed * Time.deltaTime);
                drive.target = newAngle;
                joints[i].xDrive = drive;
                allJointsReached = false;
            }
        }
        
        return allJointsReached;
    }


    // 디버그용 GUI
    // void OnGUI()
    // {
    //     GUI.Label(new Rect(10, 10, 300, 20), "Current State: " + currentState.ToString());
    // }


    public Transform GetCafeObjectFollowTransform() {
        return cafeObjectGrabPoint;
    }

    public void SetCafeObject(CafeObject cafeObject) {
        this.cafeObject = cafeObject;
    }

    public CafeObject GetCafeObject() {
        return cafeObject;
    }

    public void ClearCafeObject() {
        cafeObject = null;
    }

    public bool HasCafeObject() {
        return cafeObject != null;
    }
}
