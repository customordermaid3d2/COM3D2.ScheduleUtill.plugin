using BepInEx.Configuration;
using COM3D2.Lilly.Plugin;
using HarmonyLib;
using MaidStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using wf;
using Yotogis;

namespace BepInPluginSample
{
    class YotogiOldPatch
    {
        public static List<YotogiOldStageUnit> yotogiStageUnits = new List<YotogiOldStageUnit>();

        private static ConfigEntry<bool> isSelect;
        private static ConfigEntry<bool> isAddSkill;

        public static bool IsSelect { get => isSelect.Value; set => isSelect.Value = value; }
        public static bool IsAddSkill { get => isAddSkill.Value; set => isAddSkill.Value = value; }


        public static void init(ConfigFile Config)
        {
            isSelect = Config.Bind("Patch", "isSelect", false);
            isAddSkill = Config.Bind("Patch", "isAddSkill", false);
        }


        [HarmonyPatch(typeof(YotogiOldStageSelectManager), "OnCall")]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPrefix]
        public static void OnCallPre()//YotogiStageSelectManager __instance
        {
            if (IsSelect)
            {
                yotogiStageUnits.Clear();
            }
        }

        [HarmonyPatch(typeof(YotogiOldStageUnit), "SetStageData", new Type[] { typeof(YotogiOld.StageData), typeof(bool) })]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void SetStageData(YotogiOld.StageData stage_data, bool enabled, YotogiOldStageUnit __instance, UILabel ___name_label_)
        {
            if (enabled)
            {
                if (IsSelect)
                {
                    yotogiStageUnits.Add(__instance);
                }
            }
            else
            {
                ___name_label_.text = stage_data.draw_name;
                //this.name_label_.text = "??????";
            }

        }

        [HarmonyPatch(typeof(YotogiOldStageSelectManager), "OnCall")]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void OnCallPost(YotogiOldStageSelectManager __instance)
        {
            if (IsSelect)
            {
                Select();
            }
        }
        public static void Select()
        {
            if (yotogiStageUnits.Count > 0)
            {
                // 배경을 바꾸기만 함. 실제로도 적용 되긴 함
                YotogiOldStageUnit yotogiStageUnit = yotogiStageUnits[UnityEngine.Random.Range(0, yotogiStageUnits.Count)];
                yotogiStageUnit.UpdateBG();

                YotogiOld.StageData stage_data = yotogiStageUnit.stage_data;
                YotogiOldStageSelectManager.SelectStage(stage_data);
                GameMain.Instance.MainCamera.SetFromScriptOnTarget(stage_data.camera_data.stage_select_camera_center, stage_data.camera_data.stage_select_camera_radius, stage_data.camera_data.stage_select_camera_rotate);
                GameMain.Instance.SoundMgr.PlayBGM(yotogiStageUnit.stage_data.bgm_file, 1f, true);
            }
        }

        [HarmonyPatch(typeof(YotogiOldSkillSelectManager), "OnCall")]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void OnCall()
        {
            if (IsAddSkill)
            {
                YotogiOldPatch.AddSkill(true);
                //AddSkill();
            }
        }


        public static YotogiOldSkillContainerViewer instance;
        public static Maid maid;

        [HarmonyPatch(typeof(YotogiOldSkillContainerViewer), MethodType.Constructor, new Type[] { typeof(GameObject), typeof(MonoBehaviour) })]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void Constructor(GameObject root_obj, MonoBehaviour parent, YotogiOldSkillContainerViewer __instance)
        {
            //if (configEntryUtill["SetResolution"])
            {
                MyLog.LogMessage("YotogiOldSkillContainerViewer.Constructor"
                    , root_obj.name
                    , parent.name
                    );
            }

            instance = __instance;
        }

        [HarmonyPatch(typeof(YotogiOldSkillContainerViewer), "Init", MethodType.Normal)]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void Init(Maid maid)
        {
            YotogiOldPatch.maid = maid;
        }

        public static List<Skill.Old.Data> skillList = new List<Skill.Old.Data>();

        public static void AddSkill(bool listClear = true)
        {
            if (instance == null)
            {
                MyLog.LogMessage("YotogiSkillContainerViewer.AddSkill instance==null"
                );
                return;
            }

            if (listClear)
            {
                YotogiOld.Stage key = YotogiOld.Stage.プレイル\u30FCム;
                if (!string.IsNullOrEmpty(YotogiOldStageSelectManager.StageName))
                {
                    try
                    {
                        key = (YotogiOld.Stage)Enum.Parse(typeof(YotogiOld.Stage), YotogiOldStageSelectManager.StageName);
                    }
                    catch
                    {
                        MyLog.LogError("Yotogi.Stage enum convert error.\n" + YotogiOldStageSelectManager.StageName, false);
                        return;
                    }
                }

                YotogiOld.StageData setting_stage_data_ = YotogiOld.stage_data_list[key];

                ReadOnlySortedDictionary<int, YotogiSkillData> oldDatas = maid.status.yotogiSkill.oldDatas;
                foreach (YotogiSkillData yotogiSkillData in oldDatas.GetValueArray())
                {
                    Skill.Old.Data oldData = yotogiSkillData.oldData;
                    if (oldData.IsExecStage(setting_stage_data_.stage) && oldData.IsExecMaid(maid.status))
                    {
                        MyLog.LogMessage("AddSkillOld"
                            , oldData.category
                            , oldData.id
                            , oldData.name
                            );

                        skillList.Add(oldData);
                    }
                }
            }

            if (skillList.Count > 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    try
                    {
                        instance.AddSkill(skillList.ElementAt(UnityEngine.Random.Range(0, skillList.Count)));
                    }
                    catch (Exception e)
                    {
                        MyLog.LogError(e.ToString());
                    }
                }
            }
        }

    }
}
