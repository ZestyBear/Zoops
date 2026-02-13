// File: Assets/Zoops/Scripts/UnityShell/SimulationRunner.cs
using System;
using UnityEngine;
using Zoops.Simulation;

public class SimulationRunner : MonoBehaviour
{
    [Header("Sim Settings")]
    [SerializeField] private int seed = 12345;

    [Header("Config (authoritative knobs)")]
    [SerializeField] private WorldConfig config;

    [Header("Time Controls")]
    [Tooltip("Overall multiplier on how fast sim time advances relative to real time.")]
    [SerializeField, Range(0f, 10f)] private float simSpeed = 1f;

    [Tooltip("Physics ticks per second (movement, eating, timers).")]
    [SerializeField, Range(1f, 120f)] private float physicsHz = 10f;

    [Tooltip("Brain ticks per second (decision / intent updates).")]
    [SerializeField, Range(0.1f, 60f)] private float brainHz = 8f;

    [Header("Debug")]
    [SerializeField] private bool logTicks = false;

    private SimulationWorld _world;
    public SimulationWorld World => _world;

    public float SimTickSeconds => physicsHz > 0f ? (1f / physicsHz) : 0f;

    public event Action WorldChanged;
    public event Action<SimEvent> SimEventRaised;

    private float _physicsAccum;
    private float _brainAccum;

    private void Start()
    {
        CreateWorld();
    }

    private void Update()
    {
        if (_world == null) return;
        if (simSpeed <= 0f) return;

        float scaledDt = Time.deltaTime * simSpeed;

        float physicsDt = 1f / physicsHz;
        float brainDt = 1f / brainHz;

        _physicsAccum += scaledDt;
        _brainAccum += scaledDt;

        // Brain lane first so physics consumes freshest intent.
        while (_brainAccum >= brainDt)
        {
            _world.StepBrainTick(brainDt);
            _brainAccum -= brainDt;
        }

        while (_physicsAccum >= physicsDt)
        {
            _world.StepOneTick(physicsDt);
            _physicsAccum -= physicsDt;

            if (logTicks)
                Debug.Log($"[Sim] Tick {_world.Tick}");
        }
    }

    public void RebuildWorld()
    {
        CreateWorld();
    }

    private void CreateWorld()
    {
        _physicsAccum = 0f;
        _brainAccum = 0f;

        if (config == null)
        {
            Debug.LogError("[Sim] SimulationRunner has no WorldConfig assigned. Create one (Assets > Create > Zoops > World Config) and assign it.");
            return;
        }

        float startE = Mathf.Max(0f, config.zoopStartingEnergy);
        float maxE = Mathf.Max(0f, config.zoopMaxEnergy);

        if (maxE <= 0f) maxE = startE;
        if (maxE < startE) maxE = startE;

        float geneDefaultMoveSpeed = config.geneDefaultMoveSpeed;
        if (geneDefaultMoveSpeed <= 0f)
            geneDefaultMoveSpeed = Mathf.Max(0f, config.zoopSpeed);

        // Spread Ecology sanity
        int plantMin = Mathf.Max(0, config.plantMinCount);
        int plantMax = Mathf.Max(1, config.plantMaxCount);
        if (plantMax < plantMin) plantMax = plantMin;

        int initialPlantCount = Mathf.Max(1, config.initialPlantCount);
        if (initialPlantCount > plantMax) initialPlantCount = plantMax;

        int rescueBatch = Mathf.Max(1, config.plantRescueBatchSize);

        var p = new SimulationParams(
            // energy
            zoopStartingEnergy: startE,
            zoopMaxEnergy: maxE,
            zoopMetabolismPerSecond: Mathf.Max(0f, config.zoopMetabolismPerSecond),

            // plant (edible entity today)
            plantSpawnX: config.plantSpawnX,
            plantSpawnY: config.plantSpawnY,
            plantEnergyGain: Mathf.Max(0f, config.plantEnergyGain),
            plantRespawnSeconds: Mathf.Max(0f, config.plantRespawnSeconds),

            // plant population dynamics (Spread Ecology)
            plantMinCount: plantMin,
            plantMaxCount: plantMax,
            plantSpreadIntervalSeconds: Mathf.Max(0f, config.plantSpreadIntervalSeconds),
            plantSpreadChance: Mathf.Clamp01(config.plantSpreadChance),
            plantSpreadRadius: Mathf.Max(0f, config.plantSpreadRadius),
            plantRescueIntervalSeconds: Mathf.Max(0f, config.plantRescueIntervalSeconds),
            plantRescueBatchSize: rescueBatch,

            // plant regrowth dynamics (legacy knobs; logic will migrate in world)
            plantRegrowRadius: Mathf.Max(0f, config.plantRegrowRadius),
            plantLongJumpChance: Mathf.Clamp01(config.plantLongJumpChance),
            plantBoundsInset: Mathf.Max(0f, config.plantBoundsInset),

            // interaction
            eatRadius: Mathf.Max(0f, config.eatRadius),

            // counts
            initialZoopCount: Mathf.Max(1, config.initialZoopCount),
            initialPlantCount: initialPlantCount,

            // default genes
            geneDefaultMoveSpeed: Mathf.Max(0f, geneDefaultMoveSpeed),
            geneDefaultVisionRange: Mathf.Max(0f, config.geneDefaultVisionRange),
            geneDefaultReproThreshold: Mathf.Max(0f, config.geneDefaultReproThreshold),

            // founder variation
            founderGeneSigmaFraction: Mathf.Max(0f, config.founderGeneSigmaFraction),

            // clamps
            geneMinMoveSpeed: Mathf.Max(0f, config.geneMinMoveSpeed),
            geneMaxMoveSpeed: Mathf.Max(0f, config.geneMaxMoveSpeed),
            geneMinVisionRange: Mathf.Max(0f, config.geneMinVisionRange),
            geneMaxVisionRange: Mathf.Max(0f, config.geneMaxVisionRange),
            geneMinReproThreshold: Mathf.Max(0f, config.geneMinReproThreshold),
            geneMaxReproThreshold: Mathf.Max(0f, config.geneMaxReproThreshold),

            // reproduction
            reproCooldownSeconds: Mathf.Max(0f, config.reproCooldownSeconds),
            reproParentEnergyFraction: Mathf.Clamp01(config.reproParentEnergyFraction),

            // mutation
            mutationSigmaFraction: Mathf.Max(0f, config.mutationSigmaFraction)
        );

        _world = new SimulationWorld(
            seed,
            config.minX, config.maxX,
            config.minY, config.maxY,
            p
        );

        _world.OnEvent += OnSimEvent;

        _world.Rebuild();

        WorldChanged?.Invoke();
    }

    private void OnSimEvent(SimEvent e)
    {
        SimEventRaised?.Invoke(e);

        if (e.Type == SimEventType.WorldRebuilt)
            Debug.Log($"[Sim] World rebuilt (seed={_world.Seed})");
    }
}
