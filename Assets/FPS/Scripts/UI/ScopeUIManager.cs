using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class ScopeUIManager : MonoBehaviour
    {
        public GameObject scopeUI;

        public void OnScope()
        {
            scopeUI.SetActive(true);
        }
        public void OffScope()
        {
            scopeUI.SetActive(false);
        }
    }
}