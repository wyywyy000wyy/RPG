using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    [Serializable]
    public class AnalyzeRuleGUI
    {
        [SerializeField]
        private TreeViewState m_TreeState;

        private AssetSettingsAnalyzeTreeView m_Tree;

        private const float k_ButtonHeight = 24f;
        internal void OnGUI(Rect rect)
        {
            if (m_Tree == null)
            {
                if (m_TreeState == null)
                    m_TreeState = new TreeViewState();

                m_Tree = new AssetSettingsAnalyzeTreeView(m_TreeState);
                m_Tree.Reload();
            }

            var treeRect = new Rect(rect.xMin, rect.yMin + k_ButtonHeight, rect.width, rect.height - k_ButtonHeight);
            m_Tree.OnGUI(treeRect);

            var buttonRect = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);

            GUILayout.BeginArea(buttonRect);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!m_Tree.SelectionContainsRuleContainer);
            if (GUILayout.Button("Analyze Selected Rules"))
            {
                EditorApplication.delayCall += () => m_Tree.RunAllSelectedRules();
            }

            if (GUILayout.Button("Clear Selected Rules"))
            {
                EditorApplication.delayCall += () => m_Tree.ClearAllSelectedRules();
            }

            EditorGUI.BeginDisabledGroup(!m_Tree.SelectionContainsFixableRule || !m_Tree.SelectionContainsErrors);
            if (GUILayout.Button("Fix Selected Rules"))
            {
                EditorApplication.delayCall += () => m_Tree.FixAllSelectedRules();
            }
            EditorGUI.EndDisabledGroup();
            //             if (GUILayout.Button("OutPut"))
            //             {
            //                 EditorApplication.delayCall += () => OutPutText();
            //             }
            if (GUILayout.Button("OutPutDup"))
            {
                EditorApplication.delayCall += () => OutPutBundleDuplicate();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            //TODO
            //if (GUILayout.Button("Revert Selected"))
            //{
            //    m_Tree.RevertAllActiveRules();
            //}
        }
        public static Func<List<string>, Dictionary<string, Dictionary<string, List<string>>>> GetUsedBy;
        private void OutPutBundleDuplicate()
        {
            StringBuilder writeText = new StringBuilder();
            var roleRules = m_Tree.GetRows();

            Dictionary<string, AnalyzeAsset> duplicateDic = new Dictionary<string, AnalyzeAsset>();
            foreach (var item in roleRules)
            {
                if (item.displayName.Contains("Analyze"))
                {
                    TreeViewItem bundleDupItem = GetViewItemIterator("Check Duplicate Bundle Dependencies", item);
                    foreach (var childItem in bundleDupItem.children)
                    {
                        foreach (var child2Item in childItem.children)
                        {
                            foreach (var child3Item in child2Item.children)
                            {
                                if (duplicateDic.TryGetValue(child3Item.displayName, out AnalyzeAsset outAsset) == false)
                                {
                                    outAsset = new AnalyzeAsset();
                                    FileInfo fileInfo = new FileInfo(Path.GetFullPath(child3Item.displayName));
                                    outAsset.AssetPath = child3Item.displayName;
                                    outAsset.AssetSize = fileInfo.Length;
                                    duplicateDic.Add(child3Item.displayName, outAsset);
                                }
                                outAsset.BelongBundles.Add(child3Item.parent.displayName.Substring(0, child3Item.parent.displayName.LastIndexOf("(")));
                            }
                        }
                    }
                }
            }


            List<AnalyzeAsset> allAssetdup = new List<AnalyzeAsset>(duplicateDic.Values);
            allAssetdup.Sort(DupAssetSort);
            Dictionary<string, Dictionary<string, List<string>>> getUsedBys = null;
            if (GetUsedBy != null)
            {
                getUsedBys = GetUsedBy(allAssetdup.Select(a => a.AssetPath).ToList());
            }
            writeText.AppendLine("资源路径\t大小\t类型\t所属bundle1\t所属bundle2");
            for (int i = 0; i < allAssetdup.Count; i++)
            {
                string curText = "";
                AnalyzeAsset curAsset = allAssetdup[i];
                //curAsset.SortBelongBundles();
                curText += curAsset.AssetPath + "\t";
                curText += (curAsset.AssetSize / 1024.00).ToString("F2") + "KB\t";
                curText += curAsset.AssetType.ToString() + "\t";
                foreach (var item in curAsset.BelongBundles)
                {
                    curText += item + "\t";
                }
                if (getUsedBys != null)
                {
                    Dictionary<string, List<string>> curGroupDic = getUsedBys[curAsset.AssetPath];
                    Dictionary<string, List<string>>.Enumerator groupEnme = curGroupDic.GetEnumerator();
                    using (groupEnme)
                    {
                        while (groupEnme.MoveNext())
                        {
                            string curGroupName = groupEnme.Current.Key + " : ";
                            foreach (var item in groupEnme.Current.Value)
                            {
                                curGroupName += item + " | ";
                            }
                            curText += curGroupName + "\t";
                        }
                    }
                }
                writeText.AppendLine(curText);
            }

            File.WriteAllText("C:/Users/Administrator/Desktop/AnalyzeyBundleDuplicate.txt", writeText.ToString());
        }
        private void GetUsedMe(string GUID)
        {
            //             AssetDatabase.LoadAssetAtPath<FR2_Cache>("Assets/FR2_Cache.asset");
            //             var list = FR2_Cache.Api.FindAssets(FR2_Unity.Selection_AssetGUIDs, false);
            //             var dict = new Dictionary<string, UnityEngine.Object>();
            // 
            //             if (includeMe) AddToDict(dict, list.ToArray());
            // 
            //             for (var i = 0; i < list.Count; i++)
            //             {
            //                 AddToDict(dict, list[i].UsedByMap.Values.ToArray());
            //             }
            // 
            //             Selection.objects = dict.Values.ToArray();
        }
        private int DupAssetSort(AnalyzeAsset left, AnalyzeAsset right)
        {
            AssetType leftType = left.AssetType;
            AssetType rightType = right.AssetType;
            if (leftType != rightType)
            {
                return leftType - rightType;
            }
            else
            {
                string leftDir = left.AssetPath.Substring(0, left.AssetPath.LastIndexOf("/"));
                string rightDir = right.AssetPath.Substring(0, right.AssetPath.LastIndexOf("/"));
                if (leftDir != rightDir)
                {
                    return leftDir.GetHashCode() - rightDir.GetHashCode();
                }
                else
                {
                    return Path.GetFileName(left.AssetPath).GetHashCode() - Path.GetFileName(right.AssetPath).GetHashCode();
                }
            }
        }
        public int SameStringCountCompare(string s1, string s2)
        {
            int count = 0;
            int n = s1.Length > s2.Length ? s2.Length : s1.Length;
            for (int i = 0; i < n; i++)
            {
                if (s1.Substring(i, 1) == s2.Substring(i, 1))
                {
                    count++;
                }
            }
            return count;
        }
        private TreeViewItem GetViewItemIterator(string containName, TreeViewItem viewItem)
        {
            if (viewItem.hasChildren)
            {
                foreach (var item in viewItem.children)
                {
                    if (item.displayName.Contains(containName))
                    {
                        return item;
                    }
                    else
                    {
                        TreeViewItem curItem = GetViewItemIterator(containName, item);
                        if (curItem != null) return curItem;
                    }
                }
            }
            return null;
        }
        private void OutPutText()
        {
            StringBuilder writeText = new StringBuilder();
            var roleRules = m_Tree.GetRows();
            foreach (var item in roleRules)
            {
                if (item.displayName.Contains("Analyze"))
                {
                    string curSpace = "";
                    writeText.AppendLine(item.displayName);
                    IteratorChildren(writeText, curSpace, item);
                }
            }
            File.WriteAllText("C:/Users/Administrator/Desktop/AnalyzeyText.txt", writeText.ToString());
        }
        private void IteratorChildren(StringBuilder writeText, string curSpace, TreeViewItem viewItem)
        {
            if (viewItem.hasChildren)
            {
                string curAddSpace = curSpace + "  ";
                foreach (var childItem in viewItem.children)
                {
                    writeText.AppendLine(curAddSpace + childItem.displayName);
                    IteratorChildren(writeText, curAddSpace, childItem);
                }
            }
        }
        private class AnalyzeAsset
        {
            public string AssetPath;
            public long AssetSize;
            public List<string> BelongBundles = new List<string>();
            public void SortBelongBundles()
            {
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                List<AddressableAssetGroup> groups = settings.groups;
                string belongGroup = "";
                foreach (var item in groups)
                {
                    var assetEntry = item.GetAssetEntry(AssetDatabase.AssetPathToGUID(AssetPath));
                    if (assetEntry != null)
                    {
                        belongGroup = item.Name;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(belongGroup))
                {
                    Debug.LogError("Not Find Belong Group ,AssetPath is:" + AssetPath);
                }
                string insertBundle = "";
                for (int i = BelongBundles.Count - 1; i >= 0; i--)
                {
                    if (BelongBundles[i].Contains(belongGroup))
                    {
                        insertBundle = BelongBundles[i];
                        BelongBundles.RemoveAt(i);
                        break;
                    }
                }
                if (string.IsNullOrEmpty(insertBundle))
                {
                    Debug.LogError("Not Find insertBundle , AssetPath is:" + AssetPath);
                }
                BelongBundles.Insert(0, insertBundle);
            }
            public AssetType AssetType
            {
                get
                {
                    if (_AssetType == AssetType.None)
                    {
                        SetAssetType();
                    }
                    return _AssetType;
                }
            }
            private AssetType _AssetType = AssetType.None;
            private void SetAssetType()
            {
                string assetExtension = Path.GetExtension(AssetPath);
                if (assetExtension.Equals(".anim"))
                {
                    _AssetType = AssetType.Animation;
                }
                else if (assetExtension.Equals(".FBX") || assetExtension.Equals(".fbx"))
                {
                    _AssetType = AssetType.FBX;
                }
                else if (assetExtension.Equals(".controller"))
                {
                    _AssetType = AssetType.AnimaController;
                }
                else if (assetExtension.Equals(".hdr"))
                {
                    _AssetType = AssetType.Hdr;
                }
                else if (assetExtension.Equals(".png"))
                {
                    _AssetType = AssetType.Png;
                }
                else if (assetExtension.Equals(".mat"))
                {
                    _AssetType = AssetType.Mat;
                }
                else if (assetExtension.Equals(".tga"))
                {
                    _AssetType = AssetType.Tga;
                }
                else if (assetExtension.Equals(".shader"))
                {
                    _AssetType = AssetType.Shader;
                }
                else if (assetExtension.Equals(".signal"))
                {
                    _AssetType = AssetType.Signal;
                }
                else if (assetExtension.Equals(".jpg"))
                {
                    _AssetType = AssetType.Jpg;
                }
                else if (assetExtension.Equals(".obj"))
                {
                    _AssetType = AssetType.Obj;
                }
                else if (assetExtension.Equals(".tif"))
                {
                    _AssetType = AssetType.Tif;
                }
                else if (assetExtension.Equals(".playable"))
                {
                    _AssetType = AssetType.Playable;
                }
                else if (IsSpine(assetExtension))
                {
                    _AssetType = AssetType.Spine;
                }
                else if (assetExtension.Equals(".asset"))
                {
                    _AssetType = AssetType.Playable;
                }
                else if (assetExtension.Equals(".ttf"))
                {
                    _AssetType = AssetType.TTF;
                }
                else if (assetExtension.Equals(".texture2darray"))
                {
                    _AssetType = AssetType.Textture2Arrary;
                }
            }
            private bool IsSpine(string extension)
            {
                if (extension.Equals(".json") && File.ReadAllText(Path.GetFullPath(AssetPath)).Contains("\"skeleton\":"))
                {
                    return true;
                }
                else if (AssetPath.Contains(".atlas.txt"))
                {
                    return true;
                }
                else if (AssetPath.Contains("_Atlas.asset"))
                {
                    return true;
                }
                else if (AssetPath.Contains("skel.bytes"))
                {
                    return true;
                }
                else if (AssetPath.Contains("_SkeletonData.asset"))
                {
                    return true;
                }
                return false;
            }
        }

        private enum AssetType
        {
            None,
            Spine,
            Hdr,
            Png,
            Jpg,
            Tga,
            Tif,
            Animation,
            AnimaController,
            Shader,
            Mat,
            Obj,
            Signal,
            Playable,
            Asset,
            FBX,
            TTF,
            Textture2Arrary,
        }
    }
}
