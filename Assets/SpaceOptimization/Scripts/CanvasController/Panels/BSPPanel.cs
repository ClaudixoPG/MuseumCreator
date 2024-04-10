using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpaceOptimization
{
    public class BSPPanel : Panel
    {
        [Header("BSP Configuration")]
        public TMP_InputField widthInput;
        public TMP_InputField heightInput;
        public TMP_InputField maxRoomSizeInput;

        public void ShowBSPPanel()
        {
            ShowMenu();
        }

        public void HideBSPPanel()
        {
            HideMenu();
        }

        public override List<string> GetPanelData()
        {
            List<string> data = new List<string>();
            data.Add(widthInput.text);
            data.Add(heightInput.text);
            data.Add(maxRoomSizeInput.text);

            return data;
        }
    }
}