using UnityEngine;

public class MoveTest : MonoBehaviour
{
    public SimulationSettings simuSetting;

    public bool localSpace = true;

    public bool translation = false;
    public Vector3 translationSpeed;

    public bool rotation = false;
    public Vector3 rotationSpeed;

    void Update()
    {
        if (simuSetting.motionAndSensingUpdateScheme == SimulationSettings.UpdateScheme.renderUpdate)
        {
            if (translation)
            {
                transform.Translate(translationSpeed * Time.deltaTime, localSpace ? Space.Self : Space.World);
            }

            if (rotation)
            {
                transform.Rotate(rotationSpeed * Time.deltaTime, localSpace ? Space.Self : Space.World);
            }
        }
    }

    void FixedUpdate()
    {
        if (simuSetting.motionAndSensingUpdateScheme == SimulationSettings.UpdateScheme.physxUpdate)
        {
            if (translation)
            {
                transform.Translate(translationSpeed * Time.fixedDeltaTime, localSpace ? Space.Self : Space.World);
            }

            if (rotation)
            {
                transform.Rotate(rotationSpeed * Time.fixedDeltaTime, localSpace ? Space.Self : Space.World);
            }
        }
    }
}
