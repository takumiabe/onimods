using HarmonyLib;

namespace PauseOnReadyToPrint
{
    [HarmonyPatch(typeof(Telepad.States))]
    [HarmonyPatch("InitializeStates")]
    public class Patches
    {
        public static void Postfix(Telepad.States __instance)
        {
            // Telepadの状態がopenに遷移したときに、ゲームを一時停止する
            __instance.open.Enter("trap", delegate (Telepad.StatesInstance smi)
            {
                if(SpeedControlScreen.Instance != null)
                {
                    // 音は鳴らす (true)。クラッシュではない？ので第2引数はfalseで問題ないと思う
                    SpeedControlScreen.Instance.Pause(true, false);
                }
            }
            );
        }
    }
}
