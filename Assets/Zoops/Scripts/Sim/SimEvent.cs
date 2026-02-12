namespace Zoops.Simulation
{
    public readonly struct SimEvent
    {
        public readonly SimEventType Type;
        public readonly int Tick;

        // -1 / Unknown when not applicable
        public readonly int EntityId;
        public readonly EntityKind Kind;

        public SimEvent(SimEventType type, int tick, int entityId = -1, EntityKind kind = default)
        {
            Type = type;
            Tick = tick;
            EntityId = entityId;
            Kind = kind;
        }
    }
}
