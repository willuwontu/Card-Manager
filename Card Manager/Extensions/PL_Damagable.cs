using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace R3DCore.CM.Extensions
{
    public static class PL_DamagableExtension
    {
        public static void RPCA_ADDCARDWITHID(this PL_Damagable pL_Damagable, int cardID)
        {
            typeof(PL_Damagable).InvokeMember("RPCA_ADDCARDWITHID",
                BindingFlags.Instance | BindingFlags.InvokeMethod |
                BindingFlags.NonPublic, null, pL_Damagable, new object[] { cardID });
        }
    }
}