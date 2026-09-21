using HarmonyLib;

namespace Norsemen;

[HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
public static class VerifyClient
{
	private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
	{
		if (!__instance.IsServer() || RpcHandlers.ValidatedPeers.Contains(rpc))
		{
			return true;
		}
		NorsemenPlugin.LogWarning("Peer (" + rpc.GetSocket().GetHostName() + ") never sent version or couldn't due to previous disconnect, disconnecting");
		rpc.Invoke("Error", new object[1] { 3 });
		return false;
	}

	private static void Postfix(ZNet __instance)
	{
        //IL_001d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0023: Expected O, but got Unknown
        ZRoutedRpc.instance.InvokeRoutedRPC(
			"NorsemenRequestAdminSync",
			new ZPackage());
    }
}
