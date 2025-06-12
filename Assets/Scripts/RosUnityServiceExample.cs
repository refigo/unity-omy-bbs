using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.UnityRoboticsDemo;

public class RosUnityServiceExample : MonoBehaviour
{
    [SerializeField]
    string m_ServiceName = "obj_pose_srv";

    void Start() {
        // register the service with ROS
        ROSConnection
            .GetOrCreateInstance()
            .ImplementService<ObjectPoseServiceRequest, ObjectPoseServiceResponse>(
                m_ServiceName, 
                GetObjectPose
            );
    }

    private ObjectPoseServiceResponse GetObjectPose(ObjectPoseServiceRequest request) {
        Debug.Log("Received request for object: " + request.object_name);

        ObjectPoseServiceResponse objectPoseResponse = new ObjectPoseServiceResponse();
        GameObject gameObject = GameObject.Find(request.object_name);
        if (gameObject) {
            objectPoseResponse.object_pose.position = gameObject.transform.position.To<FLU>();
            objectPoseResponse.object_pose.orientation = gameObject.transform.rotation.To<FLU>();
        }

        return objectPoseResponse;
    }
}
