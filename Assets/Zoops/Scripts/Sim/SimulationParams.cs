namespace Zoops.Simulation
{
    public readonly struct SimulationParams
    {
        public readonly float ZoopStartingEnergy;
        public readonly float ZoopMaxEnergy;
        public readonly float ZoopMetabolismPerSecond;

        public readonly float FoodSpawnX;
        public readonly float FoodSpawnY;
        public readonly float FoodEnergyGain;
        public readonly float FoodRespawnSeconds;

        public readonly float ZoopSpeed;
        public readonly float EatRadius;

        public SimulationParams(
            float zoopStartingEnergy,
            float zoopMaxEnergy,
            float zoopMetabolismPerSecond,
            float foodSpawnX,
            float foodSpawnY,
            float foodEnergyGain,
            float foodRespawnSeconds,
            float zoopSpeed,
            float eatRadius)
        {
            ZoopStartingEnergy = zoopStartingEnergy;
            ZoopMaxEnergy = zoopMaxEnergy;
            ZoopMetabolismPerSecond = zoopMetabolismPerSecond;

            FoodSpawnX = foodSpawnX;
            FoodSpawnY = foodSpawnY;
            FoodEnergyGain = foodEnergyGain;
            FoodRespawnSeconds = foodRespawnSeconds;

            ZoopSpeed = zoopSpeed;
            EatRadius = eatRadius;
        }
    }
}
