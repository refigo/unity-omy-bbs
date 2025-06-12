using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosJointState = RosMessageTypes.Sensor.JointStateMsg;
using Unity.Robotics.UrdfImporter;
using Unity.Robotics.UrdfImporter.Control;

public class RosJointStatesSubscriber : MonoBehaviour
{
    // Reference to the robot's root GameObject
    public GameObject robotRoot;

    // Dictionary to map joint names to their corresponding articulation bodies
    private Dictionary<string, ArticulationBody> jointDict = new Dictionary<string, ArticulationBody>();

    // Joint names in the Open Manipulator Y (update these if your robot has different joint names)
    private readonly string[] jointNames = {
        "joint1",     // waist joint
        "joint2",     // shoulder joint
        "joint3",     // elbow joint
        "joint4",     // wrist1 joint
        "joint5",     // wrist2 joint
        "joint6",     // wrist3 joint
        // "gripper"     // gripper joint
    };

    void Start()
    {
        // If robotRoot is not assigned in the Inspector, try to find it
        if (robotRoot == null)
        {
            // Try to find a GameObject with UrdfRobot component
            UrdfRobot urdfRobot = FindObjectOfType<UrdfRobot>();
            if (urdfRobot != null)
            {
                robotRoot = urdfRobot.gameObject;
                Debug.Log("Found robot root: " + robotRoot.name);
            }
            else
            {
                Debug.LogError("Robot root not found! Please assign the robot root GameObject in the inspector.");
                return;
            }
        }

        // Find and map all articulation bodies to their joint names
        MapJointNamesToArticulationBodies();

        // Subscribe to the ROS joint_states topic
        ROSConnection.GetOrCreateInstance().Subscribe<RosJointState>("joint_states", JointStateChange);
        Debug.Log("Subscribed to joint_states topic");
    }

    void MapJointNamesToArticulationBodies()
    {
        // Clear the dictionary
        jointDict.Clear();

        // Get all articulation bodies in the robot
        ArticulationBody[] articulationBodies = robotRoot.GetComponentsInChildren<ArticulationBody>();
        Debug.Log($"Found {articulationBodies.Length} articulation bodies");

        // Map each articulation body to its joint name
        foreach (ArticulationBody joint in articulationBodies)
        {
            // Skip the root articulation body if it exists
            if (joint.isRoot) continue;

            // The joint name is the GameObject name
            string jointName = joint.name;
            
            // Replace "link" prefix with "joint" if present (common in URDF imports)
            if (jointName.StartsWith("link"))
            {
                jointName = "joint" + jointName.Substring(4);
            }

            // Add to dictionary
            jointDict[jointName] = joint;
            Debug.Log($"Mapped joint: {jointName}");
        }
    }

    void JointStateChange(RosJointState jointStateMessage)
    {
        // Test
        // Debug.Log(jointStateMessage);
        // return;


        // Process the received joint state message
        for (int i = 0; i < jointStateMessage.name.Length; i++)
        {
            string name = jointStateMessage.name[i];
            double position = jointStateMessage.position[i];

            // Check if this joint exists in our dictionary
            if (jointDict.TryGetValue(name, out ArticulationBody joint))
            {
                // Get the current drive
                ArticulationDrive drive = joint.xDrive;
                
                // Convert from radians to degrees if needed
                float targetPositionDegrees = (float)position * Mathf.Rad2Deg;
                
                // Set the target position
                drive.target = targetPositionDegrees;
                
                // Apply the drive back to the joint
                joint.xDrive = drive;
                
                Debug.Log($"Applied position {targetPositionDegrees} degrees to joint {name}");
            }
            else
            {
                Debug.LogWarning($"Joint {name} not found in the robot!");
            }
        }
    }
}