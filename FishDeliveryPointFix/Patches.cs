using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using HarmonyLib;

namespace FishDeliveryPointFix
{
    [HarmonyPatch(typeof(CreatureDeliveryPoint))]
    [HarmonyPatch("RefreshCreatureCount")]
    internal class Patches
    {
        // int cell = Grid.PosToCell(this);
        // to
        // int cell = Grid.OffsetCell(Grid.PosToCell(this), this.spawnOffset);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInsturctions)
        {
            
            MethodInfo posToCell = typeof(Grid).GetMethod("PosToCell", BindingFlags.Static | BASE, null, new Type[] { typeof(KMonoBehaviour) }, new ParameterModifier[3]);
            if (posToCell == null)
            {
                Debug.Log("FishDeliveryPointFix: posToCell not found");
                return codeInsturctions;
            }
            MethodInfo newPosToCell = typeof(Patches).GetMethod("PosToReleasePoint", BindingFlags.Static | BindingFlags.InvokeMethod | BASE);
            if (newPosToCell == null)
            {
                Debug.Log("FishDeliveryPointFix: newPosToCell not found");
                return codeInsturctions;
            }

            var list = new List<CodeInstruction>(codeInsturctions);
            for(var i = 0; i < list.Count; ++i)
            {
                var instruction = list[i];
                if (instruction == null) continue;
                if(instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt && (instruction.operand as MethodInfo) == posToCell)
                {
                    Debug.Log("PATCH: PosToCell");
                    instruction.operand = newPosToCell;
                    break;
                }
            }
            return list;
        }

        private static readonly BindingFlags BASE = BindingFlags.Public | BindingFlags.NonPublic;

        private static int PosToReleasePoint(KMonoBehaviour kmono)
        {
            var fishDeliveryPoint = kmono as CreatureDeliveryPoint;
            return Grid.OffsetCell(Grid.PosToCell(fishDeliveryPoint), fishDeliveryPoint.spawnOffset);
        }
    }
}
