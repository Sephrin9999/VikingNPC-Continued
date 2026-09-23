using HarmonyLib;

namespace Norsemen;

[HarmonyPatch(typeof(ZNetView), "Awake")]
public static class VikingPersistencePatch
{
    [HarmonyPostfix]
    public static void Postfix(ZNetView __instance)
    {
        if (__instance == null)
        {
            return;
        }

        Viking viking =
            __instance.GetComponent<Viking>();

        if (viking == null)
        {
            return;
        }

        ZDO zdo = __instance.GetZDO();

        if (zdo == null)
        {
            return;
        }

        if (!zdo.Persistent)
        {
            zdo.Persistent = true;
        }
    }
}
