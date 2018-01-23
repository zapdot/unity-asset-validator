﻿/*
unity-asset-validator Copyright (C) 2017  Jeff Campbell

unity-asset-validator is licensed under a
Creative Commons Attribution-NonCommercial 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc/4.0/>.
*/
using JCMG.AssetValidator.Editor.Utility;
using JCMG.AssetValidator.Editor.Validators.Output;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JCMG.AssetValidator.Editor
{
    public static class AssetValidatorMenuItems
    {
        private const string DEBUGGING_MENU_ITEM = "Tools/AssetValidator/Toggle Debugging";

        static AssetValidatorMenuItems()
        {
            AssetValidatorUtility.IsDebugging = EditorPrefs.GetBool(DEBUGGING_MENU_ITEM, false);

            // Delaying until first editor tick so that the menu
            // will be populated before setting check state, and
            // re-apply correct action
            EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            PerformAction(ref AssetValidatorUtility.IsDebugging);
        }

        public static void PerformAction(ref bool enabled)
        {
            // Set checkmark on menu item
            Menu.SetChecked(DEBUGGING_MENU_ITEM, enabled);

            // Saving editor state
            EditorPrefs.SetBool(DEBUGGING_MENU_ITEM, enabled);

            AssetValidatorUtility.IsDebugging = enabled;
        }

        [MenuItem(DEBUGGING_MENU_ITEM)]
        public static void ToggleAssetValidiatorDebuggingOn()
        {
            AssetValidatorUtility.IsDebugging = !AssetValidatorUtility.IsDebugging;
            PerformAction(ref AssetValidatorUtility.IsDebugging);
        }

        [MenuItem("Tools/AssetValidator/Validate Project Assets", priority = 2)]
        public static void ValidateAllAssetsInAssetFolder()
        {
            var logger = GetLogger();

            AssetValidatorUtility.ValidateAllAssetsInAssetsFolder(logger);

            PrintLogsToConsole(logger.GetLogs());
        }

        [MenuItem("Tools/AssetValidator/Validate Active Scene", priority = 2)]
        public static void ValidateAllAssetsInActiveScene()
        {
            var logger = GetLogger();

            AssetValidatorUtility.ValidateAllAssetsInActiveScene(logger);

            PrintLogsToConsole(logger.GetLogs());
        }

        [MenuItem("Tools/AssetValidator/Validate All Scenes", priority = 2)]
        public static void ValidateAllAssetsInAllScenes()
        {
            var logger = GetLogger();

            AssetValidatorUtility.ValidateAllAssetsInAllScenes(logger);

            PrintLogsToConsole(logger.GetLogs());
        }

        [MenuItem("Tools/AssetValidator/Validate All Scenes in Build Settings", priority = 2)]
        public static void ValidateAllAssetsInAllScenesInBuildSettings()
        {
            var logger = GetLogger();

            AssetValidatorUtility.ValidateAllAssetsInAllScenesInBuildSettings(logger);

            PrintLogsToConsole(logger.GetLogs());
        }

        [MenuItem("Tools/AssetValidator/Validate All Scenes in Build Settings and Asset Bundles", priority = 2)]
        public static void ValidateAllAssetsInAllScenesInBuildSettingsAndAssetBundles()
        {
            var logger = GetLogger();

            AssetValidatorUtility.ValidateAllAssetsInAllScenesInBuildSettingsAndAssetBundles(logger);

            PrintLogsToConsole(logger.GetLogs());
        }
        
        private static AssetValidatorLogger GetLogger()
        {
            return new AssetValidatorLogger();
        }

        private static void PrintLogsToConsole(IList<VLog> logs)
        {
            for (var i = 0; i < logs.Count; i++)
            {
                switch (logs[i].vLogType)
                {
                    case VLogType.Info:
                        Debug.Log(logs[i].message);
                        break;
                    case VLogType.Warning:
                        Debug.LogWarning(logs[i].message);
                        break;
                    case VLogType.Error:
                        Debug.LogError(logs[i].message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}