// File: Assets/Zoops/Scripts/View/PlantView.cs
using UnityEngine;
using Zoops.Simulation;

namespace Zoops.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlantView : MonoBehaviour
    {
        [Header("Injected")]
        [SerializeField] private SimulationRunner runner;

        [Header("Identity Bridge")]
        [SerializeField] private SimEntityHandle entityHandle;

        private SpriteRenderer _sr;

        /// <summary>
        /// Called by the spawner at instantiation time.
        /// Keeps dependency explicit (no FindFirstObjectByType magic).
        /// </summary>
        public void BindRunner(SimulationRunner injectedRunner)
        {
            runner = injectedRunner;
        }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();

            if (entityHandle == null)
            {
                entityHandle = GetComponent<SimEntityHandle>();
                if (entityHandle == null)
                    entityHandle = gameObject.AddComponent<SimEntityHandle>();
            }

            // Hidden until we have a valid binding + alive.
            _sr.enabled = false;
        }

        private void LateUpdate()
        {
            if (runner == null) return;

            SimulationWorld world = runner.World;
            if (world == null) return;

            // Spawner must assign EntityId.
            if (entityHandle.EntityId < 0)
            {
                _sr.enabled = false;
                return;
            }

            if (!world.TryGetPlant(entityHandle.EntityId, out var plant) || plant == null)
            {
                _sr.enabled = false;
                return;
            }

            _sr.enabled = plant.IsAlive;

            if (plant.IsAlive)
                transform.position = new Vector3(plant.X, plant.Y, transform.position.z);
        }
    }
}
