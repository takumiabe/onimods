using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using STRINGS;

namespace DisplayDTU
{
    [HarmonyPatch(typeof(SelectToolHoverTextCard))]
    [HarmonyPatch("UpdateHoverElements")]
    internal class Patch
    {
        private static bool Prefix(SelectToolHoverTextCard __instance, List<KSelectable> hoverObjects)
        {
            new SelectToolHoverTextCardOverride(__instance).UpdateHoverElements(hoverObjects);

            // 全ての処理を奪う。他にやりようがない…
            return false;
        }
    }
}
