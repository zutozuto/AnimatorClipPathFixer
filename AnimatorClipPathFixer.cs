#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 修复 Animator Controller 中动画片段因层级变化导致的路径丢失。
/// </summary>
public class AnimatorClipPathFixer : EditorWindow
{
    private AnimatorController _controller;
    private GameObject _referenceRoot;
    private string _manualPrefix = "";
    private string _syncStatus = "";
    private MessageType _syncStatusType = MessageType.Info;
    private Vector2 _scroll;
    private readonly List<ClipReport> _reports = new();

    private struct ClipReport
    {
        public AnimationClip Clip;
        public int TotalBindings;
        public int BrokenBindings;
        public int FixedBindings;
        public List<string> Messages;
    }

    [MenuItem("Tools/Animation/AnimatorClipPathFixer")]
    public static void OpenWindow()
    {
        var window = GetWindow<AnimatorClipPathFixer>();
        window.titleContent = new GUIContent(AnimatorClipPathFixerLoc.WindowTitle);
        window.SyncFromSelection();
        window.minSize = new Vector2(480, 360);
    }

    private void OnGUI()
    {
        DrawLanguageSelector();

        EditorGUILayout.LabelField(AnimatorClipPathFixerLoc.Title, EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(AnimatorClipPathFixerLoc.HelpBox, MessageType.Info);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            _controller = EditorGUILayout.ObjectField(
                AnimatorClipPathFixerLoc.FieldAnimatorController,
                _controller,
                typeof(AnimatorController),
                false) as AnimatorController;
            _referenceRoot = EditorGUILayout.ObjectField(
                new GUIContent(
                    AnimatorClipPathFixerLoc.FieldReferenceRoot,
                    AnimatorClipPathFixerLoc.FieldReferenceRootTip),
                _referenceRoot,
                typeof(GameObject),
                true) as GameObject;
            _manualPrefix = EditorGUILayout.TextField(
                new GUIContent(
                    AnimatorClipPathFixerLoc.FieldManualPrefix,
                    AnimatorClipPathFixerLoc.FieldManualPrefixTip),
                _manualPrefix);
        }

        if (!string.IsNullOrEmpty(_syncStatus))
            EditorGUILayout.HelpBox(_syncStatus, _syncStatusType);

        EditorGUILayout.Space(4);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(AnimatorClipPathFixerLoc.BtnDetectFromSelection))
                SyncFromSelection();

