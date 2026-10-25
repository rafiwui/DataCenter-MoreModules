using HarmonyLib;
using Il2CppCoop.Net;
using Il2CppSteamworks;

namespace DataCenter_MoreModules.Patches;

[HarmonyPatch(typeof(SteamNetTransport), nameof(SteamNetTransport.OnLobbyCreated))]
public class AnnounceModOnLobbyCreated
{
    public static void Postfix(LobbyCreated_t e)
    {
        CoopGate.OnLobbyCreated(e.m_ulSteamIDLobby);
    }
}

[HarmonyPatch(typeof(SteamNetTransport), nameof(SteamNetTransport.OnLobbyEnter))]
public class AnnounceModOnLobbyEnter
{
    public static void Postfix(LobbyEnter_t e)
    {
        CoopGate.OnLobbyEnter(e.m_ulSteamIDLobby);
    }
}
