using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BepInPluginSample
{
    class ScheduleUtillGUI
    {
        private static Vector2 scrollPosition;

        public static void WindowFunction(int id)
        {
            GUI.enabled = true;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("now scene.name : " + ScheduleUtill.scene_name);

            GUILayout.Label("Schedule 진입 필요.");
            GUI.enabled = ScheduleUtillPatch.m_scheduleApi != null;
            if (GUILayout.Button("슬롯에 메이드 자동 배치")) ScheduleUtillTool.SetSlotAllMaid();
            if (GUILayout.Button("슬롯의 메이드들 제거")) ScheduleUtillTool.SetSlotAllDel();
            if (GUILayout.Button("메이드 전체 자동 배치 - 주간"))   ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.DayTime);
            if (GUILayout.Button("메이드 전체 자동 배치 - 야간"))   ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.Night);
            if (GUILayout.Button("메이드 밤시중 자동 배치 - 주간")) ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.DayTime, true, false, false);
            if (GUILayout.Button("메이드 밤시중 자동 배치 - 야간")) ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.Night, true, false, false);
            if (GUILayout.Button("메이드 훈련 자동 배치 - 주간"))   ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.DayTime, false, true, false);
            if (GUILayout.Button("메이드 훈련 자동 배치 - 야간"))   ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.Night, false, true, false);
            if (GUILayout.Button("메이드 시설에 자동 배치 - 주간")) ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.DayTime, false, false, true);
            if (GUILayout.Button("메이드 시설에 자동 배치 - 야간")) ScheduleUtillTool.SetScheduleAllMaid(ScheduleMgr.ScheduleTime.Night, false, false, true);

            GUI.enabled = true;
            GUILayout.Label("매일 자동 적용.");
            if (GUILayout.Button("슬롯에_메이드_자동_배치 " + ScheduleUtillPatch.IsSetSlotAllMaid)) ScheduleUtillPatch.IsSetSlotAllMaid = !ScheduleUtillPatch.IsSetSlotAllMaid;
            if (GUILayout.Button("메이드_스케줄_자동_배치 " + ScheduleUtillPatch.IsSetScheduleAllMaid)) ScheduleUtillPatch.IsSetScheduleAllMaid = !ScheduleUtillPatch.IsSetScheduleAllMaid;
            if (GUILayout.Button("커뮤니티_자동_적용 " + ScheduleUtillPatch.IsSetRandomCommu)) ScheduleUtillPatch.IsSetRandomCommu = !ScheduleUtillPatch.IsSetRandomCommu;

            GUI.enabled = true;
            GUILayout.Label("밤시중");
            if (GUILayout.Button("스테이지 자동 선택 " + YotogiPatch.IsSelect)) YotogiPatch.IsSelect = !YotogiPatch.IsSelect;
            if (GUILayout.Button("스테이지 자동 선택 ")) YotogiPatch.Select();
            GUI.enabled = !DailyMgr.IsLegacy;
            if (GUILayout.Button("스킬 자동 선택 " + YotogiPatch.IsAddSkill)) YotogiPatch.IsAddSkill = !YotogiPatch.IsAddSkill;
            if (GUILayout.Button("스킬 자동 선택 ")) YotogiPatch.AddSkill(false);
            GUI.enabled = DailyMgr.IsLegacy;
            if (GUILayout.Button("스킬 자동 선택 old " + YotogiOldPatch.IsAddSkill)) YotogiOldPatch.IsAddSkill = !YotogiOldPatch.IsAddSkill;
            if (GUILayout.Button("스킬 자동 선택 old ")) YotogiOldPatch.AddSkill(false);


            GUILayout.EndScrollView();
            /*
            if (GUI.changed)
            {
                Debug.Log("changed");
            }
            */
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
        }
    }
}
