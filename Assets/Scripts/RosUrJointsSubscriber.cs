using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UrJoints = RosMessageTypes.UnityUrInterfaces.UrJointsMsg;

public class RosUrJointsSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "ur_joints_data";

    public GameObject ur_robot;

    private UR3eJointsPose ur_joints_pose;

    void Start() {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<UrJoints>(topicName, SyncJoints);

        ur_joints_pose = ur_robot.GetComponent<UR3eJointsPose>();
    }

    void SyncJoints(UrJoints jointsMessage) {
        Debug.Log("Joints message degrees:");
        // Debug.Log(jointsMessage);
        Debug.Log(jointsMessage);
        double[] check_degrees = jointsMessage.degrees;
        foreach (double each in check_degrees) {
            Debug.Log(each);
        }
        ur_joints_pose.SetAllJoints(check_degrees);
    }
}
