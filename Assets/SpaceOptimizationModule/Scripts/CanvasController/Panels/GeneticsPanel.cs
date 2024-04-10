using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceOptimization
{
    public class GeneticsPanel : Panel
    {
        [Header("Genetics Configuration")]
        public TMP_InputField populationSizeInput;
        public TMP_InputField generationsInput;
        public Toggle haveElitismToggle;
        public TMP_Dropdown crossoverTypeDropdown;
        public TMP_Dropdown mutationTypeDropdown;
        public TMP_Dropdown selectionTypeDropdown;

        public void ShowGeneticsPanel()
        {
            ShowMenu();
        }

        public void HideGeneticsPanel()
        {
            HideMenu();
        }

        public override List<string> GetPanelData()
        {
            List<string> data = new List<string>();
            data.Add(populationSizeInput.text);
            data.Add(generationsInput.text);
            data.Add(haveElitismToggle.isOn.ToString());
            //save the selected value of the dropdowns a number, then convert it to the string value
            data.Add(crossoverTypeDropdown.value.ToString());
            data.Add(mutationTypeDropdown.value.ToString());
            data.Add(selectionTypeDropdown.value.ToString());
            
            return data;
        }

    }
}
