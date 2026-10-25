using Il2Cpp;

using Il2CppSteamworks;

using MelonLoader;

namespace DataCenter_MoreModules;

// The game has no mod handshake in coop, so every player with the mod announces it in the Steam lobby:
// the host through lobby data, every member through its own member data. Bulk boxes are only offered when
// everybody in the lobby has the mod, because a vanilla player would only ever see 5 slots of such a box.
public static class CoopGate
{
    private const string MarkerKey = "mm.ver";
    private const string MarkerValue = "1";

    private static ulong _lobbyId;

    public static void OnLobbyCreated(ulong lobbyId)
    {
        _lobbyId = lobbyId;
        try
        {
            _ = SteamMatchmaking.SetLobbyData(new CSteamID(lobbyId), MarkerKey, MarkerValue);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[BulkBox] Could not announce the mod in the lobby: {ex}");
        }
    }

    public static void OnLobbyEnter(ulong lobbyId)
    {
        _lobbyId = lobbyId;
        try
        {
            SteamMatchmaking.SetLobbyMemberData(new CSteamID(lobbyId), MarkerKey, MarkerValue);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[BulkBox] Could not announce the mod to the lobby: {ex}");
        }
    }

    public static bool BulkAllowed()
    {
        var session = CoopSession.Instance;
        if (session == null || !session.IsActive)
        {
            return true;
        }

        // In a session but no lobby seen: not safe to assume the others have the mod
        if (_lobbyId == 0UL)
        {
            return false;
        }

        try
        {
            var lobby = new CSteamID(_lobbyId);
            if (string.IsNullOrEmpty(SteamMatchmaking.GetLobbyData(lobby, MarkerKey)))
            {
                return false;
            }

            var memberCount = SteamMatchmaking.GetNumLobbyMembers(lobby);
            for (var i = 0; i < memberCount; i++)
            {
                var member = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i);
                if (string.IsNullOrEmpty(SteamMatchmaking.GetLobbyMemberData(lobby, member, MarkerKey)))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[BulkBox] Could not read the lobby, bulk boxes stay off: {ex}");
            return false;
        }
    }
}
