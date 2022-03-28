
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
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace vsfixes.src.Systems
{
    [HarmonyPatch]
    public sealed class PoulticeOthersMod : ModSystem
    {
        private readonly Harmony harmony;
        public PoulticeOthersMod()
        {
            harmony = new Harmony("PoulticeOthersMod");
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

    [HarmonyPatch(typeof(ItemPoultice), "OnHeldInteractStop")]
    public class ItemPoultice_OnHeldInteractStop
    {
        [HarmonyPrefix]
        public static bool Prefix(BlockEntityBed __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (secondsUsed > 0.7f && byEntity.World.Side == EnumAppSide.Server)
            {
                JsonObject attr = slot.Itemstack.Collectible.Attributes;
                float health = attr["health"].AsFloat();
                var entityToHeal = entitySel?.Entity;
                if (entityToHeal == null)
                    entityToHeal = byEntity;
                
                entityToHeal.ReceiveDamage(new DamageSource()
                {
                    Source = EnumDamageSource.Internal,
                    Type = health > 0 ? EnumDamageType.Heal : EnumDamageType.Poison
                }, Math.Abs(health));

                slot.TakeOut(1);
                slot.MarkDirty();
            }

            return false;
        }
    }
}
