// File: Assets/Zoops/Scripts/Sim/ZoopSim.cs
namespace Zoops.Simulation
{
    public sealed class ZoopSim
    {
        public int EntityId { get; }
        public int BirthTick { get; }

        /// <summary>
        /// Minimal lineage tracking (for future emergent species analysis).
        /// -1 means no parent (founder).
        /// </summary>
        public int ParentId { get; }

        public float X;
        public float Y;

        public bool IsAlive { get; private set; }

        public float Energy { get; internal set; }

        /// <summary>
        /// Heritable configuration (copied + mutated on reproduction).
        /// </summary>
        public ZoopGenes Genes;

        // Written by policy (brain lane), consumed by physics lane.
        public float IntentX;
        public float IntentY;

        /// <summary>
        /// Reproduction cooldown (world decrements in physics lane).
        /// </summary>
        public float ReproCooldown;

        public ZoopSim(
            int entityId,
            int birthTick,
            int parentId,
            float x,
            float y,
            float startingEnergy,
            ZoopGenes genes)
        {
            EntityId = entityId;
            BirthTick = birthTick;
            ParentId = parentId;

            X = x;
            Y = y;

            IsAlive = true;
            Energy = startingEnergy;

            Genes = genes;

            // Default intent so it moves if policy returns none.
            IntentX = 1f;
            IntentY = 0f;

            ReproCooldown = 0f;
        }

        public void Kill()
        {
            IsAlive = false;
            IntentX = 0f;
            IntentY = 0f;
        }

        /// <summary>
        /// Physics-lane movement only. World owns interactions (eating, repro, etc).
        /// </summary>
        public void StepMove(float dt, SimulationWorld world)
        {
            if (!IsAlive) return;

            float speed = Genes.MoveSpeed;

            X += IntentX * speed * dt;
            Y += IntentY * speed * dt;

            world.WrapPositionXY(ref X, ref Y);
        }
    }
}
