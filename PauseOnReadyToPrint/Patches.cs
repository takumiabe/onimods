using Harmony;
using UnityEngine;

namespace PauseOnReadyToPrint
{
    public class Patches
    {

        [HarmonyPatch(typeof(HeadquartersConfig))]
        [HarmonyPatch("ConfigureBuildingTemplate")]
        public static class HeadquartersConfig_ConfigureBuildingTemplate
        {
            public static void Postfix(GameObject go, Tag prefab_tag)
            {
                LogicAlarm alarm = go.AddOrGet<LogicAlarm>();
                alarm.notificationName = STRINGS.CODEX.HEADQUARTERS.TITLE;
                alarm.notificationTooltip = STRINGS.BUILDING.STATUSITEMS.NEWDUPLICANTSAVAILABLE.NAME;
                alarm.pauseOnNotify = true;
                alarm.zoomOnNotify = false;
            }
        }

        [HarmonyPatch(typeof(Telepad.States))]
        [HarmonyPatch("InitializeStates")]
        public static class TelepadStates_InitializeStates
        {
            public static void Postfix(Telepad.States __instance)
            {
                // Telepadの状態がopenに遷移したときに、内部的なLogicAlarmにON信号を送る
                __instance.open.Enter(delegate (Telepad.StatesInstance smi)
                {
                    LogicAlarm alarm = smi.GetComponent<LogicAlarm>();
                    if (alarm == null)
                    {
                        Debug.Log("LogicAlarm not found for telepad...");
                        return;
                    }
                    alarm.OnLogicValueChanged(new LogicValueChanged { newValue = 1, portID = LogicAlarm.INPUT_PORT_ID });
                });

                // Telepadの状態がcloseに遷移したときに、内部的なLogicAlarmにOFF信号を送る
                __instance.close.Enter(delegate (Telepad.StatesInstance smi)
                {
                    LogicAlarm alarm = smi.GetComponent<LogicAlarm>();
                    if (alarm == null)
                    {
                        Debug.Log("LogicAlarm not found for telepad...");
                        return;
                    }
                    alarm.OnLogicValueChanged(new LogicValueChanged { newValue = 0, portID = LogicAlarm.INPUT_PORT_ID });
                });
            }
        }
    }
}
