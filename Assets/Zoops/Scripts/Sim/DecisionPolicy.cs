namespace Zoops.Simulation
{
    /// <summary>
    /// Sim-only decision policy.
    /// Produces intent (desired movement direction) for a specific Zoop.
    /// No Unity types allowed.
    /// </summary>
    public interface DecisionPolicy
    {
        /// <param name="world">Authoritative sim world state.</param>
        /// <param name="zoop">The zoop being controlled.</param>
        /// <param name="brainDt">Fixed simulated time step for brain updates.</param>
        void StepBrain(SimulationWorld world, ZoopSim zoop, float brainDt);
    }
}
