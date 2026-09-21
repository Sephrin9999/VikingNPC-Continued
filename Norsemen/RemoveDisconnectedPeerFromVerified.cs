using HarmonyLib;

namespace Norsemen;

[HarmonyPatch(typeof(ZNet), "Disconnect")]
public static class RemoveDisconnectedPeerFromVerified
{
	private static void Prefix(ZNetPeer peer, ref ZNet __instance)
	{
		if (__instance.IsServer())
		{
            NorsemenPlugin.LogInfo(
				"Peer (" + peer.m_rpc.GetSocket().GetHostName() +
				") disconnected, removing from validated list");
            RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
		}
	}
}