            GUI.enabled = _controller != null && _referenceRoot != null;
            if (GUILayout.Button(AnimatorClipPathFixerLoc.BtnAnalyze))
                Analyze();
            if (GUILayout.Button(AnimatorClipPathFixerLoc.BtnApplyFix))
                ApplyFix();
            GUI.enabled = true;
        }

        DrawReports();
    }

    private void DrawLanguageSelector()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(AnimatorClipPathFixerLoc.LabelLanguage, GUILayout.Width(36));
            var index = (int)AnimatorClipPathFixerLoc.Current;
            var newIndex = EditorGUILayout.Popup(index, AnimatorClipPathFixerLoc.LanguageOptions, GUILayout.Width(100));
            if (newIndex != index)
            {
                AnimatorClipPathFixerLoc.Current = (AnimatorClipPathFixerLoc.Language)newIndex;
                titleContent = new GUIContent(AnimatorClipPathFixerLoc.WindowTitle);
                Repaint();
            }
        }

        EditorGUILayout.Space(2);
    }

    private void SyncFromSelection()
    {
        _syncStatus = "";
        _syncStatusType = MessageType.Info;

        foreach (var obj in Selection.objects)
        {
            if (obj is AnimatorController projectController)
            {
                _controller = projectController;
                break;
            }
        }

        var animator = FindAnimatorFromSelection();
        if (animator != null)
        {
            _referenceRoot = animator.gameObject;
            ApplyControllerFromAnimator(animator);
            return;
        }

        if (Selection.gameObjects.Length > 0 || Selection.activeGameObject != null)
        {
            _syncStatus = AnimatorClipPathFixerLoc.SyncNoAnimator;
            _syncStatusType = MessageType.Warning;
            return;
        }

        if (_controller != null)
        {
            _syncStatus = AnimatorClipPathFixerLoc.SyncControllerOnly(_controller.name);
            _syncStatusType = MessageType.Info;
            return;
        }

        _syncStatus = AnimatorClipPathFixerLoc.SyncSelectObject;
        _syncStatusType = MessageType.Warning;
    }

    private void ApplyControllerFromAnimator(Animator animator)
    {
        var rac = animator.runtimeAnimatorController;
        if (rac is AnimatorController runtimeController)
        {
            _controller = runtimeController;
            _syncStatus = AnimatorClipPathFixerLoc.SyncAnimatorAndController(
                GetHierarchyPath(animator.transform),
                runtimeController.name);
            return;
        }

        if (rac is AnimatorOverrideController overrideController)
        {
            var baseController = overrideController.runtimeAnimatorController as AnimatorController;
            if (baseController != null)
                _controller = baseController;

            _syncStatus = AnimatorClipPathFixerLoc.SyncOverrideController(
                GetHierarchyPath(animator.transform),
                overrideController.name,
                baseController?.name ?? AnimatorClipPathFixerLoc.None);
            _syncStatusType = baseController != null ? MessageType.Info : MessageType.Warning;
            return;
        }

        if (rac != null)
        {
            _syncStatus = AnimatorClipPathFixerLoc.SyncUnknownControllerType(
                GetHierarchyPath(animator.transform),
                rac.GetType().Name,
                rac.name);
            _syncStatusType = MessageType.Warning;
            return;
        }

        _syncStatus = AnimatorClipPathFixerLoc.SyncNoRuntimeController(
            GetHierarchyPath(animator.transform));
        _syncStatusType = MessageType.Warning;
    }

    private static Animator FindAnimatorFromSelection()
    {
        var gameObjects = Selection.gameObjects;
        if (gameObjects == null || gameObjects.Length == 0)
        {
            if (Selection.activeGameObject != null)
                gameObjects = new[] { Selection.activeGameObject };
            else
                return null;
        }

        foreach (var go in gameObjects)
        {
            if (go == null)
                continue;

            var animator = go.GetComponent<Animator>();
            if (animator != null)
                return animator;

            animator = go.GetComponentInChildren<Animator>(true);
            if (animator != null)
                return animator;

            animator = go.GetComponentInParent<Animator>();
            if (animator != null)
                return animator;
        }

        return null;
    }

    private static string GetHierarchyPath(Transform t)
    {
        if (t == null)
            return "";
        var names = new List<string>();
        while (t != null)
        {
            names.Add(t.name);
            t = t.parent;
        }
        names.Reverse();
        return string.Join("/", names);
    }

    private void Analyze()
    {
        _reports.Clear();
        var animator = GetAnimator(_referenceRoot);
        if (animator == null)
        {
            EditorUtility.DisplayDialog(
                AnimatorClipPathFixerLoc.DialogTitle,
                AnimatorClipPathFixerLoc.DialogNoAnimatorOnReference,
                AnimatorClipPathFixerLoc.DialogOk);
            return;
        }

        foreach (var clip in GetClips(_controller))
        {
            var resolver = new PathResolver(animator.transform, _manualPrefix);
            _reports.Add(resolver.AnalyzeClip(clip));
        }

        SortReportsByBrokenBindings();
        Repaint();
    }

    private void ApplyFix()
    {
        if (_controller == null || _referenceRoot == null)
            return;

        var animator = GetAnimator(_referenceRoot);
        if (animator == null)
        {
            EditorUtility.DisplayDialog(
                AnimatorClipPathFixerLoc.DialogTitle,
                AnimatorClipPathFixerLoc.DialogNoAnimatorOnReference,
                AnimatorClipPathFixerLoc.DialogOk);
            return;
        }

        var summary = FixController(_controller, animator.gameObject, _manualPrefix);
        _reports.Clear();
        _reports.AddRange(summary.Reports);
        SortReportsByBrokenBindings();
        Repaint();
        ShowFixResultDialog(summary);
    }

    /// <summary>
    /// 含失效绑定（或仍有未修复项）的片段靠前；同类按失效数量降序，再按片段名排序。
    /// </summary>
    private void SortReportsByBrokenBindings()
    {
        _reports.Sort((a, b) =>
        {
            var aRemaining = Math.Max(0, a.BrokenBindings - a.FixedBindings);
            var bRemaining = Math.Max(0, b.BrokenBindings - b.FixedBindings);

            var aHasIssue = aRemaining > 0;
            var bHasIssue = bRemaining > 0;
            if (aHasIssue != bHasIssue)
                return bHasIssue.CompareTo(aHasIssue);

            if (aRemaining != bRemaining)
                return bRemaining.CompareTo(aRemaining);

            if (a.BrokenBindings != b.BrokenBindings)
                return b.BrokenBindings.CompareTo(a.BrokenBindings);

            return string.Compare(a.Clip?.name, b.Clip?.name, StringComparison.Ordinal);
        });
    }

    private void DrawReports()
    {
        if (_reports.Count == 0)
            return;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(AnimatorClipPathFixerLoc.ReportSectionTitle, EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var r in _reports)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField(r.Clip != null ? r.Clip.name : "(null)", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(AnimatorClipPathFixerLoc.ReportBindings(
                    r.TotalBindings, r.BrokenBindings, r.FixedBindings));
                if (r.Messages != null)
                {
                    foreach (var msg in r.Messages.Take(30))
                        EditorGUILayout.LabelField("  " + msg, EditorStyles.miniLabel);
                    if (r.Messages.Count > 30)
                        EditorGUILayout.LabelField(
                            AnimatorClipPathFixerLoc.ReportMoreMessages(r.Messages.Count - 30),
                            EditorStyles.miniLabel);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private static FixSummary FixController(AnimatorController controller, GameObject referenceRoot, string manualPrefix)
    {
        var summary = new FixSummary();
        var animator = GetAnimator(referenceRoot);
        if (animator == null || controller == null)
            return summary;

        var clips = GetClips(controller);
        var resolver = new PathResolver(animator.transform, manualPrefix);

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var clip in clips)
            {
                if (clip == null)
                    continue;

                var report = resolver.FixClip(clip);
                summary.Reports.Add(report);
                summary.TotalFixed += report.FixedBindings;
                if (report.FixedBindings > 0)
                    EditorUtility.SetDirty(clip);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        return summary;
    }

    private static void ShowFixResultDialog(FixSummary summary)
    {
        var totalFixed = summary.TotalFixed;
        var unresolved = summary.Reports.Sum(r => Math.Max(0, r.BrokenBindings - r.FixedBindings));

        var message =
            AnimatorClipPathFixerLoc.DialogClipsProcessed(summary.Reports.Count) + "\n" +
            AnimatorClipPathFixerLoc.DialogBindingsFixed(totalFixed) + "\n" +
            AnimatorClipPathFixerLoc.DialogStillBroken(Math.Max(0, unresolved));
        if (totalFixed == 0 && unresolved == 0)
            message += "\n" + AnimatorClipPathFixerLoc.DialogNothingToFix;

        EditorUtility.DisplayDialog(
            AnimatorClipPathFixerLoc.DialogFixCompleteTitle,
            message,
            AnimatorClipPathFixerLoc.DialogOk);
    }

    private static Animator GetAnimator(GameObject root)
    {
        if (root == null)
            return null;
        return root.GetComponent<Animator>() ?? root.GetComponentInChildren<Animator>();
    }

    private static IEnumerable<AnimationClip> GetClips(AnimatorController controller)
    {
        if (controller == null)
            yield break;

        var set = new HashSet<AnimationClip>();
        foreach (var clip in controller.animationClips)
        {
            if (clip != null && set.Add(clip))
                yield return clip;
        }
    }

    private class FixSummary
    {
        public readonly List<ClipReport> Reports = new();
        public int TotalFixed;
    }

    private class PathResolver
    {
        private readonly Transform _animatorRoot;
        private readonly string _manualPrefix;
        private readonly HashSet<string> _validPaths = new();
        private readonly List<string> _allPaths = new();

        public PathResolver(Transform animatorRoot, string manualPrefix)
        {
            _animatorRoot = animatorRoot;
            _manualPrefix = NormalizePrefix(manualPrefix);
            BuildHierarchyPaths();
        }

        public ClipReport AnalyzeClip(AnimationClip clip)
        {
            var report = new ClipReport
            {
                Clip = clip,
                Messages = new List<string>()
            };

            if (clip == null)
                return report;

            foreach (var binding in GetAllBindings(clip))
            {
                report.TotalBindings++;
                if (IsPathValid(binding.path))
                    continue;

                report.BrokenBindings++;
                if (TryResolvePath(binding.path, out var newPath))
                    report.Messages.Add(AnimatorClipPathFixerLoc.PathPreview(binding.path, newPath));
                else
                    report.Messages.Add(AnimatorClipPathFixerLoc.PathCannotAutoMatch(binding.path));
            }

            return report;
        }

        public ClipReport FixClip(AnimationClip clip)
        {
            var report = AnalyzeClip(clip);
            report.FixedBindings = 0;
            report.Messages.Clear();

            if (clip == null || report.BrokenBindings == 0)
                return report;

            Undo.RecordObject(clip, "Fix Animation Paths");

            RemapFloatCurves(clip, ref report);
            RemapObjectCurves(clip, ref report);

            return report;
        }

        private void RemapFloatCurves(AnimationClip clip, ref ClipReport report)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (IsPathValid(binding.path))
                    continue;

                if (!TryResolvePath(binding.path, out var newPath))
                    continue;

                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                AnimationUtility.SetEditorCurve(clip, binding, null);

                var newBinding = binding;
                newBinding.path = newPath;
                AnimationUtility.SetEditorCurve(clip, newBinding, curve);

                report.FixedBindings++;
                report.Messages.Add(AnimatorClipPathFixerLoc.PathFixed(binding.path, newPath));
            }
        }

        private void RemapObjectCurves(AnimationClip clip, ref ClipReport report)
        {
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (IsPathValid(binding.path))
                    continue;

                if (!TryResolvePath(binding.path, out var newPath))
                    continue;

                var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);

                var newBinding = binding;
                newBinding.path = newPath;
                AnimationUtility.SetObjectReferenceCurve(clip, newBinding, curve);

                report.FixedBindings++;
                report.Messages.Add(AnimatorClipPathFixerLoc.PathFixed(binding.path, newPath));
            }
        }

        private bool TryResolvePath(string oldPath, out string newPath)
        {
            newPath = null;
            if (string.IsNullOrEmpty(oldPath))
            {
                if (_validPaths.Contains(""))
                {
                    newPath = "";
                    return true;
                }
                return false;
            }

            if (!string.IsNullOrEmpty(_manualPrefix))
            {
                var manual = _manualPrefix + oldPath;
                if (_validPaths.Contains(manual))
                {
                    newPath = manual;
                    return true;
                }
            }

            var autoPrefix = TryDetectParentPrefix(oldPath);
            if (!string.IsNullOrEmpty(autoPrefix))
            {
                var prefixed = autoPrefix + oldPath;
                if (_validPaths.Contains(prefixed))
                {
                    newPath = prefixed;
                    return true;
                }
            }

            var suffixMatches = _allPaths
                .Where(p => p == oldPath || p.EndsWith("/" + oldPath, StringComparison.Ordinal))
                .ToList();

            if (suffixMatches.Count == 1)
            {
                newPath = suffixMatches[0];
                return true;
            }

            if (suffixMatches.Count > 1)
            {
                newPath = suffixMatches
                    .OrderBy(p => p.Length)
                    .First();
                return true;
            }

            var leaf = oldPath.Split('/').LastOrDefault();
            if (!string.IsNullOrEmpty(leaf))
            {
                var leafMatches = _allPaths
                    .Where(p => p == leaf || p.EndsWith("/" + leaf, StringComparison.Ordinal))
                    .ToList();

                if (leafMatches.Count == 1)
                {
                    newPath = leafMatches[0];
                    return true;
                }
            }

            return false;
        }

        private bool IsPathValid(string path)
        {
            return _validPaths.Contains(path ?? "");
        }

        private void BuildHierarchyPaths()
        {
            _validPaths.Clear();
            _allPaths.Clear();
            _validPaths.Add("");

            foreach (Transform child in _animatorRoot)
                CollectPaths(child, child.name);
        }

        private void CollectPaths(Transform t, string relativePath)
        {
            _validPaths.Add(relativePath);
            _allPaths.Add(relativePath);

            foreach (Transform child in t)
                CollectPaths(child, relativePath + "/" + child.name);
        }

        /// <summary>
        /// 动画仍从旧根节点名开始，但该节点已变成 Animator 下某个子物体的后代（多包了一层父节点）。
        /// </summary>
        private string TryDetectParentPrefix(string oldPath)
        {
            var firstSegment = oldPath.Split('/')[0];
            if (_animatorRoot.Find(firstSegment) != null)
                return "";

            foreach (Transform child in _animatorRoot)
            {
                if (child.Find(firstSegment) == null)
                    continue;

                var prefix = child.name + "/";
                if (_validPaths.Contains(prefix + oldPath))
                    return prefix;
            }

            return "";
        }

        private static string NormalizePrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return "";
            prefix = prefix.Replace('\\', '/').Trim('/');
            return string.IsNullOrEmpty(prefix) ? "" : prefix + "/";
        }

        private static IEnumerable<EditorCurveBinding> GetAllBindings(AnimationClip clip)
        {
            foreach (var b in AnimationUtility.GetCurveBindings(clip))
                yield return b;
            foreach (var b in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                yield return b;
        }
    }
}
#endif
