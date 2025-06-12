using UnityEngine;

public class UR3eJointsPose : MonoBehaviour
{
    private ArticulationBody[] joints;

    public float[] targetPosition = { -90f, -49f, -100f, 150f, -90f, 90f };
    
    void Start() {
        joints = GetComponentsInChildren<ArticulationBody>();

        // Index 0 is base_link_inertia
        SetJointAngle(1, targetPosition[0]);
        SetJointAngle(2, targetPosition[1]);
        SetJointAngle(3, targetPosition[2]);
        SetJointAngle(4, targetPosition[3]);
        SetJointAngle(5, targetPosition[4]);
        SetJointAngle(6, targetPosition[5]);
    }

    void Update() {
        // SetJointAngle(1, targetPosition[0]);
        // SetJointAngle(2, targetPosition[1]);
        // SetJointAngle(3, targetPosition[2]);
        // SetJointAngle(4, targetPosition[3]);
        // SetJointAngle(5, targetPosition[4]);
        // SetJointAngle(6, targetPosition[5]);
        if (Input.GetKeyDown(KeyCode.Space)) {
            SetJointAngle(1, targetPosition[0]);
            SetJointAngle(2, targetPosition[1]);
            SetJointAngle(3, targetPosition[2]);
            SetJointAngle(4, targetPosition[3]);
            SetJointAngle(5, targetPosition[4]);
            SetJointAngle(6, targetPosition[5]);
        }
    }

    void SetJointAngle(int jointIndex, float targetAngle) {
        if (jointIndex < joints.Length) {
            var drive = joints[jointIndex].xDrive;
            drive.target = targetAngle;
            joints[jointIndex].xDrive = drive;
        }
    }

    public void SetAllJoints(double[] jointDegrees) {
        for (int i = 1; i <= 6; i++) {
            SetJointAngle(i, (float)jointDegrees[i - 1]);
        }
    }
}
