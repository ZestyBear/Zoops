using UnityEngine;

[CreateAssetMenu(menuName = "Zoops/World Config", fileName = "WorldConfig")]
public sealed class WorldConfig : ScriptableObject
{
    [Header("World Bounds (authoritative)")]
    public float minX = -10f;
    public float maxX =  10f;
    public float minY = -10f;
    public float maxY =  10f;

    [Header("Zoop Energy")]
    [Min(0f)] public float zoopStartingEnergy = 20f;
    [Min(0f)] public float zoopMaxEnergy = 20f;

    [Header("Metabolism")]
    [Min(0f)] public float zoopMetabolismPerSecond = 1f;

    [Header("Food")]
    public float foodSpawnX = 4f;
    public float foodSpawnY = 2f;
    [Min(0f)] public float foodEnergyGain = 6f;
    [Min(0f)] public float foodRespawnSeconds = 3f;

    [Header("Movement / Eating (MVP)")]
    [Min(0f)] public float zoopSpeed = 2.5f;
    [Min(0f)] public float eatRadius = 0.4f;
}
