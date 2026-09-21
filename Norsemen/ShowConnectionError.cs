using HarmonyLib;
using TMPro;

namespace Norsemen;

[HarmonyPatch(typeof(FejdStartup), "ShowConnectError")]
public class ShowConnectionError
{
	private static void Postfix(FejdStartup __instance)
	{
		if (__instance.m_connectionFailedPanel.activeSelf)
		{
			__instance.m_connectionFailedError.fontSizeMax = 25f;
			__instance.m_connectionFailedError.fontSizeMin = 15f;
			TMP_Text connectionFailedError = __instance.m_connectionFailedError;
			connectionFailedError.text = connectionFailedError.text + "\n" + NorsemenPlugin.ConnectionError;
		}
	}
}
