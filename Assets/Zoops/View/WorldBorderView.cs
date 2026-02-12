// File: Assets/Zoops/View/WorldBorderView.cs
using UnityEngine;
using Zoops.Simulation;

namespace Zoops.View
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class WorldBorderView : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private SimulationRunner runner;

        [Header("Appearance")]
        [Tooltip("Z position used for the border line.")]
        [SerializeField] private float z = 0f;

        [Tooltip("If true, redraw every frame (useful while iterating).")]
        [SerializeField] private bool redrawEveryFrame = false;

        private LineRenderer _lr;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();

            // Border is a closed rectangle: 5 points (last = first)
            _lr.positionCount = 5;
            _lr.loop = false; // we explicitly close it
            _lr.useWorldSpace = true;

            if (runner == null)
                runner = FindFirstObjectByType<SimulationRunner>();
        }

        private void OnEnable()
        {
            if (runner != null)
                runner.WorldChanged += Redraw;
        }

        private void OnDisable()
        {
            if (runner != null)
                runner.WorldChanged -= Redraw;
        }

        private void Start()
        {
            Redraw();
        }

        private void Update()
        {
            if (redrawEveryFrame)
                Redraw();
        }

        private void Redraw()
        {
            if (runner == null || runner.World == null) return;

            SimulationWorld w = runner.World;

            float minX = w.MinX;
            float maxX = w.MaxX;
            float minY = w.MinY;
            float maxY = w.MaxY;

            _lr.SetPosition(0, new Vector3(minX, minY, z));
            _lr.SetPosition(1, new Vector3(maxX, minY, z));
            _lr.SetPosition(2, new Vector3(maxX, maxY, z));
            _lr.SetPosition(3, new Vector3(minX, maxY, z));
            _lr.SetPosition(4, new Vector3(minX, minY, z));
        }
    }
}
