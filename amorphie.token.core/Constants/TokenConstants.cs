using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Enums;

namespace amorphie.token.core.Constants
{
    public static class TokenConstants
    {
        public static readonly FrozenDictionary<LogonChannel, string> LogonChannelMap = new Dictionary<LogonChannel, string>()
        {
            {LogonChannel.OnMobil,"On Mobil"}
        }.ToFrozenDictionary();

        public static readonly ImmutableList<string> SupportedPkceAlgs = ["S256"];
    }
}