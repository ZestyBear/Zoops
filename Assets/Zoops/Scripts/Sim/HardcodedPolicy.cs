using System;

namespace Zoops.Simulation
{
    /// <summary>
    /// MVP policy:
    /// - Steer toward nearest alive food (wrap-aware).
    /// - If no food alive, keep current intent.
    /// Only writes ZoopSim intent; does not move the Zoop.
    /// </summary>
    public sealed class HardcodedPolicy : DecisionPolicy
    {
        public void StepBrain(SimulationWorld world, ZoopSim zoop, float brainDt)
        {
            if (zoop == null || !zoop.IsAlive) return;

            FoodSim best = null;
            float bestD2 = float.PositiveInfinity;

            var foods = world.Foods;
            for (int i = 0; i < foods.Count; i++)
            {
                var f = foods[i];
                if (f == null || !f.IsAlive) continue;

                float dx = world.ShortestWrappedDeltaX(zoop.X, f.X);
                float dy = world.ShortestWrappedDeltaY(zoop.Y, f.Y);

                float d2 = dx * dx + dy * dy;
                if (d2 < bestD2)
                {
                    bestD2 = d2;
                    best = f;
                }
            }

            if (best == null) return;

            float lenSq = bestD2;
            if (lenSq > 0.0000001f)
            {
                float invLen = 1f / (float)Math.Sqrt(lenSq);
                float dx = world.ShortestWrappedDeltaX(zoop.X, best.X);
                float dy = world.ShortestWrappedDeltaY(zoop.Y, best.Y);

                zoop.IntentX = dx * invLen;
                zoop.IntentY = dy * invLen;
            }
        }
    }
}
