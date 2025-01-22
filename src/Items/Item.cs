using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using EntWatchSharp.Helpers;

namespace EntWatchSharp.Items
{
    public class Item : ItemConfig
    {
        public CCSWeaponBase WeaponHandle;
        public CCSPlayerController Owner;
        public CParticleSystem Particle;
        public CDynamicProp Prop;

		public double fDelay;
        public byte Team;

        public bool ThisItem(uint weaponindex)
        {
            if (WeaponHandle.Index == weaponindex) return true;
            return false;
        }
        public Item(ItemConfig cNewItem, CCSWeaponBase weapon)
        {
            WeaponHandle = weapon;
            Name = cNewItem.Name;
            ShortName = cNewItem.ShortName;
            Color = cNewItem.Color;
            HammerID = cNewItem.HammerID;
			if (cNewItem.GlowColor.Length == 4) GlowColor = cNewItem.GlowColor;
            else GlowColor = [255, 255, 255, 255];
			BlockPickup = cNewItem.BlockPickup;
            if(BlockPickup || Cvar.GlobalBlock) WeaponHandle.CanBePickedUp = false;
            else WeaponHandle.CanBePickedUp = true;
			AllowTransfer = cNewItem.AllowTransfer;
			ForceDrop = cNewItem.ForceDrop;
			Chat = cNewItem.Chat;
			Hud = cNewItem.Hud;
			UsePriority = cNewItem.UsePriority;
            TriggerID = cNewItem.TriggerID;
            SpawnerID = cNewItem.SpawnerID;
			AbilityList = [];
            foreach(Ability ability in cNewItem.AbilityList.ToList())
            {
				AbilityList.Add(new Ability(ability));
			}
			Owner = null;

            if (Cvar.GlowParticle)
            {
                Particle = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system");
                if (Particle != null && Particle.IsValid)
                {
                    Particle.EffectName = "particles/overhead_icon_fx/player_ping_ground_rings.vpcf";
                    Particle.TintCP = 1;
                    Particle.Tint = System.Drawing.Color.FromArgb(GlowColor[3], GlowColor[0], GlowColor[1], GlowColor[2]);
                    Particle.StartActive = true;
                    Particle.Teleport(WeaponHandle.CBodyComponent?.SceneNode?.AbsOrigin, WeaponHandle.CBodyComponent?.SceneNode?.AbsRotation, new Vector(0, 0, 0));
                    Particle.DispatchSpawn();
                    Particle.AcceptInput("SetParent", WeaponHandle, null, "!activator");
                }
            }

			if (Cvar.GlowSpawn)
            {
				WeaponHandle!.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(WeaponHandle.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));
				WeaponHandle.Glow.GlowColorOverride = System.Drawing.Color.FromArgb(255, GlowColor[0], GlowColor[1], GlowColor[2]);
				WeaponHandle.Glow.GlowColor.X = GlowColor[0];
                WeaponHandle.Glow.GlowColor.Y = GlowColor[1];
                WeaponHandle.Glow.GlowColor.Z = GlowColor[2];
                WeaponHandle.Glow.GlowType = 3;
                WeaponHandle.Glow.GlowRange = 5000;
                WeaponHandle.Glow.GlowRangeMin = 1;
                WeaponHandle.Glow.GlowTeam = -1;
                WeaponHandle.Glow.GlowTime = 1;
                WeaponHandle.Glow.Glowing = true;
			}

			if (!Cvar.GlowSpawn) EnableGlow();

			EW.UpdateTime();
            fDelay = EW.fGameTime - Cvar.Delay;

            UI.EWSysInfo("Info.Item.Spawn", 8,  Name, weapon.Index);
        }

        public void EnableGlow()
        {
            if (Cvar.GlowProp)
            {
                Prop = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
                if (Prop != null && Prop.IsValid)
                {
                    Prop.Spawnflags = 256;
                    Prop!.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(Prop.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));

                    //Prop.Teleport(WeaponHandle.CBodyComponent?.SceneNode?.AbsOrigin, WeaponHandle.CBodyComponent?.SceneNode?.AbsRotation, new Vector(0, 0, 0));
                    Prop.SetModel(WeaponHandle.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
                    Prop.Glow.GlowColorOverride = System.Drawing.Color.FromArgb(255, GlowColor[0], GlowColor[1], GlowColor[2]);
                    Prop.Glow.GlowColor.X = GlowColor[0];
                    Prop.Glow.GlowColor.Y = GlowColor[1];
                    Prop.Glow.GlowColor.Z = GlowColor[2];
                    Prop.Glow.GlowType = 3;
                    Prop.Glow.GlowRange = 5000;
                    Prop.Glow.GlowRangeMin = 1;
                    Prop.Glow.GlowTeam = -1;
                    Prop.Glow.GlowTime = 1;
                    Prop.Glow.Glowing = true;
                    //Prop.AcceptInput("SetParent", WeaponHandle, null, "!activator");
                    Prop.AcceptInput("FollowEntity", WeaponHandle, null, "!activator");
                }
            }
		}

        public void DisableGlow()
        {
			if (Cvar.GlowProp)
            {
                if (Prop != null && Prop.IsValid)
                {
                    Prop.Remove();
                }
                Prop = null;
			}
		}

		~Item()
		{
			AbilityList.Clear();
		}

		public bool CheckDelay()
        {
            EW.UpdateTime();
            if (fDelay < EW.fGameTime) return true;
            return false;
        }

        public void SetDelay()
        {
            EW.UpdateTime();
            fDelay = EW.fGameTime + Cvar.Delay;
        }
    }
}
