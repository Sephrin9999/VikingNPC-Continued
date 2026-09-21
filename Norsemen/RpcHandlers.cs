using System.Collections.Generic;

namespace Norsemen;

public static class RpcHandlers
{
	public static readonly List<ZRpc> ValidatedPeers = new List<ZRpc>();

    public static void RPC_Norsemen_Version(ZRpc rpc, ZPackage pkg)
    {
        string text = pkg.ReadString();

        NorsemenPlugin.LogInfo("Version check, local: " + NorsemenPlugin.ModVersion + ", remote: " + text);

        if (text != NorsemenPlugin.ModVersion)
        {
            NorsemenPlugin.ConnectionError = "Norsemen Installed: " + NorsemenPlugin.ModVersion + "\n Needed: " + text;

            if (ZNet.instance.IsServer())
            {
                NorsemenPlugin.LogWarning("Peer (" + rpc.GetSocket().GetHostName() +") has incompatible version, disconnecting...");
                rpc.Invoke("Error", new object[1] { 3 });
            }
        }
        else if (!ZNet.instance.IsServer())
        {
            NorsemenPlugin.LogInfo("Received same version from server!");
        }
        else
        {
            NorsemenPlugin.LogInfo(
                "Adding peer (" + rpc.GetSocket().GetHostName() +") to validated list");
            ValidatedPeers.Add(rpc);
        }
    }
}
