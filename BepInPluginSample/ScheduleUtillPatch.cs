using BepInEx.Configuration;
using COM3D2.Lilly.Plugin;
using HarmonyLib;
using Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Yotogis;

namespace BepInPluginSample
{
    public class ScheduleUtillPatch
    {

        public static ScheduleScene m_scheduleApi;

        public static ConfigFile Config;

        private static ConfigEntry<bool> isSetSlotAllMaid;
        private static ConfigEntry<bool> isSetScheduleAllMaid;
        private static ConfigEntry<bool> isSetRandomCommu;
        private static ConfigEntry<bool> isSelect;
        private static ConfigEntry<bool> isAddSkill;

        public static bool IsSetSlotAllMaid { get => isSetSlotAllMaid.Value; set => isSetSlotAllMaid.Value = value; }
        public static bool IsSetScheduleAllMaid { get => isSetScheduleAllMaid.Value; set => isSetScheduleAllMaid.Value = value; }
        public static bool IsSetRandomCommu { get => isSetRandomCommu.Value; set => isSetRandomCommu.Value = value; }
        public static bool IsSelect        {            get => isSelect.Value;            set => isSelect.Value = value;        }
        public static bool IsAddSkill {            get => isAddSkill.Value;            set => isAddSkill.Value = value;        }

        public static void init(ConfigFile Config)
        {
            ScheduleUtillPatch.Config = Config;
            isSetSlotAllMaid = Config.Bind("Patch", "isSetSlotAllMaid", false);
            isSetScheduleAllMaid = Config.Bind("Patch", "isSetScheduleAllMaid", false);
            isSetRandomCommu = Config.Bind("Patch", "isSetRandomCommu", false);
            isSelect = Config.Bind("Patch", "isSelect", false);
            isAddSkill = Config.Bind("Patch", "isAddSkill", false);
        }


