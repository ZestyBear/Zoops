// File: Assets/Zoops/Scripts/Sim/HardcodedPolicy.cs
namespace Zoops.Simulation
{
    /// <summary>
    /// MVP policy:
    /// - Steer toward nearest sensed plant (today's food).
    /// - If no plant sensed, return Intent.None (world may choose to preserve prior intent).
    /// </summary>
    public sealed class HardcodedPolicy : DecisionPolicy
    {
        public Intent StepBrain(in Observation observation, float brainDt)
        {
            if (!observation.HasNearestPlant) return Intent.None;

            float dist = observation.PlantDist;
            if (dist <= 0.000001f) return Intent.None;

            // Normalize (dx,dy) to get a direction.
            float inv = 1f / dist;

            return new Intent
            {
                MoveX = observation.PlantDx * inv,
                MoveY = observation.PlantDy * inv
            };
        }
    }
}
