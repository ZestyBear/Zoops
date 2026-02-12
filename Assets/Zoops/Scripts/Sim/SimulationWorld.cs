using System;
using System.Collections.Generic;

namespace Zoops.Simulation
{
    // Authoritative world state (NO Unity types here)
    public sealed class SimulationWorld
    {
        public int Seed { get; }
        public int Tick { get; private set; }

        public readonly float MinX, MaxX, MinY, MaxY;

        // ---- Containers (authoritative) ----
        public List<ZoopSim> Zoops { get; } = new List<ZoopSim>(capacity: 16);
        public List<FoodSim> Foods { get; } = new List<FoodSim>(capacity: 64);

        // Fast lookup by EntityId (rebuilt deterministically when needed).
        private readonly Dictionary<int, ZoopSim> _zoopById = new Dictionary<int, ZoopSim>(capacity: 32);
        private readonly Dictionary<int, FoodSim> _foodById = new Dictionary<int, FoodSim>(capacity: 128);

        // Decision seam (swappable brain)
        public DecisionPolicy Policy { get; set; }

        // Energy rules / knobs
        private readonly SimulationParams _p;

        /// <summary>Authoritative max energy for a Zoop (used by UI for energy bar scaling).</summary>
        public float ZoopMaxEnergy => _p.ZoopMaxEnergy;

        // IDs
        private int _nextEntityId = 1;

        // Existing sim event pipeline
        public event Action<SimEvent> OnEvent;

        public SimulationWorld(int seed, float minX, float maxX, float minY, float maxY, SimulationParams p)
        {
            Seed = seed;
            MinX = minX; MaxX = maxX;
            MinY = minY; MaxY = maxY;

            _p = p;

            Policy = new HardcodedPolicy();
        }

        public bool TryGetZoop(int entityId, out ZoopSim zoop) => _zoopById.TryGetValue(entityId, out zoop);
        public bool TryGetFood(int entityId, out FoodSim food) => _foodById.TryGetValue(entityId, out food);

        public void Rebuild()
        {
            Tick = 0;

            Zoops.Clear();
            Foods.Clear();
            _zoopById.Clear();
            _foodById.Clear();

            _nextEntityId = 1;

            // MVP numbers for now (we'll drive these from config later)
            const int zoopCount = 1;
            const int foodCount = 1;

            // Spawn zoops
            for (int i = 0; i < zoopCount; i++)
            {
                int id = NextId();
                var z = new ZoopSim(
                    entityId: id,
                    birthTick: Tick,
                    x: 0f,
                    y: 0f,
                    startingEnergy: _p.ZoopStartingEnergy,
                    speed: _p.ZoopSpeed
                );

                Zoops.Add(z);
                _zoopById[id] = z;

                Emit(new SimEvent(SimEventType.EntityBorn, Tick, id, EntityKind.Zoop));
            }

            // Spawn foods
            for (int i = 0; i < foodCount; i++)
            {
                int id = NextId();
                var f = new FoodSim(
                    entityId: id,
                    birthTick: Tick,
                    x: _p.FoodSpawnX,
                    y: _p.FoodSpawnY
                );

                Foods.Add(f);
                _foodById[id] = f;

                Emit(new SimEvent(SimEventType.EntityBorn, Tick, id, EntityKind.Food));
            }

            // Let brain set initial intent immediately
            StepBrainTick(0f);

            Emit(new SimEvent(SimEventType.WorldRebuilt, Tick));
        }

        private int NextId() => _nextEntityId++;

        /// <summary>
        /// Brain lane: updates intent only (for all zoops).
        /// </summary>
        public void StepBrainTick(float brainDt)
        {
            if (Policy == null) return;

            for (int i = 0; i < Zoops.Count; i++)
            {
                var z = Zoops[i];
                if (z == null || !z.IsAlive) continue;

                Policy.StepBrain(this, z, brainDt);
            }
        }

        /// <summary>
        /// World-owned: apply food death + energy gain + respawn scheduling.
        /// </summary>
        private void ConsumeFood(ZoopSim eater, FoodSim food)
        {
            if (eater == null || !eater.IsAlive) return;
            if (food == null || !food.IsAlive) return;

            food.IsAlive = false;
            food.RespawnTimer = _p.FoodRespawnSeconds;

            eater.Energy += _p.FoodEnergyGain;
            if (_p.ZoopMaxEnergy > 0f && eater.Energy > _p.ZoopMaxEnergy)
                eater.Energy = _p.ZoopMaxEnergy;

            // Treat as entity died (food “removed from world”)
            Emit(new SimEvent(SimEventType.EntityDied, Tick, food.EntityId, EntityKind.Food));
        }

