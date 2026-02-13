// File: Assets/Zoops/Scripts/Sim/PlantSim.cs
namespace Zoops.Simulation
{
    // Plant entity. Today it is edible; later other entities may be edible too.
    public sealed class PlantSim
    {
        public int EntityId { get; }
        public int BirthTick { get; private set; }

        public float X;
        public float Y;

        public bool IsAlive { get; set; } = true;
        public float RespawnTimer;

        // For patchy regrowth: where this plant was last eaten.
        public float LastEatenX;
        public float LastEatenY;

        public EntityKind Kind => EntityKind.Plant;

        public PlantSim(int entityId, int birthTick, float x, float y)
        {
            EntityId = entityId;
            BirthTick = birthTick;
            X = x;
            Y = y;

            LastEatenX = x;
            LastEatenY = y;
        }

        public void MarkEatenAt(float x, float y, float respawnSeconds)
        {
            LastEatenX = x;
            LastEatenY = y;
            IsAlive = false;
            RespawnTimer = respawnSeconds;
        }

        public void RespawnAt(int tick, float x, float y)
        {
            BirthTick = tick;
            X = x;
            Y = y;
            IsAlive = true;
            RespawnTimer = 0f;
        }
    }
}
