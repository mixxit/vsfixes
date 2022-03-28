using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace vsfixes.src.Systems
{
    [HarmonyPatch]
    public sealed class BedfixMod : ModSystem
    {
        private readonly Harmony harmony;
        public BedfixMod()
        {
            harmony = new Harmony("BedfixMod");
            harmony.PatchAll();
        }

        public override void Start(ICoreAPI api)
        {
            harmony.PatchAll();
            base.Start(api);
        }

        public override double ExecuteOrder()
        {
            /// Worldgen:
            /// - GenTerra: 0 
            /// - RockStrata: 0.1
            /// - Deposits: 0.2
            /// - Caves: 0.3
            /// - Blocklayers: 0.4
            /// Asset Loading
            /// - Json Overrides loader: 0.05
            /// - Load hardcoded mantle block: 0.1
            /// - Block and Item Loader: 0.2
            /// - Recipes (Smithing, Knapping, Clayforming, Grid recipes, Alloys) Loader: 1
            /// 
            return 1.1;
        }
    }

    [HarmonyPatch(typeof(BlockEntityBed), "DidMount")]
    public class BlockEntityBed_DidMount
    {
        [HarmonyPrefix]
        public static bool Prefix(BlockEntityBed __instance, EntityAgent entityAgent)
        {
            if (__instance.MountedBy != null && __instance.MountedBy != entityAgent)
            {
                entityAgent.TryUnmount();
                return false;
            }

            FieldInfo fieldInfoHoursTotal = typeof(BlockEntityBed).GetField("hoursTotal", BindingFlags.NonPublic |
                                              BindingFlags.Instance);
            

            __instance.MountedBy = entityAgent;
            if (__instance?.Api?.Side == EnumAppSide.Server)
            {
                __instance.RegisterGameTickListener((dt) => RestPlayer(__instance,dt), 200);
                fieldInfoHoursTotal.SetValue(__instance,__instance.Api.World.Calendar.TotalHours);
            }

            EntityBehaviorTiredness ebt = __instance.MountedBy?.GetBehavior("tiredness") as EntityBehaviorTiredness;
            if (ebt != null) ebt.IsSleeping = true;

            return false;
        }

        private static void RestPlayer(BlockEntityBed __instance, float dt)
        {
            MethodInfo methodInfoRestPlayer = typeof(BlockEntityBed).GetMethod("RestPlayer", BindingFlags.NonPublic |
                                              BindingFlags.Instance);
            methodInfoRestPlayer.Invoke(__instance, new object[] { dt });
        }
    }
}
