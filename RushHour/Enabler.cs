using RushHourLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RushHour
{
    public class Enabler : ModExtensionHandler
    {
        public override void OnEnabled()
        {
            Debug.Log("ModExtensionHandler for RushHour");
        }
    }
}
