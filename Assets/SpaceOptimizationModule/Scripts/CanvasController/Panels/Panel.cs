using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOptimization
{
    public abstract class Panel : MonoBehaviour
    {
        bool isMenuActive = false;

        //Show the panel using LeanTween to animate the menu, entering from the left and hidden other menus
        public void ShowMenu()
        {
            if (!isMenuActive)
            {
                LeanTween.moveX(gameObject, 0, 0.5f).setEase(LeanTweenType.easeOutBack);
                isMenuActive = true;
            }
            else
            {
                HideMenu();
            }
        }

        public void HideMenu()
        {
            LeanTween.moveX(gameObject, -1000, 0.5f).setEase(LeanTweenType.easeInBack);
            isMenuActive = false;
        }
        public abstract List<string> GetPanelData();

    }
}