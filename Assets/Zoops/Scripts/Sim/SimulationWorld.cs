// File: Assets/Zoops/Scripts/Sim/SimulationWorld.cs
using System;
using System.Collections.Generic;

namespace Zoops.Simulation
{
    public sealed class SimulationWorld
    {
        public int Seed { get; }
        public int Tick { get; private set; }

        public readonly float MinX, MaxX, MinY, MaxY;

        public List<ZoopSim> Zoops { get; } = new List<ZoopSim>(16);
        public List<PlantSim> Plants { get; } = new List<PlantSim>(64);

        private readonly Dictionary<int, ZoopSim> _zoopById = new Dictionary<int, ZoopSim>(32);
        private readonly Dictionary<int, PlantSim> _plantById = new Dictionary<int, PlantSim>(128);

        public DecisionPolicy Policy { get; set; }

        private readonly SimulationParams _p;

        // Back-compat for UI/debug
        public float ZoopMaxEnergy => _p.ZoopMaxEnergy;

        private int _nextEntityId = 1;
        private Random _rng;

        // Spread Ecology timers (sim-time, deterministic)
        private float _plantSpreadTimer;
        private float _plantRescueTimer;

        public event Action<SimEvent> OnEvent;

        public SimulationWorld(int seed, float minX, float maxX, float minY, float maxY, SimulationParams p)
        {
            Seed = seed;
            MinX = minX; MaxX = maxX;
            MinY = minY; MaxY = maxY;

            _p = p;

            Policy = new HardcodedPolicy();
            _rng = new Random(Seed);
        }

        public bool TryGetZoop(int entityId, out ZoopSim zoop) => _zoopById.TryGetValue(entityId, out zoop);
        public bool TryGetPlant(int entityId, out PlantSim plant) => _plantById.TryGetValue(entityId, out plant);

        public void Rebuild()
        {
            Tick = 0;

            Zoops.Clear();
            Plants.Clear();
            _zoopById.Clear();
            _plantById.Clear();

            _nextEntityId = 1;
            _rng = new Random(Seed);

            _plantSpreadTimer = 0f;
            _plantRescueTimer = 0f;

            int zoopCount = _p.InitialZoopCount <= 0 ? 1 : _p.InitialZoopCount;
            int plantCount = _p.InitialPlantCount <= 0 ? 1 : _p.InitialPlantCount;

            // Ensure initial plants respect max cap (deterministic clamp)
            int plantMax = _p.PlantMaxCount > 0 ? _p.PlantMaxCount : int.MaxValue;
            if (plantCount > plantMax) plantCount = plantMax;

            // Spawn Zoops (founders)
            for (int i = 0; i < zoopCount; i++)
            {
                int id = NextId();

                float x = Lerp(MinX, MaxX, Next01());
                float y = Lerp(MinY, MaxY, Next01());

                var genes = new ZoopGenes(
                    _p.GeneDefaultMoveSpeed,
                    _p.GeneDefaultVisionRange,
                    _p.GeneDefaultReproThreshold
                );

                // Founder-only initial variation
                if (_p.FounderGeneSigmaFraction > 0f)
                    genes = MutateGenes(genes, _p.FounderGeneSigmaFraction);

                var z = new ZoopSim(
                    entityId: id,
                    birthTick: Tick,
                    parentId: -1,
                    x: x,
                    y: y,
                    startingEnergy: _p.ZoopStartingEnergy,
                    genes: genes
                );

                Zoops.Add(z);
                _zoopById[id] = z;

                Emit(new SimEvent(SimEventType.EntityBorn, Tick, id, EntityKind.Zoop));
            }

            // Spawn Plants
            for (int i = 0; i < plantCount; i++)
            {
                SpawnPlantDeterministic(
                    // first plant uses hint spawn
                    useHint: (i == 0),
                    hintX: _p.PlantSpawnX,
                    hintY: _p.PlantSpawnY,
                    aroundX: 0f,
                    aroundY: 0f,
                    radius: 0f
                );
            }

            Emit(new SimEvent(SimEventType.WorldRebuilt, Tick));
        }

        private int NextId() => _nextEntityId++;

        // ===========================
        // Brain Lane
        // ===========================

        public void StepBrainTick(float brainDt)
        {
            if (Policy == null) return;

            for (int i = 0; i < Zoops.Count; i++)
            {
                var z = Zoops[i];
                if (z == null || !z.IsAlive) continue;

                Observation obs = BuildObservation(z);
                Intent intent = Policy.StepBrain(in obs, brainDt);

                // If intent is non-zero, update. If zero, preserve prior intent (MVP behavior).
                if (intent.MoveX != 0f || intent.MoveY != 0f)
                {
                    z.IntentX = intent.MoveX;
                    z.IntentY = intent.MoveY;
                }
            }
        }

