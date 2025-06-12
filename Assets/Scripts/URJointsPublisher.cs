using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityUrPkg;
// using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.UrdfImporter;

public class URJointsPublisher : MonoBehaviour
{
    ROSConnection ros;
    [SerializeField] string topicName = "ur_joints";


    const int k_NumRobotJoints = 6;


    public static readonly string[] LinkNames = {
        "shoulder_link",
        "upper_arm_link",
        "forearm_link",
        "wrist_1_link",
        "wrist_2_link",
        "wrist_3_link"
    };



    [SerializeField]
    GameObject m_UR_Robot;

    [SerializeField]
    float publishMessageFrequency = 0.5f;


    private float timeElapsed;

    // Robot Joints
    UrdfJointRevolute[] m_JointArticulationBodies;

    void Start() {
        // Register the message type first
        RosMessageTypes.UnityUrPkg.UrJointsMsg.Register();
        
        ros = ROSConnection.GetOrCreateInstance();
        
        // Ensure ROS connection is established
        ros.Connect();
        
        // Register publisher with the correct message type
        ros.RegisterPublisher<RosMessageTypes.UnityUrPkg.UrJointsMsg>(topicName);
        
        m_JointArticulationBodies = new UrdfJointRevolute[k_NumRobotJoints];

        var linkName = string.Empty;
        for (var i = 0; i < k_NumRobotJoints; i++) {
            linkName = LinkNames[i];
            var links = m_UR_Robot.GetComponentsInChildren<UrdfJointRevolute>();
            foreach (var link in links) {
                if (link.name == linkName) {
                    m_JointArticulationBodies[i] = link;
                }
            }
        }
    }

    void Update() {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency) {
            double[] positions = new double[k_NumRobotJoints];
            for (var i =0; i < k_NumRobotJoints; i++) {
                // Debug.Log(m_JointArticulationBodies[i]);
                // Debug.Log(m_JointArticulationBodies[i].GetPosition());
                positions[i] = m_JointArticulationBodies[i].GetPosition();
            }
            UrJointsMsg jointMsg = new UrJointsMsg(positions);
            Debug.Log(jointMsg);

            ros.Publish(topicName, jointMsg);

            timeElapsed = 0;
        }
    }
}
