using UnityEngine;
using Zoops.Simulation;
using Zoops.View;

[RequireComponent(typeof(Collider2D))]
public class ZoopView : MonoBehaviour
{
    [Header("Injected")]
    [SerializeField] private SimulationRunner runner;

    [Header("Identity Bridge")]
    [SerializeField] private SimEntityHandle entityHandle;

    /// <summary>
    /// Called by the spawner at instantiation time.
    /// Keeps dependency explicit (no FindFirstObjectByType magic).
    /// </summary>
    public void BindRunner(SimulationRunner injectedRunner)
    {
        runner = injectedRunner;
    }

    private void Reset()
    {
        entityHandle = GetComponent<SimEntityHandle>();

        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    private void Awake()
    {
        if (entityHandle == null)
        {
            entityHandle = GetComponent<SimEntityHandle>();
            if (entityHandle == null)
                entityHandle = gameObject.AddComponent<SimEntityHandle>();
        }

        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    private void LateUpdate()
    {
        if (runner == null) return;

        SimulationWorld world = runner.World;
        if (world == null) return;

        // Spawner must assign EntityId.
        if (entityHandle.EntityId < 0)
            return;

        if (!world.TryGetZoop(entityHandle.EntityId, out var zoop) || zoop == null || !zoop.IsAlive)
            return;

        transform.position = new Vector3(zoop.X, zoop.Y, 0f);
    }
}
