using System.Collections.Generic;

namespace JPB.Communication.Shared
{
    /// <summary>
    /// Guid Container
    /// </summary>
    public static class NetworkListControler
    {
        static NetworkListControler()
        {
            Guids = new List<string>();
        }

        internal static List<string> Guids;

        public static IEnumerable<string> GetGuids()
        {
            return Guids.ToArray();
        }
    }
}