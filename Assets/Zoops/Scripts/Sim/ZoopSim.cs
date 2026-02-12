namespace Zoops.Simulation
{
    public sealed class ZoopSim
    {
        public int EntityId { get; }
        public int BirthTick { get; }

        public float X;
        public float Y;

        public bool IsAlive { get; private set; }

        public float Energy { get; internal set; }

        // Written by policy (brain lane), consumed by physics lane.
        public float IntentX;
        public float IntentY;

        private readonly float _speed;

        public ZoopSim(
            int entityId,
            int birthTick,
            float x,
            float y,
            float startingEnergy,
            float speed)
        {
            EntityId = entityId;
            BirthTick = birthTick;

            X = x;
            Y = y;

            IsAlive = true;
            Energy = startingEnergy;

            _speed = speed;

            // Default intent so it moves if policy is absent.
            IntentX = 1f;
            IntentY = 0f;
        }

        public void Kill()
        {
            IsAlive = false;
            IntentX = 0f;
            IntentY = 0f;
        }

        /// <summary>
        /// Physics-lane movement only. World owns interactions (eating).
        /// </summary>
        public void StepMove(float dt, SimulationWorld world)
        {
            if (!IsAlive) return;

            X += IntentX * _speed * dt;
            Y += IntentY * _speed * dt;

            world.WrapPositionXY(ref X, ref Y);
        }
    }
}
