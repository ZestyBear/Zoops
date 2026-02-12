// File: SelectedZoopPanelView.cs
using UnityEngine;
using TMPro;
using Zoops.UI; // MaskedFillBar

namespace Zoops.UI.Views
{
    /// <summary>
    /// Dumb view binder for the "Selected Zoop" panel.
    /// Presenter pushes values in; this never reads the sim.
    /// </summary>
    public sealed class SelectedZoopPanelView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Text (TMP)")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text ageText;

        [Header("Energy Bar")]
        [Tooltip("Width-driven masked bar fill (recommended over Image.fillAmount for 9-sliced rounded bars).")]
        [SerializeField] private MaskedFillBar energyBar;

        private void Awake()
        {
            if (root == null)
                root = gameObject;
        }

        public void ShowNoneSelected()
        {
            if (root != null) root.SetActive(false);
        }

        public void ShowSelected(string name, float ageSeconds, float energy01)
        {
            if (root != null) root.SetActive(true);

            if (nameText != null) nameText.text = name;
            if (ageText != null) ageText.text = $"Age: {ageSeconds:0.0}s";

            if (energyBar != null)
                energyBar.Set01(energy01);
        }
    }
}
