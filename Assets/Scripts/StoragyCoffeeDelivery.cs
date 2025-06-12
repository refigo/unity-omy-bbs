using System;
using UnityEngine;
using UnityEngine.AI;

public class StoragyCoffeeDelivery : MonoBehaviour, ICafeObjectParent {


    public event EventHandler OnStoragyArrivedAtBBS;


    // public static StoragyCoffeeDelivery Instance { get; private set; }


    [SerializeField] private UR3eOnRailPickAndPlace ur3eOnRailPickAndPlace;
    [SerializeField] private Transform cafeObjectGrabPoint;
    // [SerializeField] private CafeObject cafeObjectToGet;
    [SerializeField] private Transform bbsPickupPoint;
    [SerializeField] private Transform xDoorInPoint;
    [SerializeField] private Transform xDoorOutPoint;
    [SerializeField] private Transform firstFloorHallPoint;
    [SerializeField] private Transform yDoorOutPoint;
    [SerializeField] private Transform yDoorInPoint;
    [SerializeField] private Transform loungeYPoint;

    
    private StoragyState currentState = StoragyState.Idle;
    private MovingToLoungeYState movingToLoungeYState = MovingToLoungeYState.Idle;
    private NavMeshAgent agent;
    private bool isManualMoving = false;
    private CafeObject cafeObject;


    private enum StoragyState
    {
        Idle,
        MovingToBBS,
        WaitingPickupDone,
        MovidngToLoungeY,
        ReturnToHome
    }

    private enum MovingToLoungeYState
    {
        Idle,
        MovingToXDoorIn,
        MovingToXDoorOut,
        MovingToHall,
        MovingToYDoorOut,
        MovingToYDoorIn,
        MovingToLoungeYCenter,
        ReturnToHome
    }


    void Start() {
        ur3eOnRailPickAndPlace.OnPickAndPlaceDone += UR3EPickAndPlace_OnPickAndPlaceDone;
        

        agent = GetComponent<NavMeshAgent>();
        
        if (bbsPickupPoint != null) {
            agent.SetDestination(bbsPickupPoint.position);
            isManualMoving = false;
            currentState = StoragyState.MovingToBBS;
        }
    }

    private void UR3EPickAndPlace_OnPickAndPlaceDone(object sender, EventArgs e) {
        currentState = StoragyState.MovidngToLoungeY;
        Debug.Log("To MovidngToLoungeY");
        isManualMoving = false;
        movingToLoungeYState = MovingToLoungeYState.MovingToXDoorIn;
        agent.SetDestination(xDoorInPoint.position);
    }

    void Update() {
        if (currentState == StoragyState.MovingToBBS) {
            if (!isManualMoving && (agent.remainingDistance <= agent.stoppingDistance)) {
                isManualMoving = true;
            }
            if (isManualMoving) {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.zero);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 15f);
                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f) {
                    currentState = StoragyState.WaitingPickupDone;
                    // Debug.Log("To WaitingPickupDone");
                    OnStoragyArrivedAtBBS?.Invoke(this, EventArgs.Empty);
                }
            }
        } else if (currentState == StoragyState.WaitingPickupDone) {
            // Debug.Log("In WaitingPickupDone");
        } else if (currentState == StoragyState.MovidngToLoungeY) {
            // Debug.Log("In MovidngToLoungeY");
            if (movingToLoungeYState == MovingToLoungeYState.MovingToXDoorIn) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    movingToLoungeYState = MovingToLoungeYState.MovingToXDoorOut;
                    agent.SetDestination(xDoorOutPoint.position);
                }
            } else if (movingToLoungeYState == MovingToLoungeYState.MovingToXDoorOut) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    movingToLoungeYState = MovingToLoungeYState.MovingToHall;
                    agent.SetDestination(firstFloorHallPoint.position);
                }
            } else if (movingToLoungeYState == MovingToLoungeYState.MovingToHall) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    movingToLoungeYState = MovingToLoungeYState.MovingToYDoorOut;
                    agent.SetDestination(yDoorOutPoint.position);
                }
            } else if (movingToLoungeYState == MovingToLoungeYState.MovingToYDoorOut) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    movingToLoungeYState = MovingToLoungeYState.MovingToLoungeYCenter;
                    agent.SetDestination(yDoorInPoint.position);
                }
            } else if (movingToLoungeYState == MovingToLoungeYState.MovingToYDoorIn) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    movingToLoungeYState = MovingToLoungeYState.MovingToLoungeYCenter;
                    agent.SetDestination(loungeYPoint.position);
                }
            } else if (movingToLoungeYState == MovingToLoungeYState.MovingToLoungeYCenter) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    currentState = StoragyState.Idle;
                    Debug.Log("To Idle");
                }
            }
        }
    }

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
