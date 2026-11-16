using System;
using UnityEngine.Scripting;

namespace PurrNet.Transports
{
    /// <summary>
    /// The player signed in through PurrServices (dev.purrnet.services), if any: PurrServices sets it when a player
    /// signs in and clears it when they sign out, so PurrNet's own services can say who a request is for without
    /// depending on that package (like <see cref="PurrServicesProjectLink"/>). Today the recording uploader uses it to
    /// tie a recording to the player who made it, which is what lets deleting that player's account delete it too.
    /// It lives in memory only and is never written anywhere.
    /// PurrServices finds this class by name (it also supports PurrNet versions without it), so it is kept by
    /// managed code stripping.
    /// </summary>
    [Preserve]
    public static class PurrServicesPlayerSession
    {
        /// <summary>The session token PurrServices got at sign-in, or null when nobody is signed in.</summary>
        public static string sessionToken { get; private set; }

        /// <summary>The signed-in player's id (<c>provider:id</c>), or null.</summary>
        public static string playerId { get; private set; }

        /// <summary>The scope the session belongs to (<c>production</c>, <c>editor</c>, ...), or null for the default.</summary>
        public static string scope { get; private set; }

        public static bool isSignedIn => !string.IsNullOrEmpty(sessionToken) && !string.IsNullOrEmpty(playerId);

        /// <summary>Called by PurrServices when a player signs in (or the session is renewed).</summary>
        [Preserve]
        public static void Set(string sessionToken, string playerId, string scope)
        {
            if (string.IsNullOrWhiteSpace(sessionToken) || string.IsNullOrWhiteSpace(playerId))
            {
                Clear();
                return;
            }
            PurrServicesPlayerSession.sessionToken = sessionToken.Trim();
            PurrServicesPlayerSession.playerId = playerId.Trim();
            PurrServicesPlayerSession.scope = string.IsNullOrWhiteSpace(scope) ? null : scope.Trim();
        }

        /// <summary>Called by PurrServices when the player signs out, or their account is deleted.</summary>
        [Preserve]
        public static void Clear()
        {
            sessionToken = null;
            playerId = null;
            scope = null;
        }

        /// <summary>The headers that name the player to a PurrNet service, or none when nobody is signed in.</summary>
        internal static void Apply(Action<string, string> setHeader)
        {
            if (!isSignedIn)
                return;
            setHeader("X-Player-Session", sessionToken);
            if (scope != null)
                setHeader("X-Purr-Scope", scope);
        }
    }
}
