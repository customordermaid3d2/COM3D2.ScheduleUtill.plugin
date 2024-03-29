﻿
using MaidStatus;
using PlayerStatus;
using Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.ScheduleUtill.plugin
{
    public class ScheduleUtillTool
    {


        public static void SetSlotAllMaid()
        {
            if (ScheduleUtillPatch.m_scheduleApi == null)
            {
                ScheduleUtill.log.LogMessage("SetSlotAllMaid 스케줄 관리 접속 한번 필요");
                return;
            }


            List<Maid> maids = new List<Maid>();
            if (DailyMgr.IsLegacy)
            {
                maids.AddRange(GameMain.Instance.CharacterMgr.GetStockMaidList().Where(x => x.status.OldStatus != null));
            }
            else
            {
                maids.AddRange(GameMain.Instance.CharacterMgr.GetStockMaidList());
            }

            for (int i = 0; i < ScheduleUtillPatch.m_scheduleApi.slot.Length; i++)
            {
                if (maids.Count == 0)
                {
                    return;
                }
                Maid maid = maids[UnityEngine.Random.Range(0, maids.Count)];
                //m_scheduleApi.slot[i] 
                ScheduleUtillPatch.m_scheduleApi.SetSlot_Safe(i, maid, true, false);
                maids.Remove(maid);
            }
        }



        public static void SetSlotAllDel()
        {
            if (ScheduleUtillPatch.m_scheduleApi == null)
            {
                ScheduleUtill.log.LogMessage("SetSlotAllMaid 스케줄 관리 접속 한번 필요");
                return;
            }

            for (int i = 0; i < ScheduleUtillPatch.m_scheduleApi.slot.Length; i++)
            {
                ScheduleUtillPatch.m_scheduleApi.SetSlot_Safe(i, null, true, false);
            }
        }


        public static void SetScheduleAllMaid(ScheduleMgr.ScheduleTime scheduleTime, bool isYotogi = true, bool isTraining = true, bool isSetFacility = true)
        {
            if (ScheduleUtillPatch.m_scheduleApi == null)
            {
                ScheduleUtill.log.LogMessage("SetSlotAllMaid 스케줄 관리 접속 한번 필요");
                return;
            }

            WorkIdResetAll(scheduleTime);

            // 사용 가능한 메이드 슬롯 목록
            List<int> slots = new List<int>();
            List<int> slotsn = new List<int>();

            ScheduleData[] scheduleDatas = GameMain.Instance.CharacterMgr.status.scheduleSlot;

            SetSlots(scheduleDatas, slots);

            int c1 = 40, c2 = 40, c3=0, c4=0;
            if (isYotogi && isTraining && isSetFacility)
            {
                c3 = UnityEngine.Random.Range(0, 40);
                c4 = UnityEngine.Random.Range(0, 40);
                if (c3 < c4)
                {
                    c1 = c3;
                    c2 -= c4;
                }
                else
                {
                    c1 -= c3;
                    c2 = c4;
                }
            }

            if (isYotogi)
                SetSchedule(scheduleTime, ScheduleType.Yotogi, slots, slotsn, c1);

            if (isTraining)
                SetSchedule(scheduleTime, ScheduleType.Training, slots, slotsn, c2);

            if (isSetFacility)
                SetWorkFacility(scheduleTime, slots, slotsn);

            SetFinal();
        }

        public static void SetWorkFacility(ScheduleMgr.ScheduleTime scheduleTime, List<int> slots, List<int> slotsn)
        {
            slots.AddRange(slotsn);
            slotsn.Clear();

            if (slots.Count == 0)
            {
                return;
            }

            Facility facility;
            FacilityDataTable.FacilityDefaultData defaultData;
            ScheduleCSVData.Work workData;
            Maid maid;
            int fn;
            int sn;

            var facilitys = GameMain.Instance.FacilityMgr.GetFacilityArray().ToList();

            while (facilitys.Count > 1)
            {
                fn = UnityEngine.Random.Range(1, facilitys.Count);
                if (facilitys[fn] == null)
                {
                    facilitys.RemoveAt(fn);
                    continue;
                }

                facility = facilitys[fn];
                defaultData = facility.defaultData;
                workData = defaultData.workData;

                if (facility.minMaidCount <= slots.Count && workData.id != 0)
                {
                    for (int k = 0; k < facility.minMaidCount; k++)
                    {
                        sn = UnityEngine.Random.Range(0, slots.Count);
                        maid = GameMain.Instance.CharacterMgr.status.GetScheduleSlot(slots[sn]);

                        SetWorkId(scheduleTime, workData.id, slots[sn]);

                        facility.AllocationMaid(maid, scheduleTime);

                        slots.Remove(slots[sn]);
                    }
                    if (slots.Count == 0)
                    {
                        return;
                    }
                }

                facilitys.RemoveAt(fn);
            }
        }

        internal static void WorkIdReset(Maid maid, ScheduleMgr.ScheduleTime time)
        {
            if (time == ScheduleMgr.ScheduleTime.DayTime)
            {
                    int num = ScheduleAPI.NoonWorkRandom(maid);
                    maid.status.noonWorkId = num;

                    ScheduleAPI.AddTrainingFacility(maid, num, ScheduleMgr.ScheduleTime.DayTime);
                    if (maid.status.heroineType == HeroineType.Sub)
                    {
                        ScheduleAPI.SubMaidDefaultFacility(maid, num, ScheduleMgr.ScheduleTime.DayTime);
                    }
                }
            else if (time == ScheduleMgr.ScheduleTime.Night)
            {                
                    int num = ScheduleAPI.NightWorkRandom(maid);
                    maid.status.nightWorkId = num;

                    ScheduleAPI.AddTrainingFacility(maid, num, ScheduleMgr.ScheduleTime.Night);
                    if (maid.status.heroineType == HeroineType.Sub)
                    {
                        ScheduleAPI.SubMaidDefaultFacility(maid, num, ScheduleMgr.ScheduleTime.Night);
                    }
                }
        }

        internal static void WorkIdResetAll(ScheduleMgr.ScheduleTime time)
        {
            List<Maid> maids = GameMain.Instance.CharacterMgr.GetStockMaidList();

            if (time == ScheduleMgr.ScheduleTime.DayTime)
            {
                foreach (var maid in maids)
                {
                    int num = ScheduleAPI.NoonWorkRandom(maid);
                    maid.status.noonWorkId = num;

                    ScheduleAPI.AddTrainingFacility(maid, num, ScheduleMgr.ScheduleTime.DayTime);
                    if (maid.status.heroineType == HeroineType.Sub)
                    {
                        ScheduleAPI.SubMaidDefaultFacility(maid, num, ScheduleMgr.ScheduleTime.DayTime);
                    }
                }
            }
            else if (time == ScheduleMgr.ScheduleTime.Night)
            {
                foreach (var maid in maids)
                {
                    int num = ScheduleAPI.NightWorkRandom(maid);
                    maid.status.nightWorkId = num;

                    ScheduleAPI.AddTrainingFacility(maid, num, ScheduleMgr.ScheduleTime.Night);
                    if (maid.status.heroineType == HeroineType.Sub)
                    {
                        ScheduleAPI.SubMaidDefaultFacility(maid, num, ScheduleMgr.ScheduleTime.Night);
                    }
                }
            }
        }

        public static void SetSchedule(ScheduleMgr.ScheduleTime scheduleTime, ScheduleType scheduleType, List<int> slots, List<int> slotsn, int cnt = 40)
        {
            if (scheduleType == ScheduleType.Work || cnt == 0)
            {
                return;
            }

            slots.AddRange(slotsn);
            slotsn.Clear();

            if (slots.Count == 0)
            {
                return;
            }

            Maid maid;
            List<ScheduleBase> list = new List<ScheduleBase>();
            List<ScheduleBase> scheduleData;
            int sc;
            int wc;

            for (int i = 0; i < cnt; i++)
            {
                sc = UnityEngine.Random.Range(0, slots.Count);

                maid = GameMain.Instance.CharacterMgr.status.GetScheduleSlot(slots[sc]);
                if (maid.status.heroineType == HeroineType.Sub)
                {
                    slotsn.Add(slots[sc]);
                    slots.Remove(slots[sc]);
                    continue;
                }

                scheduleData = ScheduleUtillPatch.m_scheduleApi.slot[slots[sc]].scheduleData.Where(
                    x =>
                    x.workType == scheduleType
                    && x.enabled
                    ).ToList();

                list.Clear();

                foreach (ScheduleBase scheduleBase in scheduleData)
                {
                    if (DailyMgr.IsLegacy && ScheduleCSVData.WorkLegacyDisableId.Contains(scheduleBase.id))
                        continue;

                    if (!PersonalEventBlocker.IsEnabledScheduleTask(maid.status.personal, scheduleBase.id))
                        continue;

                    list.Add(scheduleBase);
                }
                if (list.Count > 0)
                {
                    wc = UnityEngine.Random.Range(0, list.Count);
                    SetWorkId(scheduleTime, list[wc].id, slots[sc]);
                }
                slots.Remove(slots[sc]);
                if (slots.Count == 0)
                {
                    break;
                }
            }
        }



        public static void SetSlots(ScheduleData[] scheduleDatas, List<int> slots)
        {
            for (int i = 0; i < scheduleDatas.Length; i++)
            {
                if (scheduleDatas[i].maid_guid == string.Empty)
                    continue;

                //if (GameMain.Instance.CharacterMgr.status.GetScheduleSlot(i).status.heroineType == HeroineType.Sub)
                //    continue;

                slots.Add(i);
            }
        }
        /*
        */

        public static void SetWorkId(ScheduleMgr.ScheduleTime workTime, int taskId, int slotId)
        {
            if (ScheduleCSVData.AllData.ContainsKey(taskId))
            {
                ScheduleCSVData.ScheduleBase scheduleBase = ScheduleCSVData.AllData[taskId];
                //int slotId = 0;
                //for (int i = 0; i < 40; i++)
                //{
                //    Maid scheduleSlot = GameMain.Instance.CharacterMgr.status.GetScheduleSlot(i);
                //    if (scheduleSlot != null && scheduleSlot == this.m_scheduleCtrl.SelectedMaid)
                //    {
                //        slotId = i;
                //    }
                //}
                ScheduleTaskCtrl.TaskType type = scheduleBase.type;
                if (type != ScheduleTaskCtrl.TaskType.Training && type != ScheduleTaskCtrl.TaskType.Work)
                {
                    if (type == ScheduleTaskCtrl.TaskType.Yotogi)
                    {
                        ScheduleUtillPatch.m_scheduleApi.SetNightWorkSlot_Safe(workTime, slotId, taskId);
                    }
                }
                else
                {
                    ScheduleUtillPatch.m_scheduleApi.SetNoonWorkSlot_Safe(workTime, slotId, taskId);
                }
            }
        }

        public static void SetFinal()
        {
            if (!DailyMgr.IsLegacy)
            {
                GameMain.Instance.FacilityMgr.UpdateFacilityAssignedMaidData();
            }
            ScheduleAPI.MaidWorkIdErrorCheck(true);
        }

        public static void SetRandomCommu(bool isDaytime)
        {
            List<Maid> list = ScheduleAPI.CanCommunicationMaids(isDaytime);
            if (list.Count > 0)
            {
                int i = UnityEngine.Random.Range(0, list.Count);
                if (isDaytime)
                {
                    foreach (Maid item in list)
                    {
                        item.status.noonCommu = false;
                    }
                    list[i].status.noonCommu = true;
                }
                else
                {
                    foreach (var item in list)
                    {
                        item.status.nightCommu = false;
                    }
                    list[i].status.nightCommu = true;
                }
            }
            else
            {
                ScheduleUtill.log.LogMessage($"ScheduleAPI.SetRandomCommu count 0 {isDaytime}"
            );
            }
        }
    }
}
