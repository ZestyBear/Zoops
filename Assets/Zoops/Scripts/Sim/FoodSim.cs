namespace Zoops.Simulation
{
    /// <summary>
    /// Sim-side food entity (no Unity types).
    /// </summary>
    public sealed class FoodSim
    {
        public int EntityId { get; }
        public int BirthTick { get; private set; }

        public float X;
        public float Y;

        public bool IsAlive { get; internal set; }

        /// <summary>
        /// When IsAlive is false, counts down to respawn (seconds).
        /// World decrements this in deterministic physics ticks.
        /// </summary>
        public float RespawnTimer { get; internal set; }

        public FoodSim(int entityId, int birthTick, float x, float y)
        {
            EntityId = entityId;
            BirthTick = birthTick;

            X = x;
            Y = y;

            IsAlive = true;
            RespawnTimer = 0f;
        }

        internal void RespawnAt(int tick, float x, float y)
        {
            BirthTick = tick;
            X = x;
            Y = y;

            IsAlive = true;
            RespawnTimer = 0f;
        }
    }
}