        private Observation BuildObservation(ZoopSim zoop)
        {
            bool hasPlant = false;
            float bestD2 = float.PositiveInfinity;
            float bestDx = 0f;
            float bestDy = 0f;

            float vision = zoop.Genes.VisionRange;
            float vision2 = vision > 0f ? vision * vision : 0f;

            for (int i = 0; i < Plants.Count; i++)
            {
                var p = Plants[i];
                if (p == null || !p.IsAlive) continue;

                float dx = ShortestWrappedDeltaX(zoop.X, p.X);
                float dy = ShortestWrappedDeltaY(zoop.Y, p.Y);
                float d2 = dx * dx + dy * dy;

                if (vision2 > 0f && d2 > vision2) continue;

                if (d2 < bestD2)
                {
                    bestD2 = d2;
                    bestDx = dx;
                    bestDy = dy;
                    hasPlant = true;
                }
            }

            float dist = hasPlant ? (float)Math.Sqrt(bestD2) : 0f;

            float energy01 = (_p.ZoopMaxEnergy > 0f)
                ? Clamp01(zoop.Energy / _p.ZoopMaxEnergy)
                : 0f;

            return new Observation(
                energy01,
                zoop.X,
                zoop.Y,
                hasPlant,
                bestDx,
                bestDy,
                dist,
                0f,
                0f,
                Normalize01(zoop.Genes.MoveSpeed, _p.GeneMinMoveSpeed, _p.GeneMaxMoveSpeed),
                Normalize01(zoop.Genes.VisionRange, _p.GeneMinVisionRange, _p.GeneMaxVisionRange),
                Normalize01(zoop.Genes.ReproThreshold, _p.GeneMinReproThreshold, _p.GeneMaxReproThreshold)
            );
        }

        // ===========================
        // Physics Lane
        // ===========================

        public void StepOneTick(float dt)
        {
            Tick++;

            // Metabolism + cooldown + death
            for (int i = 0; i < Zoops.Count; i++)
            {
                var z = Zoops[i];
                if (!z.IsAlive) continue;

                if (z.ReproCooldown > 0f)
                {
                    z.ReproCooldown -= dt;
                    if (z.ReproCooldown < 0f) z.ReproCooldown = 0f;
                }

                z.Energy -= _p.ZoopMetabolismPerSecond * dt;

                if (z.Energy <= 0f)
                {
                    z.Energy = 0f;
                    z.Kill();
                    Emit(new SimEvent(SimEventType.EntityDied, Tick, z.EntityId, EntityKind.Zoop));
                }
            }

            // Movement (intent-driven)
            for (int i = 0; i < Zoops.Count; i++)
            {
                var z = Zoops[i];
                if (!z.IsAlive) continue;
                z.StepMove(dt, this);
            }

            // Eating (consume plants) — plants are removed from world
            float eatR2 = _p.EatRadius * _p.EatRadius;

            for (int zi = 0; zi < Zoops.Count; zi++)
            {
                var z = Zoops[zi];
                if (!z.IsAlive) continue;

                for (int pi = 0; pi < Plants.Count; pi++)
                {
                    var p = Plants[pi];
                    if (p == null || !p.IsAlive) continue;

                    float dx = ShortestWrappedDeltaX(z.X, p.X);
                    float dy = ShortestWrappedDeltaY(z.Y, p.Y);
                    float d2 = dx * dx + dy * dy;

                    if (d2 <= eatR2)
                    {
                        ConsumePlantAndRemove(z, p, pi);
                        break;
                    }
                }
            }

            // Reproduction (append children after scanning)
            if (_p.ReproParentEnergyFraction > 0f && _p.ReproParentEnergyFraction < 1f)
            {
                List<ZoopSim> newborns = null;

                for (int i = 0; i < Zoops.Count; i++)
                {
                    var parent = Zoops[i];
                    if (!parent.IsAlive) continue;

                    if (parent.ReproCooldown > 0f) continue;
                    if (parent.Energy < parent.Genes.ReproThreshold) continue;

                    int id = NextId();

                    float childEnergy = parent.Energy * (1f - _p.ReproParentEnergyFraction);
                    parent.Energy *= _p.ReproParentEnergyFraction;

                    var childGenes = MutateGenes(parent.Genes, _p.MutationSigmaFraction);

                    var child = new ZoopSim(
                        entityId: id,
                        birthTick: Tick,
                        parentId: parent.EntityId,
                        x: parent.X,
                        y: parent.Y,
                        startingEnergy: childEnergy,
                        genes: childGenes
                    );

                    // inherit current intent as a reasonable default
                    child.IntentX = parent.IntentX;
                    child.IntentY = parent.IntentY;

                    parent.ReproCooldown = _p.ReproCooldownSeconds;

                    newborns ??= new List<ZoopSim>(4);
                    newborns.Add(child);
                }

                if (newborns != null)
                {
                    for (int i = 0; i < newborns.Count; i++)
                    {
                        var z = newborns[i];
                        Zoops.Add(z);
                        _zoopById[z.EntityId] = z;
                        Emit(new SimEvent(SimEventType.EntityBorn, Tick, z.EntityId, EntityKind.Zoop));
                    }
                }
            }

            // Spread Ecology (Spread + Rescue), deterministic
            StepPlantPopulation(dt);

            Emit(new SimEvent(SimEventType.TickAdvanced, Tick));
        }

