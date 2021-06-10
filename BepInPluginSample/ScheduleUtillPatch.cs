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

        private static ConfigEntry<bool> isSetSlotAllMaid;
        private static ConfigEntry<bool> isSetScheduleAllMaid;
        private static ConfigEntry<bool> isSetRandomCommu;



        public static bool IsSetSlotAllMaid { get => isSetSlotAllMaid.Value; set => isSetSlotAllMaid.Value = value; }
        public static bool IsSetScheduleAllMaid { get => isSetScheduleAllMaid.Value; set => isSetScheduleAllMaid.Value = value; }
        public static bool IsSetRandomCommu { get => isSetRandomCommu.Value; set => isSetRandomCommu.Value = value; }

        public static void init(ConfigFile Config)
        {
            isSetSlotAllMaid = Config.Bind("Patch", "isSetSlotAllMaid", false);
            isSetScheduleAllMaid = Config.Bind("Patch", "isSetScheduleAllMaid", false);
            isSetRandomCommu = Config.Bind("Patch", "isSetRandomCommu", false);
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

    }
}
