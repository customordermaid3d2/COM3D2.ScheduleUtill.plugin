using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using COM3D2API;
using HarmonyLib;
using LillyUtill.MyWindowRect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.ScheduleUtill.plugin
{
    class MyAttribute
    {
        public const string PLAGIN_NAME = "ScheduleUtill";
        public const string PLAGIN_VERSION = "22.2.23";
        public const string PLAGIN_FULL_NAME = "COM3D2.ScheduleUtill.plugin";
    }

    [BepInPlugin(MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, MyAttribute.PLAGIN_VERSION)]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    //[BepInPlugin("COM3D2.Sample.Plugin", "COM3D2.Sample.Plugin", "21.6.6")]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
    //[BepInProcess("COM3D2x64.exe")]
    public class ScheduleUtill : BaseUnityPlugin
    {

        // 단축키 설정파일로 연동
        public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;

        // GUI ON OFF 설정파일로 저장
        private static ConfigEntry<bool> IsGUIOn;

        public static bool isGUIOn
        {
            get => IsGUIOn.Value;
            set => IsGUIOn.Value = value;
        }

        Harmony harmony;

        public static WindowRectUtill myWindowRect;
        public static ManualLogSource log;

        /// <summary>
        ///  게임 실행시 한번만 실행됨
        /// </summary>
        public void Awake()
        {
            log = Logger;
            log.LogInfo("Awake");
                       
            // 단축키 기본값 설정
            ShowCounter = Config.Bind("KeyboardShortcut", "KeyboardShortcut0", new BepInEx.Configuration.KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl));

            // 일반 설정값
            IsGUIOn = Config.Bind("GUI", "isGUIOn", false);

            ScheduleUtillPatch.init(Config);
            YotogiPatch.init(Config);
            YotogiOldPatch.init(Config);
            
            // 위치 저장용 테스트 json
            myWindowRect = new WindowRectUtill(Config, MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, "SU");

            // 기어 메뉴 추가. 이 플러그인 기능 자체를 멈추려면 enabled 를 꺽어야함. 그러면 OnEnable(), OnDisable() 이 작동함
            //SystemShortcutAPI.AddButton(MyAttribute.PLAGIN_FULL_NAME, new Action(delegate () { enabled = !enabled; }), MyAttribute.PLAGIN_NAME, MyUtill.ExtractResource(Properties.Resources.icon));
            SystemShortcutAPI.AddButton(MyAttribute.PLAGIN_FULL_NAME, new Action(delegate () { isGUIOn = !isGUIOn; }), MyAttribute.PLAGIN_NAME + " " + ScheduleUtill.ShowCounter.Value.ToString(), Properties.Resources.icon);
        }



        public void OnEnable()
        {
            log.LogInfo("OnEnable");
            // 하모니 패치
            harmony = Harmony.CreateAndPatchAll(typeof(ScheduleUtillPatch));
            harmony.PatchAll(typeof(YotogiPatch));
            harmony.PatchAll(typeof(YotogiOldPatch));            
        }

        /// <summary>
        /// 게임 실행시 한번만 실행됨
        /// </summary>
       //public void Start()
       //{
       //    log.LogMessage("Start");
       //}


        private int windowId = new System.Random().Next();     

        public void OnGUI()
        {
            if (!isGUIOn)
            {
                return;
            }

            myWindowRect.WindowRect = GUILayout.Window(windowId, myWindowRect.WindowRect, ScheduleUtillGUI.WindowFunction, "", GUI.skin.box);
        }

    

        public void OnDisable()
        {
            log.LogInfo("OnDisable");

            harmony.UnpatchSelf();// ==harmony.UnpatchAll(harmony.Id);
            //harmony.UnpatchAll(); // 정대 사용 금지. 다름 플러그인이 패치한것까지 다 풀려버림

            //myWindowRect.save();
        }





    }
}
