using System;
using UnityEngine;
using Zoops.Simulation;
using Zoops.UI.Views;

namespace Zoops.UI
{
    public sealed class SimUIPresenter : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private SimulationRunner runner;

        [Header("Views")]
        [SerializeField] private SelectedZoopPanelView selectedZoopPanel;

        [Header("Refresh")]
        [SerializeField, Range(1f, 60f)] private float refreshHz = 15f;

        private int? _selectedEntityId;
        private float _refreshAccum;

        private void Awake()
        {
            if (runner == null)
                runner = FindFirstObjectByType<SimulationRunner>();

            selectedZoopPanel?.ShowNoneSelected();
        }

        private void OnEnable()
        {
            if (runner != null)
            {
                runner.WorldChanged += HandleWorldChanged;
                runner.SimEventRaised += HandleSimEventRaised;
            }
        }

        private void OnDisable()
        {
            if (runner != null)
            {
                runner.WorldChanged -= HandleWorldChanged;
                runner.SimEventRaised -= HandleSimEventRaised;
            }
        }

        public void SetSelectedEntity(int? entityId)
        {
            _selectedEntityId = entityId;
            _refreshAccum = 0f;
            RefreshSelectedPanel(force: true);
        }

        private void HandleWorldChanged()
        {
            _selectedEntityId = null;
            _refreshAccum = 0f;
            selectedZoopPanel?.ShowNoneSelected();
        }

        private void HandleSimEventRaised(SimEvent e)
        {
            // Keep event-driven contract; Update throttles actual refresh.
            if (e.Type == SimEventType.EntityDied && _selectedEntityId != null && e.EntityId == _selectedEntityId.Value)
            {
                // Selected entity died -> hide immediately
                selectedZoopPanel?.ShowNoneSelected();
            }
        }

        private void Update()
        {
            if (_selectedEntityId == null) return;
            if (runner == null || runner.World == null) return;

            _refreshAccum += Time.unscaledDeltaTime;

            float step = 1f / Mathf.Max(1f, refreshHz);
            if (_refreshAccum >= step)
            {
                _refreshAccum = 0f;
                RefreshSelectedPanel(force: false);
            }
        }

        private void RefreshSelectedPanel(bool force)
        {
            if (selectedZoopPanel == null) return;

            SimulationWorld w = runner != null ? runner.World : null;
            if (w == null || _selectedEntityId == null)
            {
                selectedZoopPanel.ShowNoneSelected();
                return;
            }

            if (!w.TryGetZoop(_selectedEntityId.Value, out ZoopSim zoop) || zoop == null || !zoop.IsAlive)
            {
                selectedZoopPanel.ShowNoneSelected();
                return;
            }

            float simTickSeconds = runner.SimTickSeconds;
            float ageSeconds = (w.Tick - zoop.BirthTick) * simTickSeconds;

            float maxEnergy = Mathf.Max(0.0001f, w.ZoopMaxEnergy);
            float energy01 = Mathf.Clamp01(zoop.Energy / maxEnergy);

            selectedZoopPanel.ShowSelected(
                name: $"Zoop {zoop.EntityId}",
                ageSeconds: ageSeconds,
                energy01: energy01
            );
        }
    }
}