        private void StepPlantPopulation(float dt)
        {
            int plantMax = _p.PlantMaxCount > 0 ? _p.PlantMaxCount : int.MaxValue;

            // --- Spread ---
            if (_p.PlantSpreadIntervalSeconds > 0f && _p.PlantSpreadChance > 0f && _p.PlantSpreadRadius > 0f)
            {
                _plantSpreadTimer += dt;

                while (_plantSpreadTimer >= _p.PlantSpreadIntervalSeconds)
                {
                    _plantSpreadTimer -= _p.PlantSpreadIntervalSeconds;

                    // deterministic stable pass over current plants
                    int currentCount = Plants.Count;
                    if (currentCount <= 0) break;

                    int capacityLeft = plantMax - currentCount;
                    if (capacityLeft <= 0) break;

                    List<(float x, float y)> queued = null;

                    for (int i = 0; i < currentCount; i++)
                    {
                        if (capacityLeft <= 0) break;

                        var source = Plants[i];
                        if (source == null || !source.IsAlive) continue;

                        // Use world RNG only inside deterministic loop.
                        if (Next01() > _p.PlantSpreadChance) continue;

                        float sx = source.X;
                        float sy = source.Y;

                        float x, y;
                        PickRandomPointInRadius(sx, sy, _p.PlantSpreadRadius, out x, out y);
                        WrapPositionXY(ref x, ref y);
                        ClampToInset(ref x, ref y);

                        queued ??= new List<(float x, float y)>(8);
                        queued.Add((x, y));
                        capacityLeft--;
                    }

                    if (queued != null)
                    {
                        for (int i = 0; i < queued.Count; i++)
                        {
                            if (Plants.Count >= plantMax) break;

                            var pos = queued[i];
                            SpawnPlantAt(pos.x, pos.y);
                        }
                    }
                }
            }

            // --- Rescue ---
            if (_p.PlantRescueIntervalSeconds > 0f && _p.PlantRescueBatchSize > 0 && _p.PlantMinCount >= 0)
            {
                _plantRescueTimer += dt;

                while (_plantRescueTimer >= _p.PlantRescueIntervalSeconds)
                {
                    _plantRescueTimer -= _p.PlantRescueIntervalSeconds;

                    int count = Plants.Count;
                    if (count >= _p.PlantMinCount) break;

                    int needed = _p.PlantMinCount - count;
                    int add = _p.PlantRescueBatchSize;
                    if (add > needed) add = needed;

                    int room = plantMax - count;
                    if (room <= 0) break;
                    if (add > room) add = room;

                    for (int i = 0; i < add; i++)
                    {
                        float x = Lerp(MinX, MaxX, Next01());
                        float y = Lerp(MinY, MaxY, Next01());
                        ClampToInset(ref x, ref y);
                        SpawnPlantAt(x, y);
                    }

                    // One rescue batch per check tick is enough; if still low, next interval will add again.
                    break;
                }
            }
        }

        private void ConsumePlantAndRemove(ZoopSim eater, PlantSim plant, int plantIndex)
        {
            // Energy gain
            eater.Energy += _p.PlantEnergyGain;
            if (_p.ZoopMaxEnergy > 0f && eater.Energy > _p.ZoopMaxEnergy)
                eater.Energy = _p.ZoopMaxEnergy;

            // Emit death before removal (view can destroy)
            Emit(new SimEvent(SimEventType.EntityDied, Tick, plant.EntityId, EntityKind.Plant));

            // Remove from lookups + stable list
            _plantById.Remove(plant.EntityId);

            int last = Plants.Count - 1;
            if (plantIndex < 0 || plantIndex > last) return;

            if (plantIndex != last)
            {
                Plants[plantIndex] = Plants[last];
            }
            Plants.RemoveAt(last);
        }

