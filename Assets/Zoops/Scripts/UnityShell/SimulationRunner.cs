// File: SimulationRunner.cs
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

    /// <summary>
    /// Deterministic sim tick duration (seconds per physics tick).
    /// UI uses this to convert tick counts to seconds (e.g., age).
    /// </summary>
    public float SimTickSeconds => physicsHz > 0f ? (1f / physicsHz) : 0f;

    /// <summary>
    /// View-layer lifecycle signal: fired whenever a new world is built/rebuilt.
    /// </summary>
    public event Action WorldChanged;

    /// <summary>
    /// Unity-facing forwarding of sim events (single gateway for observation layers).
    /// </summary>
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

        // Run brain lane first so physics consumes freshest intent.
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
        // Reset accumulators so the first frame after rebuild is deterministic and clean.
        _physicsAccum = 0f;
        _brainAccum = 0f;

        if (config == null)
        {
            Debug.LogError("[Sim] SimulationRunner has no SimulationConfig assigned. Create one (Assets > Create > Zoops > Simulation Config) and assign it.");
            return;
        }

        // Normalize config values (keep config authoritative, but ensure sane invariants).
        float startE = Mathf.Max(0f, config.zoopStartingEnergy);
        float maxE = Mathf.Max(0f, config.zoopMaxEnergy);

        if (maxE <= 0f) maxE = startE;        // if user left max at 0, default to starting energy
        if (maxE < startE) maxE = startE;     // ensure max >= start

        var p = new SimulationParams(
            zoopStartingEnergy: startE,
            zoopMaxEnergy: maxE,
            zoopMetabolismPerSecond: Mathf.Max(0f, config.zoopMetabolismPerSecond),
            foodSpawnX: config.foodSpawnX,
            foodSpawnY: config.foodSpawnY,
            foodEnergyGain: Mathf.Max(0f, config.foodEnergyGain),
            foodRespawnSeconds: Mathf.Max(0f, config.foodRespawnSeconds),
            zoopSpeed: Mathf.Max(0f, config.zoopSpeed),
            eatRadius: Mathf.Max(0f, config.eatRadius)
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
