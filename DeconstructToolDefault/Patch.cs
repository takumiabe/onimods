using HarmonyLib;

using System;
using System.Collections.Generic;

namespace DeconstructOnlyBuilding
{
    [HarmonyPatch(typeof(FilteredDragTool))]
    [HarmonyPatch("GetDefaultFilters")]
    public class Patch
    {
        [HarmonyPostfix]
        private static void SetFilterToBuilding(FilteredDragTool __instance, Dictionary<string, ToolParameterMenu.ToggleState> filters)
        {
            if (__instance is DeconstructTool)
            {
                Debug.Log("DeconstructToolDefault is BUILDINGS!");
                filters[ToolParameterMenu.FILTERLAYERS.ALL] = ToolParameterMenu.ToggleState.Off;
                filters[ToolParameterMenu.FILTERLAYERS.BUILDINGS] = ToolParameterMenu.ToggleState.On;
                // DebugOutput("deconstruct", filters);
            }
        }

        private static void DebugOutput(string tag, Dictionary<string, ToolParameterMenu.ToggleState> filterTargets)
        {
            foreach (KeyValuePair<string, ToolParameterMenu.ToggleState> kv in filterTargets)
            {
                Debug.Log(String.Format("{0}: {1}=>{2}", tag, kv.Key, kv.Value.ToString()));
            }
        }
    }
}
