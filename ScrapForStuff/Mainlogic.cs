using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ScrapForStuff
{
    [HarmonyPatch(typeof(PLTraderInfo), "SellComponent", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]

   
    internal class Mainlogic
    {
        public static Dictionary<PLTraderInfo,List<PLWare>> SoldComponents = new Dictionary<PLTraderInfo, List<PLWare>>();

   
        public static void Postfix(PLTraderInfo __instance, int inShipID, int inNetID, int inPrice, int inPlayer)
        {
            if (__instance is PLShop_Scrapyard && PhotonNetwork.isMasterClient)
            {
                var counter = __instance.MyPDE.ServerWareIDCounter;
                var soldComp = __instance.MyPDE.Wares[counter - 1];

                if (!SoldComponents.ContainsKey(__instance))
                {
                    SoldComponents.Add(__instance,new List<PLWare>());
                }

                SoldComponents[__instance].Add(soldComp);
                
            }
        }
    }

    [HarmonyPatch(typeof(PLWarpDrive),"OnWarpTo")]
    internal class OnWarp
    {
        public static void Postfix(PLWarpDrive __instance)
        {
            if (PhotonNetwork.isMasterClient)
            PLServer.Instance.StartCoroutine(new Wrapper().ProcessSoldComponentsOnWarp());
        }
    }

    [HarmonyPatch(typeof(PLTraderInfo),"BuyComponent")]
    internal class BuyPatch
    {
        public static void Postfix(PLTraderInfo __instance, int inShipID, int Hash, int inPrice, int wareID, int inPlayerID)
        {
            if (__instance is PLShop_Scrapyard && PhotonNetwork.isMasterClient)
            {
                if (Mainlogic.SoldComponents[__instance].FirstOrDefault(c => c.NetID == wareID) != default)
                {
                    Mainlogic.SoldComponents[__instance].Remove(Mainlogic.SoldComponents[__instance].FirstOrDefault(c => c.NetID == wareID));
                }
                
            }
        }

    }

    internal class Wrapper
    {
        public IEnumerator ProcessSoldComponentsOnWarp()
        {
            
            foreach(KeyValuePair <PLTraderInfo,List<PLWare>> KvP in Mainlogic.SoldComponents)
            {
                yield return new WaitForSeconds(0.2f);
                var trader = KvP.Key;
                var compList = KvP.Value;

                foreach (var c in compList)
                {
                    var comp = c as PLShipComponent;
                    var level = comp.Level;
                    if (comp.ActualSlotType == ESlotType.E_COMP_PROGRAM || comp.ActualSlotType == ESlotType.E_COMP_VIRUS)
                    {
                        level = 0;
                    }

                    var compInData = trader.GetWareIDOfWare(comp);

                    if (compInData != -1)
                    {
                        trader.photonView.RPC("RemoveWare", PhotonTargets.All, new object[]
                        {
                        compInData
                        });
                        trader.MyPDE.ServerAddWare(PLScrapCargo.CreateScrap(0, level, 0));
                    }
                }

                
                
            }
            Mainlogic.SoldComponents.Clear();
            
        }

        public static int GetKey(TraderPersistantDataEntry trader, PLWare comp)
        {
            foreach (var c in trader.Wares)
            {
                if (c.Value == comp)
                {
                    return c.Key;
                }
            }
            return -1;
        }
    }
}
