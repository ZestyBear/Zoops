// File: Assets/Zoops/Scripts/Sim/SimulationParams.cs
namespace Zoops.Simulation
{
    public readonly struct SimulationParams
    {
        // --- Zoop energy economy ---
        public readonly float ZoopStartingEnergy;
        public readonly float ZoopMaxEnergy;
        public readonly float ZoopMetabolismPerSecond;

        // --- Plant (edible entity today) ---
        public readonly float PlantEnergyGain;
        public readonly float PlantRespawnSeconds;

        // --- Plant population dynamics (Spread Ecology) ---
        // Plants are entities. Count varies between min/max.
        // "Spread" = local propagation attempts by living plants.
        public readonly int PlantMinCount;
        public readonly int PlantMaxCount;

        public readonly float PlantSpreadIntervalSeconds;
        public readonly float PlantSpreadChance;
        public readonly float PlantSpreadRadius;

        public readonly float PlantRescueIntervalSeconds;
        public readonly int PlantRescueBatchSize;

        // Initial placement hint (only used for first plant at rebuild)
        public readonly float PlantSpawnX;
        public readonly float PlantSpawnY;

        // Patchy regrowth controls
        public readonly float PlantRegrowRadius;
        public readonly float PlantLongJumpChance;
        public readonly float PlantBoundsInset;

        // --- Interaction ---
        public readonly float EatRadius;

        // --- Counts ---
        public readonly int InitialZoopCount;
        public readonly int InitialPlantCount;

        // --- Default genes (for initial spawns) ---
        public readonly float GeneDefaultMoveSpeed;
        public readonly float GeneDefaultVisionRange;
        public readonly float GeneDefaultReproThreshold;

        // Founder-only initial variation (one-time jitter at Rebuild)
        public readonly float FounderGeneSigmaFraction;

        // --- Gene clamps (safety + determinism) ---
        public readonly float GeneMinMoveSpeed;
        public readonly float GeneMaxMoveSpeed;
        public readonly float GeneMinVisionRange;
        public readonly float GeneMaxVisionRange;
        public readonly float GeneMinReproThreshold;
        public readonly float GeneMaxReproThreshold;

        // --- Reproduction rules ---
        public readonly float ReproCooldownSeconds;
        public readonly float ReproParentEnergyFraction; // e.g. 0.65 => parent keeps 65%, child gets 35%

        // --- Mutation ---
        public readonly float MutationSigmaFraction; // e.g. 0.05 => ±5% typical scale

        public SimulationParams(
            // energy economy
            float zoopStartingEnergy,
            float zoopMaxEnergy,
            float zoopMetabolismPerSecond,

            // plant
            float plantSpawnX,
            float plantSpawnY,
            float plantEnergyGain,
            float plantRespawnSeconds,

            // plant population dynamics (Spread Ecology)
            int plantMinCount,
            int plantMaxCount,
            float plantSpreadIntervalSeconds,
            float plantSpreadChance,
            float plantSpreadRadius,
            float plantRescueIntervalSeconds,
            int plantRescueBatchSize,

            // plant regrowth dynamics
            float plantRegrowRadius,
            float plantLongJumpChance,
            float plantBoundsInset,

            // interaction
            float eatRadius,

            // counts
            int initialZoopCount,
            int initialPlantCount,

            // default genes
            float geneDefaultMoveSpeed,
            float geneDefaultVisionRange,
            float geneDefaultReproThreshold,

            // founder variation
            float founderGeneSigmaFraction,

            // gene clamps
            float geneMinMoveSpeed,
            float geneMaxMoveSpeed,
            float geneMinVisionRange,
            float geneMaxVisionRange,
            float geneMinReproThreshold,
            float geneMaxReproThreshold,

            // reproduction
            float reproCooldownSeconds,
            float reproParentEnergyFraction,

            // mutation
            float mutationSigmaFraction)
        {
            ZoopStartingEnergy = zoopStartingEnergy;
            ZoopMaxEnergy = zoopMaxEnergy;
            ZoopMetabolismPerSecond = zoopMetabolismPerSecond;

            PlantSpawnX = plantSpawnX;
            PlantSpawnY = plantSpawnY;
            PlantEnergyGain = plantEnergyGain;
            PlantRespawnSeconds = plantRespawnSeconds;

            PlantMinCount = plantMinCount;
            PlantMaxCount = plantMaxCount;
            PlantSpreadIntervalSeconds = plantSpreadIntervalSeconds;
            PlantSpreadChance = plantSpreadChance;
            PlantSpreadRadius = plantSpreadRadius;
            PlantRescueIntervalSeconds = plantRescueIntervalSeconds;
            PlantRescueBatchSize = plantRescueBatchSize;

            PlantRegrowRadius = plantRegrowRadius;
            PlantLongJumpChance = plantLongJumpChance;
            PlantBoundsInset = plantBoundsInset;

            EatRadius = eatRadius;

            InitialZoopCount = initialZoopCount;
            InitialPlantCount = initialPlantCount;

            GeneDefaultMoveSpeed = geneDefaultMoveSpeed;
            GeneDefaultVisionRange = geneDefaultVisionRange;
            GeneDefaultReproThreshold = geneDefaultReproThreshold;

            FounderGeneSigmaFraction = founderGeneSigmaFraction;

            GeneMinMoveSpeed = geneMinMoveSpeed;
            GeneMaxMoveSpeed = geneMaxMoveSpeed;
            GeneMinVisionRange = geneMinVisionRange;
            GeneMaxVisionRange = geneMaxVisionRange;
            GeneMinReproThreshold = geneMinReproThreshold;
            GeneMaxReproThreshold = geneMaxReproThreshold;

            ReproCooldownSeconds = reproCooldownSeconds;
            ReproParentEnergyFraction = reproParentEnergyFraction;

            MutationSigmaFraction = mutationSigmaFraction;
        }
    }
}
