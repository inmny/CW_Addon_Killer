using NCMS;
using UnityEngine;
using Cultivation_Way;
using Cultivation_Way.Actions;
using Cultivation_Way.Library;
using Cultivation_Way.Animation;
namespace Addon_Namespace
{
	[ModEntry]
	public class Addon_Main_Class : CW_Addon
	{
		public override void awake(){
			// 不要在此处添加代码，除非你知道你在做什么
			// DO NOT code here.
			load_mod_info(System.Type.GetType("Mod"));
		}
		public override void initialize(){
			Log("Hello CW Addon!");
			// 在这里初始化模组内容
			// Initalize your mod content here
			
			add_example_spell();
		}
		
		
		private void add_example_spell(){
			CW_AnimationSetting anim_setting = new CW_AnimationSetting();
            anim_setting.end_action = example_spell_end_action;
            CW_EffectManager.instance.load_as_controller("example_spell_anim", "effects/example/", controller_setting: anim_setting, base_scale: 0.015f);

            CW_Asset_Spell spell = new CW_Asset_Spell(
                    id: name+"_example", anim_id: "example_spell_anim", 
                    element:new CW_Element(), element_type_limit: null, 
                    rarity: 1, free_val: 1, 
                    cost: 0.05f, min_cost: 20,
                    learn_level: 1, cast_level: 1, 
					can_get_by_random: true,
                    cultisys_black_or_white_list: true, 
                    cultisys_list: null, 
					banned_races: null, 
                    target_type: CW_Spell_Target_Type.ACTOR, 
                    target_camp: CW_Spell_Target_Camp.ENEMY, 
                    triger_type: CW_Spell_Triger_Type.ATTACK, 
                    anim_type: CW_Spell_Animation_Type.ON_TARGET, 
                    damage_action: CW_SpellAction_Damage.defualt_damage, 
                    anim_action: CW_SpellAction_Anim.default_anim, 
                    spell_action: null, 
                    check_and_cost_action: CW_SpellAction_Cost.default_check_and_cost
                    );
            spell.add_tag(CW_Spell_Tag.ATTACK);
            spell.add_tag(CW_Spell_Tag.IMMORTAL);
            CW_Library_Manager.instance.spells.add(spell);
			Log("Add spell '{0}'", spell.id);
		}
		private static void example_spell_end_action(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            ((CW_Actor)anim.src_object).regen_health(anim.cost_for_spell, 1);
        }
	}
}