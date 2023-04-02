using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace R3DCore.CM.Extensions
{
    public static class PlayerExtension
    {
        public static int RemoveCards(this Player player)
        {
            return (int)typeof(Player).InvokeMember("RemoveCards",
                BindingFlags.Instance | BindingFlags.InvokeMethod |
                BindingFlags.NonPublic, null, player, new object[] { });
        }
    }
}