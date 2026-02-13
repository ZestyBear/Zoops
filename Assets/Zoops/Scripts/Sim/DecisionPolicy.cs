// File: Assets/Zoops/Scripts/Sim/DecisionPolicy.cs
namespace Zoops.Simulation
{
    /// <summary>
    /// Sim-only decision policy ("brain").
    /// Deterministic mapping: Observation -> Intent.
    /// No Unity types. No direct world mutation.
    /// </summary>
    public interface DecisionPolicy
    {
        /// <param name="observation">What the Zoop can sense right now (sim-owned).</param>
        /// <param name="brainDt">Fixed simulated time step for brain updates.</param>
        /// <returns>Desired intent. Physics lane applies it under world rules.</returns>
        Intent StepBrain(in Observation observation, float brainDt);
    }
}
