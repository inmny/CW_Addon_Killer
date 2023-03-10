using NCMS;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using Cultivation_Way;
using Cultivation_Way.Utils;
using Cultivation_Way.Others;
using Cultivation_Way.Library;

namespace Addon_Killer
{
	[ModEntry]
	public class Addon_Killer_Main : CW_Addon
	{
		private static CW_Asset_CultiSys killer;
		public override void awake(){
			// 不要在此处添加代码，除非你知道你在做什么
			// DO NOT code here.
			load_mod_info(System.Type.GetType("Mod"));
		}
		public override void initialize(){
			Log("添加体系'杀孽'");
			// 在这里初始化模组内容
			// Initalize your mod content here
			Harmony.CreateAndPatchAll(typeof(Addon_Killer_Main), "CW_Addon_Killer");
			add_killer_cultisys();
			add_kill_combo_status();
		}

        private void add_kill_combo_status()
        {
            CW_BaseStats bonus_stats = new CW_BaseStats();
            CW_Asset_StatusEffect status_effect = new CW_Asset_StatusEffect(
                id: "status_kill_combo",
                anim_id: null,
                bonus_stats: bonus_stats,
                effect_val: 5,
                effect_time: 5f,
                action_on_get: copy_bonus_stats_on_get,
                action_on_update: kill_combo_update_action,
                action_on_end: null
                );
            CW_Library_Manager.instance.status_effects.add(status_effect);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Actor), "newKillAction")]
		public static void kill_bonus(Actor __instance, Actor pDeadUnit)
        {
			CW_Actor actor = (CW_Actor)__instance;
			if (!actor.has_cultisys(killer.id))
            {
				if(actor.fast_data.kills >= 100)
                {
					actor.add_cultisys(killer.id);
                }
				return;
            }
			actor.regen_health(((CW_Actor)pDeadUnit).cw_cur_stats.health_regen);
			actor.regen_wakan(((CW_Actor)pDeadUnit).cw_cur_stats.wakan_regen);
			CW_BaseStats cw_stats = actor.get_fixed_base_stats();
			cw_stats.base_stats.mod_health += 0.1f;
			cw_stats.base_stats.mod_damage += 0.1f;
			cw_stats.base_stats.mod_armor += 0.01f;
			cw_stats.base_stats.mod_attackSpeed += 0.01f;
			cw_stats.mod_spell_armor += 0.01f;
			cw_stats.mod_wakan += 0.1f;
			cw_stats.mod_wakan_regen += 0.1f;
			cw_stats.mod_shield_regen += 0.1f;
			cw_stats.age_bonus++;
			cw_stats.mod_shield += 0.1f;
			actor.clear_default_spell_timer();
            if (CW_Actor.get_attackTarget(actor) == null)
            {
				List<BaseSimObject> enemies = CW_SpellHelper.find_enemies_in_square(actor.currentTile, actor.kingdom, 8);
				foreach(BaseSimObject enemy in enemies)
                {
					if (enemy.objectType == MapObjectType.Building) continue;
					CW_Actor.set_attackedBy(actor, enemy);
					actor.try_to_set_attack_target_by_attacked_by();
					break;
                }
            }
			add_or_enpower_kill_combo((CW_Actor)actor);
			actor.setStatsDirty();
			actor.check_level_up();
        }
		private static void copy_bonus_stats_on_get(CW_StatusEffectData status_effect, BaseSimObject _obejct)
        {
            status_effect.bonus_stats = status_effect.bonus_stats.deepcopy();
        }
		
		private static void kill_combo_update_action(CW_StatusEffectData status_effect, BaseSimObject _object)
        {
            if (_object.objectType != MapObjectType.Actor) return;
            for(int i=0;i<10;i++)((CW_Actor)_object).spawnParticle(Color.red);
        }
		
		private static void add_or_enpower_kill_combo(CW_Actor actor)
		{
			CW_StatusEffectData combo_status_effect = actor.add_status_effect("status_kill_combo", "status_kill_combo");
			combo_status_effect.next_update_action_time = 0;
			
			combo_status_effect.bonus_stats.base_stats.damage += (int)UnityEngine.Mathf.Sqrt(actor.fast_data.kills);
			CW_BaseStats cw_stats = combo_status_effect.bonus_stats;
			cw_stats.base_stats.mod_health += 2f;
			cw_stats.base_stats.mod_damage += 2f;
			cw_stats.base_stats.mod_armor += 0.5f;
			cw_stats.base_stats.mod_attackSpeed += 0.5f;
			cw_stats.mod_spell_armor += 0.5f;
			cw_stats.mod_wakan += 2f;
			cw_stats.mod_wakan_regen += 2f;
			cw_stats.mod_shield_regen += 2f;
			cw_stats.mod_shield += 2f;
			
			combo_status_effect.left_time = combo_status_effect.left_time>combo_status_effect.status_asset.effect_val?combo_status_effect.left_time:combo_status_effect.status_asset.effect_val;
		}
		private void add_killer_cultisys(){
			killer = CW_Library_Manager.instance.cultisys.add(
				new CW_Asset_CultiSys()
				{
					id = "killer",
					sprite_name = "iconCultiBook_bushido",
					judge = kill_judge,
					level_judge = kill_level_judge,
					addition_spell_require = new CW_Spell_Tag[] { CW_Spell_Tag.IMMORTAL, CW_Spell_Tag.ACQUIRED_POWER }
				}
			);
			CW_Asset_CultiSys immortal = CW_Library_Manager.instance.cultisys.get("immortal");
			for (int i = 0; i < CW_Constants.max_cultisys_level; i++)
            {
				killer.power_level[i] = 1 + i / (1.5f*CW_Constants.max_cultisys_level);
				killer.bonus_stats[i] = immortal.bonus_stats[i].deepcopy();
				killer.bonus_stats[i].wakan = 50*i;
				killer.bonus_stats[i].wakan_regen /= 100;
				killer.bonus_stats[i].base_stats.damage *= killer.bonus_stats[i].base_stats.damage;
				killer.bonus_stats[i].age_bonus /= 10;
			}
			killer.races_list.Add("elf");
		}
		private static bool kill_judge(CW_Actor cw_actor, CW_Asset_CultiSys cultisys)
        {
			return cw_actor.haveTrait("evil") || cw_actor.haveTrait("bloodlust");
        }
		private static bool kill_level_judge(CW_Actor cw_actor, CW_Asset_CultiSys cultisys)
		{
			if (cw_actor.cw_data.cultisys_level[cultisys.tag] <= CW_Constants.max_cultisys_level - 1 - 1 && cw_actor.fast_data.kills >= cw_actor.cw_data.cultisys_level[cultisys.tag] *10* cw_actor.cw_data.cultisys_level[cultisys.tag])
			{
				if (cw_actor.city != null)
                {

                }
                else if(Toolbox.randomChance(1-5f / (5+cw_actor.cw_data.cultisys_level[cultisys.tag])))
                {
					cw_actor.addTrait("madness");
                }
				if (cw_actor.cw_status.health_level < cultisys.power_level[cw_actor.cw_data.cultisys_level[cultisys.tag] + 1])
				{
					cw_actor.fast_data.health = (int)CW_Utils_Others.get_raw_wakan(cw_actor.fast_data.health, cw_actor.cw_status.health_level);

					cw_actor.cw_status.health_level = cultisys.power_level[cw_actor.cw_data.cultisys_level[cultisys.tag] + 1];

					cw_actor.fast_data.health = (int)CW_Utils_Others.compress_raw_wakan(cw_actor.fast_data.health, cw_actor.cw_status.health_level);
				}
				if (cw_actor.cw_status.wakan_level < cultisys.power_level[cw_actor.cw_data.cultisys_level[cultisys.tag] + 1])
				{
					cw_actor.cw_status.wakan = (int)CW_Utils_Others.get_raw_wakan(cw_actor.cw_status.wakan, cw_actor.cw_status.wakan_level);

					cw_actor.cw_status.wakan_level = cultisys.power_level[cw_actor.cw_data.cultisys_level[cultisys.tag] + 1];

					cw_actor.cw_status.wakan = (int)CW_Utils_Others.compress_raw_wakan(cw_actor.cw_status.wakan, cw_actor.cw_status.wakan_level);
				}
				return true;
			}
			return false;
		}
	}
}