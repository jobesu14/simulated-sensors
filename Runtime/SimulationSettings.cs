using UnityEngine;

[CreateAssetMenu]
public class SimulationSettings : ScriptableObject
{
    public int targetCameraFps = 30;

    public enum UpdateScheme { renderUpdate, physxUpdate }
    public UpdateScheme motionAndSensingUpdateScheme = UpdateScheme.physxUpdate;

    public string dataOutputDirectory = @"D:\missions\vio\test";

}
