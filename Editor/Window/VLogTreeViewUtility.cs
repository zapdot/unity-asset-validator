﻿/*
unity-asset-validator Copyright (C) 2017  Jeff Campbell

unity-asset-validator is licensed under a
Creative Commons Attribution-NonCommercial 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-nc/4.0/>.
*/
#if UNITY_5_6_OR_NEWER
using JCMG.AssetValidator.Editor.Utility;
using JCMG.AssetValidator.Editor.Validators.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace JCMG.AssetValidator.Editor.Window
{
    public static class VLogTreeViewUtility
    {
        public static TreeViewItem CreateTreeByGroupType(VLogTreeGroupByMode mode, IList<VLog> logs)
        {
            switch (mode)
            {
                case VLogTreeGroupByMode.CollapseByValidatorType:
                    return CreateTreeGroupedByValidatorType(logs);
                case VLogTreeGroupByMode.CollapseByArea:
                    return CreateTreeGroupedByArea(logs);
                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }
        }

        public static TreeViewItem CreateTreeGroupedByValidatorType(IList<VLog> logs)
        {
            var id = -1;
            var rootItem = new TreeViewItem(++id, -1);

            // Create a lookup of all vLogs grouped by validator name
            var dict = new Dictionary<string, List<VLog>>();
            foreach (var vLog in logs)
            {
                if (!dict.ContainsKey(vLog.validatorName))
                    dict[vLog.validatorName] = new List<VLog> { vLog };
                else
                    dict[vLog.validatorName].Add(vLog);
            }

            // From the lookup and root item, create a child object for each validator
            // type and for each validator type add all logs of that type as children.
            foreach (var kvp in dict)
            {
                var header = new VLogTreeViewHeader(++id, 0, kvp.Key);
                var kLogs = kvp.Value;
                foreach (var kLog in kLogs)
                    header.AddChild(new VLogTreeViewItem(kLog, ++id, 1));

                SetLogCounts(header, kLogs);
                
                rootItem.AddChild(header);
            }

            return rootItem;
        }

        private static void SetLogCounts(VLogTreeViewHeader header, IEnumerable<VLog> logs)
        {
            var errorCount = logs.Count(x => x.vLogType == VLogType.Error);
            var warnCount = logs.Count(x => x.vLogType == VLogType.Warning);
            var infoCount = logs.Count(x => x.vLogType == VLogType.Info);

            header.SetLogCounts(errorCount, warnCount, infoCount);
        }

        public static TreeViewItem CreateTreeGroupedByArea(IList<VLog> logs)
        {
            var id = -1;
            var rootItem = new TreeViewItem(++id, -1);
            var sceneLogs = logs.Where(x => x.source == VLogSource.Scene);
            var rootSceneItem = new VLogTreeViewHeader(++id, 0, "Scene");
            SetLogCounts(rootSceneItem, sceneLogs);

            var projLogs = logs.Where(x => x.source == VLogSource.Project);
            var projRootItem = new VLogTreeViewHeader(++id, 0, "Project");
            SetLogCounts(projRootItem, projLogs);

            var miscLogs = logs.Where(x => x.source == VLogSource.None);
            var miscRoot = new VLogTreeViewHeader(++id, 0, "Misc");
            SetLogCounts(miscRoot, miscLogs);

            // Create a lookup of all vLogs grouped by validator name
            var dict = new Dictionary<VLogSource, List<VLog>>();
            foreach (var vLog in logs)
            {
                if (!dict.ContainsKey(vLog.source))
                    dict[vLog.source] = new List<VLog> { vLog };
                else
                    dict[vLog.source].Add(vLog);
            }

            // From the lookup and root item, create a child object for each validator
            // type and for each validator type add all logs of that type as children.
            foreach (var kvp in dict)
            {
                var kLogs = kvp.Value;

                switch (kvp.Key)
                {
                    case VLogSource.None:
                        foreach (var kLog in kLogs)
                            miscRoot.AddChild(new VLogTreeViewItem(kLog, ++id, 1, kLog.validatorName));
                        break;
                    case VLogSource.Scene:
                        var sceneDict = new Dictionary<string, IList<VLog>>();
                        kLogs.Sort(new SortVLogOnScenePath());
                        foreach (var kLog in kLogs)
                        {
                            if (sceneDict.ContainsKey(kLog.scenePath))
                                sceneDict[kLog.scenePath].Add(kLog);
                            else
                                sceneDict[kLog.scenePath] = new List<VLog> {kLog};
                        }

                        foreach (var skvp in sceneDict)
                        {
                            var slogs = skvp.Value;
                            var sceneRootItem = new VLogTreeViewHeader(++id, 1, skvp.Key.Split('/').Last());
                            foreach (var slog in slogs)
                                sceneRootItem.AddChild(new VLogTreeViewItem(slog, ++id, 2, slog.objectPath.Split('/').Last()));

                            SetLogCounts(sceneRootItem, slogs);

                            rootSceneItem.AddChild(sceneRootItem);
                        }
                        
                        break;
                    case VLogSource.Project:
                        kLogs.Sort(new SortVLogOnObjectPath());
                        foreach (var kLog in kLogs)
                            projRootItem.AddChild(new VLogTreeViewItem(kLog, ++id, 1, kLog.objectPath.Split('/').Last()));

                        break;
                }
            }

            rootItem.AddChild(projRootItem);
            rootItem.AddChild(rootSceneItem);
            rootItem.AddChild(miscRoot);

            return rootItem;
        }

        private class SortVLogOnScenePath : IComparer<VLog>
        {
            public int Compare(VLog x, VLog y)
            {
                return x.scenePath.CompareTo(y.scenePath);
            }
        }

        private class SortVLogOnObjectPath : IComparer<VLog>
        {
            public int Compare(VLog x, VLog y)
            {
                return x.objectPath.CompareTo(y.objectPath);
            }
        }
    }
}
#endif