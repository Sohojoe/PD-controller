using UnityEngine;


[System.Serializable]
public enum JointMode
{
	useDrive,
	useJointVelocity,
	useJointForce,
	useJointPosition,
}

public class ArticulationTest : MonoBehaviour
{
	public ArticulationBody body;
	public JointMode jointMode = JointMode.useDrive;

	public float Strength = 1f;
	public Vector3 jointPosition;

	void FixedUpdate()
	{
		var jPos = body.jointPosition;
		jointPosition = new Vector3(jPos[0], jPos[1], jPos[2]);
		jointPosition *= Mathf.Rad2Deg;

		if (jointMode == JointMode.useDrive)
		{
			// Normalize Quaternion to suppress greater than 360 degree rotations
			Quaternion normalizedRotation = Quaternion.Normalize(transform.rotation);

			// Calculate drive targets
			Vector3 driveTarget = new Vector3(
				Mathf.DeltaAngle(0, normalizedRotation.eulerAngles.x),
				Mathf.DeltaAngle(0, normalizedRotation.eulerAngles.y),
				Mathf.DeltaAngle(0, normalizedRotation.eulerAngles.z));

			// Copy the X, Y, and Z target values into the drives...
			ArticulationDrive xDrive = body.xDrive; xDrive.target = driveTarget.x; body.xDrive = xDrive;
			ArticulationDrive yDrive = body.yDrive; yDrive.target = driveTarget.y; body.yDrive = yDrive;
			ArticulationDrive zDrive = body.zDrive; zDrive.target = driveTarget.z; body.zDrive = zDrive;
		}
		else
		{

			Quaternion deltaRotation = Quaternion.Normalize(Quaternion.Inverse(body.transform.localRotation) * transform.rotation);
			// Calculate drive velocity necessary to undo this delta in one fixed timestep
			ArticulationReducedSpace driveTargetForce = new ArticulationReducedSpace(
				((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.x) * Mathf.Deg2Rad) / Time.fixedDeltaTime) * Strength,
				((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.y) * Mathf.Deg2Rad) / Time.fixedDeltaTime) * Strength,
				((Mathf.DeltaAngle(0, deltaRotation.eulerAngles.z) * Mathf.Deg2Rad) / Time.fixedDeltaTime) * Strength);

			// Apply the force in local space (unlike AddTorque which is global space)
			// Ideally we'd use inverse dynamics or jointVelocity, but jointVelocity is bugged in 2020.1a22
			if (jointMode == JointMode.useJointForce)
			{
				body.jointForce = driveTargetForce;
			}
			else if (jointMode == JointMode.useJointVelocity)
			{
				body.jointVelocity = driveTargetForce;
			}
			else if (jointMode == JointMode.useJointPosition)
			{
				Quaternion normalizedRotation = Quaternion.Normalize(transform.rotation);
				ArticulationReducedSpace driveTarget = new ArticulationReducedSpace(
					Mathf.DeltaAngle(0, normalizedRotation.eulerAngles.x) * Mathf.Deg2Rad,
					Mathf.DeltaAngle(0, normalizedRotation.eulerAngles.y) * Mathf.Deg2Rad,
					Mathf.DeltaAngle(0, normalizedRotation.eulerAngles.z) * Mathf.Deg2Rad);
				body.jointPosition = driveTarget;
			}
		}
	}
}