        /// <summary>
        /// Physics lane: deterministic ordering:
        /// 1) Tick++
        /// 2) Metabolism drain
        /// 3) Death check (emit EntityDied once)
        /// 4) Movement
        /// 5) Interactions (eating)
        /// 6) Timers (food respawn)
        /// 7) TickAdvanced event
        /// </summary>
        public void StepOneTick(float dt)
        {
            Tick++;

            // --- Metabolism + zoop death ---
            for (int i = 0; i < Zoops.Count; i++)
            {
                var z = Zoops[i];
                if (z == null || !z.IsAlive) continue;

                z.Energy -= _p.ZoopMetabolismPerSecond * dt;

                if (z.Energy <= 0f)
                {
                    z.Energy = 0f;
                    z.Kill();
                    Emit(new SimEvent(SimEventType.EntityDied, Tick, z.EntityId, EntityKind.Zoop));
                }
            }

            // --- Movement ---
            for (int i = 0; i < Zoops.Count; i++)
            {
                var z = Zoops[i];
                if (z == null || !z.IsAlive) continue;

                z.StepMove(dt, this);
            }

            // --- Eating (zoops processed in stable order; food consumed immediately) ---
            float eatR2 = _p.EatRadius * _p.EatRadius;

            for (int zi = 0; zi < Zoops.Count; zi++)
            {
                var z = Zoops[zi];
                if (z == null || !z.IsAlive) continue;

                for (int fi = 0; fi < Foods.Count; fi++)
                {
                    var f = Foods[fi];
                    if (f == null || !f.IsAlive) continue;

                    float dx = ShortestWrappedDeltaX(z.X, f.X);
                    float dy = ShortestWrappedDeltaY(z.Y, f.Y);
                    float d2 = dx * dx + dy * dy;

                    if (d2 <= eatR2)
                    {
                        ConsumeFood(z, f);
                        break; // one eat per zoop per tick (deterministic)
                    }
                }
            }

            // --- Food respawn (per food) ---
            for (int i = 0; i < Foods.Count; i++)
            {
                var f = Foods[i];
                if (f == null) continue;

                if (!f.IsAlive)
                {
                    f.RespawnTimer -= dt;

                    if (f.RespawnTimer <= 0f)
                    {
                        f.RespawnAt(Tick, _p.FoodSpawnX, _p.FoodSpawnY);

                        // Treat as entity born again (same id)
                        Emit(new SimEvent(SimEventType.EntityBorn, Tick, f.EntityId, EntityKind.Food));
                    }
                }
            }

            Emit(new SimEvent(SimEventType.TickAdvanced, Tick));
        }

        /// <summary>
        /// WrapXY (toroidal) world topology.
        /// </summary>
        public void WrapPositionXY(ref float x, ref float y)
        {
            float width = MaxX - MinX;
            float height = MaxY - MinY;

            if (width <= 0f || height <= 0f) return;

            x = Wrap1D(x, MinX, MaxX, width);
            y = Wrap1D(y, MinY, MaxY, height);
        }

        public float ShortestWrappedDeltaX(float ax, float bx)
        {
            float width = MaxX - MinX;
            if (width <= 0f) return bx - ax;

            float dx = bx - ax;
            dx = dx % width;

            if (dx > width * 0.5f) dx -= width;
            else if (dx < -width * 0.5f) dx += width;

            return dx;
        }

        public float ShortestWrappedDeltaY(float ay, float by)
        {
            float height = MaxY - MinY;
            if (height <= 0f) return by - ay;

            float dy = by - ay;
            dy = dy % height;

            if (dy > height * 0.5f) dy -= height;
            else if (dy < -height * 0.5f) dy += height;

            return dy;
        }

        private static float Wrap1D(float v, float min, float max, float span)
        {
            if (v >= min && v < max) return v;

            float t = (v - min) % span;
            if (t < 0f) t += span;
            return min + t;
        }

        private void Emit(in SimEvent e)
        {
            OnEvent?.Invoke(e);
        }
    }
}
