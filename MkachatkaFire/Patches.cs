using HarmonyLib;

namespace MkachatkaFire
{
    [HarmonyPatch(typeof(Geyser))]
    [HarmonyPatch("OnSpawn")]
    public class Patch
    {
        private static readonly EventSystem.IntraObjectHandler<Geyser> OnRefreshUserMenuDelegate =
            new EventSystem.IntraObjectHandler<Geyser>(delegate(Geyser component, object data)
        {
            if (!Game.Instance.SandboxModeActive) return;

            var startButton = new KIconButtonMenu.ButtonInfo(
                "status_item_toilet_needs_emptying",
                "Force Eruption",
                delegate () {
                    ElementEmitter emitter = Traverse.Create(component.smi.master).Field("emitter").GetValue<ElementEmitter>();
                    emitter.SetEmitting(true);
                },
                global::Action.NumActions, null, null, null, "Start the eruption immediately", true);
            Game.Instance.userMenu.AddButton(component.gameObject, startButton, 1f);

            var stopButton = new KIconButtonMenu.ButtonInfo(
                "status_item_toilet_needs_emptying",
                "Stop Eruption",
                delegate () {
                    ElementEmitter emitter = Traverse.Create(component.smi.master).Field("emitter").GetValue<ElementEmitter>();
                    emitter.SetEmitting(false);
                },
                global::Action.NumActions, null, null, null, "Stop the eruption immediately", true);
            Game.Instance.userMenu.AddButton(component.gameObject, stopButton, 1f);
        });

        private static void Postfix(Geyser __instance)
        {
            __instance.Subscribe<Geyser>(493375141, OnRefreshUserMenuDelegate);
        }
    }
}
