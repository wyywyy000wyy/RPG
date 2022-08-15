using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Build.AnalyzeRules
{
    class CheckBundleDupeDependencies : BundleRuleBase
    {
        internal struct CheckDupeResult
        {
            public AddressableAssetGroup Group;
            public string DuplicatedFile;
            public string AssetPath;
            public GUID DuplicatedGroupGuid;
        }

        internal struct ExtraCheckBundleDupeData
        {
            public bool ResultsInverted;
        }

        public override bool CanFix
        {
            get { return true; }
        }

        public override string ruleName
        { get { return "Check Duplicate Bundle Dependencies"; } }

        [NonSerialized]
        internal readonly Dictionary<string, Dictionary<string, List<string>>> m_AllIssues = new Dictionary<string, Dictionary<string, List<string>>>();
        [SerializeField]
        internal HashSet<GUID> m_ImplicitAssets;

        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            ClearAnalysis();
            return CheckForDuplicateDependencies(settings);
        }

        void RefreshDisplay()
        {
            var savedData = AnalyzeSystem.GetDataForRule<ExtraCheckBundleDupeData>(this);
            if (!savedData.ResultsInverted)
            {
                m_Results = (from issueGroup in m_AllIssues
                             from bundle in issueGroup.Value
                             from item in bundle.Value
                             select new AnalyzeResult
                             {
                                 resultName = ruleName + kDelimiter +
                                              issueGroup.Key + kDelimiter +
                                              ConvertBundleName(bundle.Key, issueGroup.Key) + kDelimiter +
                                              item,
                                 severity = MessageType.Warning
                             }).ToList();
            }
            else
            {
                m_Results = (from issueGroup in m_AllIssues
                             from bundle in issueGroup.Value
                             from item in bundle.Value
                             select new AnalyzeResult
                             {
                                 resultName = ruleName + kDelimiter +
                                              item + kDelimiter +
                                              ConvertBundleName(bundle.Key, issueGroup.Key) + kDelimiter +
                                              issueGroup.Key,
                                 severity = MessageType.Warning
                             }).ToList();
            }
            if (m_Results.Count == 0)
                m_Results.Add(noErrors);
        }

        internal override IList<CustomContextMenu> GetCustomContextMenuItems()
        {
            IList<CustomContextMenu> customItems = new List<CustomContextMenu>();
            customItems.Add(new CustomContextMenu("Organize by Asset",
                () => InvertDisplay(),
                AnalyzeSystem.AnalyzeData.Data[ruleName].Any(),
                AnalyzeSystem.GetDataForRule<ExtraCheckBundleDupeData>(this).ResultsInverted));

            customItems.Add(new CustomContextMenu("Fix Rules mark address",
            () => MarkDupAddress(),
            true, false));

            return customItems;
        }

        //标记重复打包资源
        //规则 png >256的单独标记， <256的以父文件夹打包。
        // mat不标记
        // fbx以父文件夹打包。
        void MarkDupAddress()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            var ResultsInverted = AnalyzeSystem.GetDataForRule<ExtraCheckBundleDupeData>(this).ResultsInverted;
            List<string> singleList = new List<string>();
            List<string> groupList = new List<string>();
            Dictionary<string, int> assetRef = new Dictionary<string, int>();

            string assetPath = string.Empty;

            int refCount = 0;
            string[] results = null;
            var analyzeData = AnalyzeSystem.AnalyzeData.Data[ruleName];
            var total = analyzeData.Count;
            var i = 0;
            foreach (var result in analyzeData)
            {
                i++;
                if (ResultsInverted)
                {
                    results = result.resultName.Split(kDelimiter);
                }
                else
                {
                    results = ReverseStringFromIndex(result.resultName, 1, kDelimiter).Split(kDelimiter);
                }

                assetPath = results[1];
                var refI = 1;
                if (assetRef.TryGetValue(assetPath, out refI))
                {
                    assetRef[assetPath] = ++refI;
                }
                else
                {
                    assetRef.Add(assetPath,1);
                }

                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar($"analyzeData bundle dependencies {i}/{total}", $"refCount:{refI},{assetPath}", (float)i / (float)total))
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                    return;
                }

            }


            UnityEditor.EditorUtility.DisplayCancelableProgressBar($"dup mark group  ", $"{assetRef.Count}", 1);

            foreach (var kv in assetRef)
            {
                assetPath = kv.Key;
                refCount = kv.Value;
                var extension = System.IO.Path.GetExtension(assetPath).ToLower();
                Debug.Log($"refCount={refCount},assetpath={assetPath}");
                if (extension.Equals(".png") || extension.Equals(".tag") || extension.Equals(".hdr"))
                {
                    var import = new UnityEditor.TextureImporter();
                    var texture = (Texture)AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

                    var size = texture.width * texture.height;
                    if (size > 256 * 256)
                    {
                        //single
                        singleList.Add(assetPath);
                    }
                    else if (refCount >= 3)
                    {
                        groupList.Add(assetPath);
                    }

                }
                else if (extension.Equals(".fbx") && refCount >= 3)
                {
                    groupList.Add(assetPath);
                }
                else if (extension.Equals(".mat") && refCount >= 4)
                {
                    groupList.Add(assetPath);
                }
            }

            DupSingleMark(singleList);
            DupGroupMark(groupList);
            UnityEditor.EditorUtility.ClearProgressBar();
            Debug.Log($"Fix Rules mark address singleList:{singleList.Count},groupList:{groupList.Count}");
        }

        void DupGroupMark(List<string> assetPaths)
        {
            var setting = LoadAASSetting();
            int i = 0;
            var total = assetPaths.Count;

            foreach (var str in assetPaths)
            {
                i++;
                var guid = AssetDatabase.AssetPathToGUID(str); //获得GUID
                var group = FindGroup(GetGroupName(str));
                UnityEditor.EditorUtility.DisplayCancelableProgressBar($"dup group mark {i}/{total}", $"{str}", (float)i / (float)total);
                var entry = CreateAssetEntry(setting, guid, group); //已经标记过的不需要再次标记
                if (entry != null)
                    entry.SetAddress(GetEntryName(str, guid));
            }
        }

        void DupSingleMark(List<string> assetPaths)
        {
            var dupSingleGroup = FindGroup("dup_s_");
            var setting = LoadAASSetting();
            int i = 0;
            var total = assetPaths.Count;

            foreach (var str in assetPaths)
            {
                i++;
                var guid = AssetDatabase.AssetPathToGUID(str); //获得GUID

                UnityEditor.EditorUtility.DisplayCancelableProgressBar($"single group mark {i}/{total}", $"{str}", (float)i / (float)total);

                var entry = CreateAssetEntry(setting, guid, dupSingleGroup); //已经标记过的不需要再次标记
                if (entry != null)
                    entry.SetAddress(GetEntryName(str, guid));
            }
        }

        AddressableAssetEntry CreateAssetEntry(AddressableAssetSettings setting, string guid, AddressableAssetGroup targetParent)
        {
            var entry = setting.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = setting.CreateOrMoveEntry(guid, targetParent);
            }

            return entry;
        }

        //获取组名
        public static string GetGroupName(string path)
        {
            //AssetDatabase.ass
            var dicName = System.IO.Path.GetDirectoryName(path);
            dicName = "dup_g" + UnityEngine.Animator.StringToHash(dicName);
            return dicName;
        }

        /// <summary>
        /// 获取entry name
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetEntryName(string path, string guid)
        {
            if (path.EndsWith(".prefab") || path.EndsWith(".png") || path.EndsWith(".tag") || path.EndsWith(".hdr"))
            {
                return System.IO.Path.GetFileNameWithoutExtension(path);
            }
            else
            {
                return guid;
            }
        }


        /// <summary>
        /// 创建寻找指导名字的group 
        /// 当autoCreate为true时候不存在就创建
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="Create"></param>
        /// <returns></returns>
        public static AddressableAssetGroup FindGroup(string groupName, AddressableAssetGroupTemplate groupTemplate = null)
        {
            var setting = LoadAASSetting();
            var group = setting.FindGroup(groupName);
            // Debug.Log($"{groupName} {group} {groupTemplate}");
            if (group == null)
            {
                if (groupTemplate == null)
                {
                    var groupTempObjs = setting.GroupTemplateObjects;
                    foreach (var temp in groupTempObjs)
                    {
                        if (temp is AddressableAssetGroupTemplate)
                        {
                            groupTemplate = (AddressableAssetGroupTemplate)temp;
                            break;
                        }
                    }
                }
                group = setting.CreateGroup(groupName, false, false, true, null, groupTemplate.GetTypes());
                groupTemplate.ApplyToAddressableAssetGroup(group);
            }
            return group;
        }

        public static AddressableAssetSettings LoadAASSetting()
        {
            var setting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
            return setting;
        }

        void InvertDisplay()
        {
            List<AnalyzeResult> updatedResults = new List<AnalyzeResult>();

            foreach (var result in AnalyzeSystem.AnalyzeData.Data[ruleName])
            {
                updatedResults.Add(new AnalyzeResult()
                {
                    //start at index 1 because the first result is going to be the rule name which we want to remain where it is.
                    resultName = ReverseStringFromIndex(result.resultName, 1, kDelimiter),
                    severity = result.severity
                });
            }

            AnalyzeSystem.ReplaceAnalyzeData(this, updatedResults);
            var savedData = AnalyzeSystem.GetDataForRule<ExtraCheckBundleDupeData>(this);
            savedData.ResultsInverted = !savedData.ResultsInverted;
            AnalyzeSystem.SaveDataForRule(this, savedData);
            AnalyzeSystem.SerializeData();
            AnalyzeSystem.ReloadUI();
        }

        private string ReverseStringFromIndex(string data, int startingIndex, char delimiter)
        {
            string[] splitData = data.Split(delimiter);
            int i = startingIndex;
            int k = splitData.Length - 1;
            while (i < k)
            {
                string temp = splitData[i];
                splitData[i] = splitData[k];
                splitData[k] = temp;
                i++;
                k--;
            }

            return String.Join(kDelimiter.ToString(), splitData);
        }

        List<AnalyzeResult> CheckForDuplicateDependencies(AddressableAssetSettings settings)
        {
            if (!BuildUtility.CheckModifiedScenesAndAskToSave())
            {
                Debug.LogError("Cannot run Analyze with unsaved scenes");
                m_Results.Add(new AnalyzeResult { resultName = ruleName + "Cannot run Analyze with unsaved scenes" });
                return m_Results;
            }

            CalculateInputDefinitions(settings);

            if (m_AllBundleInputDefs.Count > 0)
            {
                var context = GetBuildContext(settings);
                ReturnCode exitCode = RefreshBuild(context);
                if (exitCode < ReturnCode.Success)
                {
                    Debug.LogError("Analyze build failed. " + exitCode);
                    m_Results.Add(new AnalyzeResult { resultName = ruleName + "Analyze build failed. " + exitCode });
                    return m_Results;
                }

                var implicitGuids = GetImplicitGuidToFilesMap();
                var checkDupeResults = CalculateDuplicates(implicitGuids, context);
                BuildImplicitDuplicatedAssetsSet(checkDupeResults);
            }

            RefreshDisplay();
            return m_Results;
        }

        internal IEnumerable<CheckDupeResult> CalculateDuplicates(Dictionary<GUID, List<string>> implicitGuids, AddressableAssetsBuildContext aaContext)
        {
            //Get all guids that have more than one bundle referencing them
            IEnumerable<KeyValuePair<GUID, List<string>>> validGuids =
                from dupeGuid in implicitGuids
                where dupeGuid.Value.Distinct().Count() > 1
                where IsValidPath(AssetDatabase.GUIDToAssetPath(dupeGuid.Key.ToString()))
                select dupeGuid;

            return
                from guidToFile in validGuids
                from file in guidToFile.Value

                    //Get the files that belong to those guids
                let fileToBundle = m_ExtractData.WriteData.FileToBundle[file]

                //Get the bundles that belong to those files
                let bundleToGroup = aaContext.bundleToAssetGroup[fileToBundle]

                //Get the asset groups that belong to those bundles
                let selectedGroup = aaContext.Settings.FindGroup(findGroup => findGroup != null && findGroup.Guid == bundleToGroup)

                select new CheckDupeResult
                {
                    Group = selectedGroup,
                    DuplicatedFile = file,
                    AssetPath = AssetDatabase.GUIDToAssetPath(guidToFile.Key.ToString()),
                    DuplicatedGroupGuid = guidToFile.Key
                };
        }

        internal void BuildImplicitDuplicatedAssetsSet(IEnumerable<CheckDupeResult> checkDupeResults)
        {
            m_ImplicitAssets = new HashSet<GUID>();

            foreach (var checkDupeResult in checkDupeResults)
            {
                Dictionary<string, List<string>> groupData;
                if (!m_AllIssues.TryGetValue(checkDupeResult.Group.Name, out groupData))
                {
                    groupData = new Dictionary<string, List<string>>();
                    m_AllIssues.Add(checkDupeResult.Group.Name, groupData);
                }

                List<string> assets;
                if (!groupData.TryGetValue(m_ExtractData.WriteData.FileToBundle[checkDupeResult.DuplicatedFile], out assets))
                {
                    assets = new List<string>();
                    groupData.Add(m_ExtractData.WriteData.FileToBundle[checkDupeResult.DuplicatedFile], assets);
                }

                assets.Add(checkDupeResult.AssetPath);

                m_ImplicitAssets.Add(checkDupeResult.DuplicatedGroupGuid);
            }
        }

        public override void FixIssues(AddressableAssetSettings settings)
        {
            if (m_ImplicitAssets == null)
                CheckForDuplicateDependencies(settings);

            if (m_ImplicitAssets.Count == 0)
                return;

            var group = settings.CreateGroup("Duplicate Asset Isolation", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;

            foreach (var asset in m_ImplicitAssets)
                settings.CreateOrMoveEntry(asset.ToString(), group, false, false);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }
        public List<string> FixIssues2(AddressableAssetSettings settings)
        {
            CheckForDuplicateDependencies(settings);
            if (m_ImplicitAssets.Count == 0)
                return new List<string>();
            List<string> impliAssetPath = new List<string>(m_ImplicitAssets.Count);
            foreach (var item in m_ImplicitAssets)
            {
                impliAssetPath.Add(AssetDatabase.GUIDToAssetPath(item.ToString()));
            }
            return impliAssetPath;
        }
        public void GetNeedFixImplicitAssets(AddressableAssetSettings settings)
        {
            CheckForDuplicateDependencies(settings);
            if (m_ImplicitAssets.Count == 0)
            {
                Debug.LogError("Congratulation Duplication Not Exist");
                return;
            }



            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }

        public override void ClearAnalysis()
        {
            m_AllIssues.Clear();
            m_ImplicitAssets = null;
            base.ClearAnalysis();
        }
    }


    [InitializeOnLoad]
    class RegisterCheckBundleDupeDependencies
    {
        static RegisterCheckBundleDupeDependencies()
        {
            AnalyzeSystem.RegisterNewRule<CheckBundleDupeDependencies>();
        }
    }
}