       [HarmonyPostfix, HarmonyPatch(typeof(ScheduleMgr), "LoadData")]
        // private Dictionary<string, ScheduleCtrl.MaidStatusAndTaskUnit> LoadData()
        private static void LoadData(
            ScheduleMgr __instance
            , ScheduleScene ___m_scheduleApi
        ) // string __m_BGMName 못가져옴
        {
            MyLog.LogMessage("LoadData"
                );
            m_scheduleApi = ___m_scheduleApi;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DailyMgr), "OpenDaytimePanel")]
        public static void OpenDaytimePanel()
        {

            if (IsSetSlotAllMaid)
            {
                ScheduleUtillTool.SetSlotAllMaid();
            }

            if (IsSetScheduleAllMaid)
            {
                ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.DayTime);
                ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.Night);
            }

            if (IsSetRandomCommu)
            {
                ScheduleUtillTool.SetRandomCommu(true);
                ScheduleUtillTool.SetRandomCommu(false);
            }
        }

        public static List<YotogiStageUnit> yotogiStageUnits = new List<YotogiStageUnit>();

        [HarmonyPatch(typeof(YotogiStageSelectManager), "OnCall")]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPrefix]
        public static void OnCallPre()//YotogiStageSelectManager __instance
        {
            if (IsSelect)
            {
                yotogiStageUnits.Clear();
            }
        }

        [HarmonyPatch(typeof(YotogiStageSelectManager), "OnCall")]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void OnCallPost(YotogiStageSelectManager __instance)
        {
            if (IsSelect)
            {
                Select();
            }
        }

        public static void Select()
        {

            /*
[Message:     Lilly] UTY.GetChildObject , SystemShortcut , Base/Grid/Help , False
[Message:     Lilly] UTY.GetChildObject , StageSelectPanel , StageSelectViewer/StageViewer/Contents/StageUnitParent , False
[Message:     Lilly] UTY.GetChildObject , StageSelectPanel , Ok , False
[Message:     Lilly] UTY.GetChildObject , StageUnit(Clone) , Parent , False
[Message:     Lilly] UTY.GetChildObject , Parent , Icon , False
[Message:     Lilly] UTY.GetChildObject , Parent , Name , False
[Message:     Lilly] UTY.GetChildObject , Parent , StarGroup , False
[Message:     Lilly] UTY.GetChildObject , Parent , StarMarkGroup , False
[Message:     Lilly] UTY.GetChildObject , Parent , StarGroup , False
[Message:     Lilly] YotogiStageUnit.SetStageData
             * */

            MyLog.LogMessage("YotogiStageSelectManager.Select", yotogiStageUnits.Count);

            if (yotogiStageUnits.Count > 0)
            {
                // 배경을 바꾸기만 함. 실제로도 적용 되긴 함
                var yotogiStageUnit = yotogiStageUnits[UnityEngine.Random.Range(0, yotogiStageUnits.Count)];
                yotogiStageUnit.UpdateBG();

                YotogiStage.Data stage_data = yotogiStageUnit.stage_data;
                YotogiStageSelectManager.SelectStage(stage_data, GameMain.Instance.CharacterMgr.status.isDaytime);
                stage_data.stageSelectCameraData.Apply();
                GameMain.Instance.SoundMgr.PlayBGM(stage_data.bgmFileName, 1f, true);
            }

        }


        [HarmonyPatch(typeof(YotogiSkillSelectManager), "OnCall")]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void OnCall()
        {
            if (IsAddSkill)
            {
                AddSkill(true);
                //AddSkill();
            }
        }

        public static YotogiSkillSelectManager yotogiSkillSelectManager;

        [HarmonyPatch(typeof(YotogiSkillSelectManager), "Awake")]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void Awake(YotogiSkillSelectManager __instance)
        {
            //if (configEntryUtill["SetResolution"])
            {
                MyLog.LogMessage("YotogiSkillSelectManager.Awake");
            }
            yotogiSkillSelectManager = __instance;
        }

        public static YotogiSkillContainerViewer yotogiSkillContainerViewer;

        [HarmonyPatch(typeof(YotogiSkillContainerViewer), MethodType.Constructor, new Type[] { typeof(GameObject), typeof(MonoBehaviour) })]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void Constructor( YotogiSkillContainerViewer __instance)
        {
            yotogiSkillContainerViewer = __instance;
        }

        public static Maid maid;

        [HarmonyPatch(typeof(YotogiSkillContainerViewer), "Init", MethodType.Normal)]//, new Type[] { typeof(int), typeof(int), typeof(bool) }
        [HarmonyPostfix]
        public static void Init(Maid maid)
        {
            ScheduleUtillPatch.maid = maid;
        }

        public static List<Skill.Data> skillList = new List<Skill.Data>();

        public static void AddSkill(bool listClear = true)
        {
            if (yotogiSkillContainerViewer == null)
            {
                MyLog.LogMessage("YotogiSkillContainerViewer.AddSkill instance==null"
                );
                return;
            }

            if (listClear)
            {
                YotogiStage.Data setting_stage_data_;

                if (YotogiStageSelectManager.SelectedStage != null)
                {
                    setting_stage_data_ = YotogiStageSelectManager.SelectedStage;
                }
                else
                {
                    setting_stage_data_ = YotogiStage.GetAllDatas(true)[0];
                }

                //foreach (Skill.Data.SpecialConditionType type in Enum.GetValues(typeof(Skill.Data.SpecialConditionType)))
                foreach (Skill.Data.SpecialConditionType type in yotogiSkillSelectManager.conditionSetting.checkBoxTypes)
                {
                    //bool enabled = false;
                    Dictionary<int, YotogiSkillListManager.Data> dictionary = YotogiSkillListManager.CreateDatas(maid.status, true, type);
                    foreach (KeyValuePair<int, YotogiSkillListManager.Data> keyValuePair in dictionary)
                    {
                        YotogiSkillListManager.Data value = keyValuePair.Value;
                        if (value.skillData.IsExecStage(setting_stage_data_))
                        {
                            MyLog.LogMessage("AddSkill"
                            , type
                            , value.skillData.category
                            , value.skillData.id
                            , value.skillData.name
                            , value.maidStatusSkillData != null
                            );
                            if (value.maidStatusSkillData != null)
                                skillList.Add(value.skillData);
                            //skillOldList.Add(value.skillDataOld);
                        }
                    }
                }
            }
            int c = UnityEngine.Random.Range(0, skillList.Count);



            if (skillList.Count > 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    c = UnityEngine.Random.Range(0, skillList.Count);
                    try
                    {
                        MyLog.LogMessage("AddSkill"
                        , skillList[c].category
                        , skillList[c].id
                        , skillList[c].name
                        , skillList[c].specialConditionType
                        , skillList[c].start_call_file
                        , skillList[c].start_call_file2
                        );
                        yotogiSkillContainerViewer.AddSkill(skillList[c], false);
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
