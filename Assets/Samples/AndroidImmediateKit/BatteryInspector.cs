using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NativeInspector
{
    public class BatteryInspector : MonoBehaviour
    {
        public TextMeshProUGUI text;


        public void UpdateState()
        {
            text.text = $"{SystemInfo.batteryStatus} {SystemInfo.batteryLevel}";
        }

        private void Update()
        {
            UpdateState();
        }
    }
}