        private void SpawnPlantDeterministic(bool useHint, float hintX, float hintY, float aroundX, float aroundY, float radius)
        {
            int plantMax = _p.PlantMaxCount > 0 ? _p.PlantMaxCount : int.MaxValue;
            if (Plants.Count >= plantMax) return;

            float x, y;

            if (useHint)
            {
                x = hintX;
                y = hintY;
                ClampToInset(ref x, ref y);
                SpawnPlantAt(x, y);
                return;
            }

            if (radius > 0f)
            {
                PickRandomPointInRadius(aroundX, aroundY, radius, out x, out y);
                WrapPositionXY(ref x, ref y);
                ClampToInset(ref x, ref y);
                SpawnPlantAt(x, y);
                return;
            }

            x = Lerp(MinX, MaxX, Next01());
            y = Lerp(MinY, MaxY, Next01());
            ClampToInset(ref x, ref y);
            SpawnPlantAt(x, y);
        }

        private void SpawnPlantAt(float x, float y)
        {
            int plantMax = _p.PlantMaxCount > 0 ? _p.PlantMaxCount : int.MaxValue;
            if (Plants.Count >= plantMax) return;

            int id = NextId();
            var plant = new PlantSim(id, Tick, x, y);

            Plants.Add(plant);
            _plantById[id] = plant;

            Emit(new SimEvent(SimEventType.EntityBorn, Tick, id, EntityKind.Plant));
        }

        private void PickRandomPointInRadius(float cx, float cy, float radius, out float x, out float y)
        {
            // Uniform in disk
            float t = (float)(Next01() * (Math.PI * 2.0));
            float u = Next01();
            float rr = radius * (float)Math.Sqrt(u);

            x = cx + rr * (float)Math.Cos(t);
            y = cy + rr * (float)Math.Sin(t);
        }

        private ZoopGenes MutateGenes(ZoopGenes g, float sigmaFraction)
        {
            if (sigmaFraction <= 0f) return g;

            float ms = MutateScalar(g.MoveSpeed, sigmaFraction);
            float vr = MutateScalar(g.VisionRange, sigmaFraction);
            float rt = MutateScalar(g.ReproThreshold, sigmaFraction);

            ms = Clamp(ms, _p.GeneMinMoveSpeed, _p.GeneMaxMoveSpeed);
            vr = Clamp(vr, _p.GeneMinVisionRange, _p.GeneMaxVisionRange);
            rt = Clamp(rt, _p.GeneMinReproThreshold, _p.GeneMaxReproThreshold);

            return new ZoopGenes(ms, vr, rt);
        }

        private float MutateScalar(float v, float sigmaFraction)
        {
            float delta = (Next01() * 2f - 1f) * sigmaFraction;
            return v * (1f + delta);
        }

        // ===========================
        // Utility
        // ===========================

        public void WrapPositionXY(ref float x, ref float y)
        {
            float width = MaxX - MinX;
            float height = MaxY - MinY;
            x = Wrap1D(x, MinX, MaxX, width);
            y = Wrap1D(y, MinY, MaxY, height);
        }

        public float ShortestWrappedDeltaX(float ax, float bx)
        {
            float width = MaxX - MinX;
            float dx = bx - ax;
            dx = dx % width;
            if (dx > width * 0.5f) dx -= width;
            else if (dx < -width * 0.5f) dx += width;
            return dx;
        }

        public float ShortestWrappedDeltaY(float ay, float by)
        {
            float height = MaxY - MinY;
            float dy = by - ay;
            dy = dy % height;
            if (dy > height * 0.5f) dy -= height;
            else if (dy < -height * 0.5f) dy += height;
            return dy;
        }

        private static float Wrap1D(float v, float min, float max, float span)
        {
            if (span <= 0f) return v;
            float t = (v - min) % span;
            if (t < 0f) t += span;
            return min + t;
        }

        private void ClampToInset(ref float x, ref float y)
        {
            float inset = _p.PlantBoundsInset;
            float minX = MinX + inset;
            float maxX = MaxX - inset;
            float minY = MinY + inset;
            float maxY = MaxY - inset;

            if (x < minX) x = minX;
            else if (x > maxX) x = maxX;

            if (y < minY) y = minY;
            else if (y > maxY) y = maxY;
        }

        private void Emit(in SimEvent e) => OnEvent?.Invoke(e);

        private float Next01() => (float)_rng.NextDouble();
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        private static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);

        private static float Normalize01(float v, float min, float max)
        {
            float denom = (max - min);
            if (denom <= 0f) return 0f;
            return Clamp01((v - min) / denom);
        }
    }
}
