using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceOptimization
{
    public class TSPPanel : Panel
    {
        [Header("TSP Configuration")]
        public TMP_Dropdown solverTypeDropdown;

        public void ShowTSPPanel()
        {
            ShowMenu();
        }

        public void HideTSPPanel()
        {
            HideMenu();
        }

        public override List<string> GetPanelData()
        {
            List<string> data = new List<string>();
            data.Add(solverTypeDropdown.value.ToString());

            return data;
        }
    }
}