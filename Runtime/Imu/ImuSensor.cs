using UnityEngine;

public class ImuSensor : MonoBehaviour
{

    // Don't forget to set this script to negative value on script execution order.

    public SimulationSettings simuSetting;

    private Transform mTrans;
    private Vector3 prevSpeed = Vector3.zero;
    private Vector3 prevPosition = Vector3.zero;
    private Quaternion prevRotation = Quaternion.identity;
    private Vector3 lastAccelearation = Vector3.zero;
    private Vector3 lastAngularSpeed = Vector3.zero;
    private float lastTime;

    public void GetAcceleration(out Vector3 acceleration, out float time)
    {
        acceleration = lastAccelearation;
        time = lastTime;
    }

    public void GetAugularSpeed(out Vector3 angularSpeed, out float time)
    {
        angularSpeed = lastAngularSpeed;
        time = lastTime;
    }

    public void GetImuData(out Vector3 acceleration, out Vector3 angularSpeed, out float time)
    {
        acceleration = lastAccelearation;
        angularSpeed = lastAngularSpeed;
        time = lastTime;
    }

    private void Awake()
    {
        mTrans = this.transform;
        prevPosition = mTrans.position;
        prevRotation = mTrans.rotation;
    }

    private void Update()
    {
        if(simuSetting.motionAndSensingUpdateScheme == SimulationSettings.UpdateScheme.renderUpdate)
        {
            lastTime = Time.time;
            lastAccelearation = ComputeAcceleration(ref prevSpeed, prevPosition, mTrans.position, Time.deltaTime);
            lastAngularSpeed = ComputeAngularSpeed(prevRotation, mTrans.rotation, Time.deltaTime);
            prevPosition = mTrans.position;
            prevRotation = mTrans.rotation;
        }
    }

    private void FixedUpdate()
    {
        if(simuSetting.motionAndSensingUpdateScheme == SimulationSettings.UpdateScheme.physxUpdate)
        {
            lastTime = Time.fixedTime;
            lastAccelearation = ComputeAcceleration(ref prevSpeed, prevPosition, mTrans.position, Time.fixedDeltaTime);
            lastAngularSpeed = ComputeAngularSpeed(prevRotation, mTrans.rotation, Time.fixedDeltaTime);
            prevPosition = mTrans.position;
            prevRotation = mTrans.rotation;
        }
    }

    private Vector3 ComputeAcceleration(ref Vector3 prevSpeed, Vector3 prevPosition, Vector3 curPosition, float dt)
    {
        Vector3 curSpeed = (curPosition - prevPosition) / dt;
        Vector3 acceleration = (curSpeed - prevSpeed) / dt;
        prevSpeed = curSpeed;
        Vector3 globalAcceleration = acceleration + Physics.gravity;
        return mTrans.InverseTransformVector(globalAcceleration);
    }

    private Vector3 ComputeAngularSpeed(Quaternion prevRotation, Quaternion curRotation, float dt)
    {
	// TODO works only when angular speed is low.
        Quaternion deltaRotation = curRotation * Quaternion.Inverse(prevRotation);
        Vector3 deltaAngles = deltaRotation.eulerAngles;
        FormatAngle(ref deltaAngles);
        Vector3 globalAngularSpeed = deltaAngles / dt;
        return mTrans.InverseTransformVector(globalAngularSpeed);
    }

    private static void FormatAngle(ref Vector3 angles)
    {
        // WARNING: works only if delta angle < 180° per frame (<9000°/s @ 50Hz).
        if (angles.x > 180f)
        {
            angles.x = -(360f - angles.x);
        }
        if (angles.y > 180f)
        {
            angles.y = -(360f - angles.y);
        }
        if (angles.z > 180f)
        {
            angles.z = -(360f - angles.z);
        }
    }
}
