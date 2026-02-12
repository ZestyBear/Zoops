using System.Collections.Generic;
using UnityEngine;
using Zoops.Simulation;

namespace Zoops.View
{
    public sealed class SimEntityViewSpawner : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private SimulationRunner runner;

        [Header("Prefabs")]
        [SerializeField] private GameObject zoopPrefab;
        [SerializeField] private GameObject foodPrefab;

        [Header("Hierarchy Buckets")]
        [SerializeField] private string zoopsBucketName = "WorldViews_Zoops";
        [SerializeField] private string foodsBucketName = "WorldViews_Foods";

        private Transform _zoopsBucket;
        private Transform _foodsBucket;

        private readonly Dictionary<int, GameObject> _viewsById = new Dictionary<int, GameObject>();

        private void Awake()
        {
            EnsureBucketsExist();
        }

        private void OnEnable()
        {
            if (runner != null)
            {
                runner.WorldChanged += HandleWorldChanged;
                runner.SimEventRaised += HandleSimEvent;
            }
        }

        private void OnDisable()
        {
            if (runner != null)
            {
                runner.WorldChanged -= HandleWorldChanged;
                runner.SimEventRaised -= HandleSimEvent;
            }
        }

        private void EnsureBucketsExist()
        {
            if (_zoopsBucket == null)
                _zoopsBucket = FindOrCreateChildBucket(zoopsBucketName);

            if (_foodsBucket == null)
                _foodsBucket = FindOrCreateChildBucket(foodsBucketName);
        }

        private Transform FindOrCreateChildBucket(string name)
        {
            Transform child = transform.Find(name);
            if (child != null) return child;

            var go = new GameObject(name);
            go.transform.SetParent(transform, worldPositionStays: false);
            return go.transform;
        }

        private void HandleWorldChanged()
        {
            EnsureBucketsExist();
            ClearAllViews();

            var world = runner.World;
            if (world == null) return;

            for (int i = 0; i < world.Zoops.Count; i++)
                SpawnView(world.Zoops[i].EntityId, EntityKind.Zoop);

            for (int i = 0; i < world.Foods.Count; i++)
                SpawnView(world.Foods[i].EntityId, EntityKind.Food);
        }

        private void HandleSimEvent(SimEvent e)
        {
            if (e.Type == SimEventType.EntityBorn)
            {
                SpawnView(e.EntityId, e.Kind);
            }
            else if (e.Type == SimEventType.EntityDied)
            {
                DestroyView(e.EntityId);
            }
        }

        private void SpawnView(int entityId, EntityKind kind)
        {
            if (entityId < 0) return;
            if (_viewsById.ContainsKey(entityId)) return;

            EnsureBucketsExist();

            GameObject prefab;
            Transform parent;
            string instanceName;

            switch (kind)
            {
                case EntityKind.Zoop:
                    prefab = zoopPrefab;
                    parent = _zoopsBucket;
                    instanceName = $"Zoop_{entityId}";
                    break;

                case EntityKind.Food:
                    prefab = foodPrefab;
                    parent = _foodsBucket;
                    instanceName = $"Food_{entityId}";
                    break;

                default:
                    return;
            }

            if (prefab == null)
            {
                Debug.LogWarning($"[ViewSpawner] No prefab assigned for {kind}. Cannot spawn {instanceName}.");
                return;
            }

            var go = Instantiate(prefab, parent);
            go.name = instanceName;

            // Prevent prefab-saved transform offsets from leaking into runtime.
            go.transform.localPosition = Vector3.zero;

            // Identity
            var handle = go.GetComponent<SimEntityHandle>();
            if (handle == null)
                handle = go.AddComponent<SimEntityHandle>();
            handle.EntityId = entityId;

            // Explicit dependency injection (no FindFirstObjectByType)
            if (runner != null)
            {
                var zoopView = go.GetComponent<ZoopView>();
                if (zoopView != null) zoopView.BindRunner(runner);

                var foodView = go.GetComponent<FoodView>();
                if (foodView != null) foodView.BindRunner(runner);
            }

            _viewsById[entityId] = go;
        }

        private void DestroyView(int entityId)
        {
            if (!_viewsById.TryGetValue(entityId, out var go))
                return;

            if (go != null)
                Destroy(go);

            _viewsById.Remove(entityId);
        }

        private void ClearAllViews()
        {
            foreach (var kv in _viewsById)
            {
                if (kv.Value != null)
                    Destroy(kv.Value);
            }

            _viewsById.Clear();
        }
    }
}
