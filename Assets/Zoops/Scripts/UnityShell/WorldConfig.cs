// File: Assets/Zoops/Scripts/UnityShell/WorldConfig.cs
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Zoops/World Config", fileName = "WorldConfig")]
public sealed class WorldConfig : ScriptableObject
{
    [Header("World Bounds (authoritative)")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    [Header("Counts (initial spawns)")]
    [Min(1)] public int initialZoopCount = 8;
    [Min(1)] public int initialPlantCount = 3;

    [Header("Zoop Energy")]
    [Min(0f)] public float zoopStartingEnergy = 20f;
    [Min(0f)] public float zoopMaxEnergy = 20f;

    [Header("Metabolism")]
    [Min(0f)] public float zoopMetabolismPerSecond = 1f;

    [Header("Plant (edible entity today)")]
    [FormerlySerializedAs("foodSpawnX")]
    public float plantSpawnX = 4f;

    [FormerlySerializedAs("foodSpawnY")]
    public float plantSpawnY = 2f;

    [FormerlySerializedAs("foodEnergyGain")]
    [Min(0f)] public float plantEnergyGain = 6f;

    [FormerlySerializedAs("foodRespawnSeconds")]
    [Min(0f)] public float plantRespawnSeconds = 3f;

    [Header("Plant Population Dynamics (Spread Ecology)")]
    [Min(0)]
    [Tooltip("Floor: if total plants falls below this, rescue spawns will kick in.")]
    public int plantMinCount = 10;

    [Min(1)]
    [Tooltip("Ceiling: hard cap on total plants in the world.")]
    public int plantMaxCount = 80;

    [Min(0f)]
    [Tooltip("How often each plant attempts a local spread (seconds).")]
    public float plantSpreadIntervalSeconds = 0.75f;

    [Range(0f, 1f)]
    [Tooltip("Chance per plant per spread interval to create 1 new plant.")]
    public float plantSpreadChance = 0.25f;

    [Min(0f)]
    [Tooltip("Radius for local spread attempts (world units).")]
    public float plantSpreadRadius = 2.25f;

    [Min(0f)]
    [Tooltip("How often we check for low plants and apply rescue spawns (seconds).")]
    public float plantRescueIntervalSeconds = 1.0f;

    [Min(1)]
    [Tooltip("How many plants to add when rescuing (capped by PlantMaxCount).")]
    public int plantRescueBatchSize = 6;

    [Header("Plant Regrowth (patchiness)")]
    [Min(0f)]
    [Tooltip("When a plant regrows, it usually appears within this radius of where it was eaten (local clustering).")]
    public float plantRegrowRadius = 3.5f;

    [Range(0f, 1f)]
    [Tooltip("Chance that a regrowing plant 'long-jumps' to a random location, seeding new patches.")]
    public float plantLongJumpChance = 0.15f;

    [Min(0f)]
    [Tooltip("Inset from world bounds when placing plants, to avoid hugging the border.")]
    public float plantBoundsInset = 0.25f;

    [Header("Movement / Eating (MVP)")]
    // Kept for backward compatibility and quick tuning.
    // If geneDefaultMoveSpeed is 0, we will fall back to this.
    [Min(0f)] public float zoopSpeed = 2.5f;
    [Min(0f)] public float eatRadius = 0.4f;

    [Header("Zoop Genes (defaults)")]
    [Min(0f)] public float geneDefaultMoveSpeed = 0f;   // 0 = use zoopSpeed
    [Min(0f)] public float geneDefaultVisionRange = 6f;
    [Min(0f)] public float geneDefaultReproThreshold = 18f;

    [Header("Founder Variation (initial jitter only)")]
    [Min(0f)]
    [Tooltip("One-time gene jitter applied only to founders at Rebuild (0 = none).")]
    public float founderGeneSigmaFraction = 0.03f;

    [Header("Zoop Genes (clamps)")]
    [Min(0f)] public float geneMinMoveSpeed = 0.5f;
    [Min(0f)] public float geneMaxMoveSpeed = 6f;

    [Min(0f)] public float geneMinVisionRange = 0.5f;
    [Min(0f)] public float geneMaxVisionRange = 20f;

    [Min(0f)] public float geneMinReproThreshold = 1f;
    [Min(0f)] public float geneMaxReproThreshold = 100f;

    [Header("Reproduction")]
    [Min(0f)] public float reproCooldownSeconds = 3f;

    [Range(0f, 1f)]
    [Tooltip("Fraction of parent's energy kept by parent after reproduction. Child receives the rest.")]
    public float reproParentEnergyFraction = 0.65f;

    [Header("Mutation")]
    [Min(0f)]
    [Tooltip("Typical mutation scale as a fraction (e.g. 0.05 ~= 5%).")]
    public float mutationSigmaFraction = 0.05f;
}
