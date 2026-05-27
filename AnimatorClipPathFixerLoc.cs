#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Animator 动画路径修复工具 — 面板文案多语言。
/// </summary>
public static class AnimatorClipPathFixerLoc
{
    public enum Language
    {
        Chinese = 0,
        Japanese = 1,
        English = 2,
        Korean = 3
    }

    private const string PrefsKey = "AnimatorClipPathFixer.Language";

    public static readonly string[] LanguageOptions = { "中文", "日本語", "English", "한국어" };

    private static Language _current;

    static AnimatorClipPathFixerLoc()
    {
        _current = (Language)EditorPrefs.GetInt(PrefsKey, (int)Language.Chinese);
    }

    public static Language Current
    {
        get => _current;
        set
        {
            if (_current == value)
                return;
            _current = value;
            EditorPrefs.SetInt(PrefsKey, (int)value);
        }
    }

    public static string WindowTitle => T("动画路径修复", "アニメパス修復", "Animation Path Fixer", "애니메이션 경로 복구");

    public static string Title => T(
        "修复 Animator 动画片段路径",
        "Animator アニメクリップのパスを修復",
        "Fix Animator Animation Clip Paths",
        "Animator 애니메이션 클립 경로 복구");

    public static string HelpBox => T(
        "层级变更父节点后，动画片段里的绑定路径会失效。\n" +
        "1. 在层级中选中带 Animator 的物体（或其任意子/父节点）\n" +
        "2. 点击「从选中识别」自动填充 Animator 与 Controller\n" +
        "3. 先「分析」查看将修复的路径，再「应用修复」",
        "階層の親ノード変更後、アニメクリップ内のバインドパスが無効になります。\n" +
        "1. ヒエラルキーで Animator が付いたオブジェクト（またはその子/親）を選択\n" +
        "2. 「選択から認識」で Animator と Controller を自動入力\n" +
        "3. 「分析」で修復内容を確認し、「修復を適用」で反映",
        "After hierarchy parent changes, animation clip binding paths may break.\n" +
        "1. Select an object with Animator in the Hierarchy (or any child/parent)\n" +
        "2. Click \"Detect from Selection\" to fill Animator and Controller\n" +
        "3. Run \"Analyze\" first, then \"Apply Fix\"",
        "계층 부모 노드 변경 후 애니메이션 클립의 바인딩 경로가 무효화될 수 있습니다.\n" +
        "1. Hierarchy에서 Animator가 있는 오브젝트(또는 자식/부모) 선택\n" +
        "2. 「선택에서 인식」으로 Animator와 Controller 자동 입력\n" +
        "3. 「분석」으로 확인 후 「수정 적용」");

    public static string LabelLanguage => T("语言", "言語", "Language", "언어");

    public static string FieldAnimatorController => "Animator Controller";

    public static string FieldReferenceRoot => T("参考层级根物体", "参照ルート", "Reference Root", "참조 루트");

    public static string FieldReferenceRootTip => T(
        "带 Animator 的物体，动画路径相对此节点解析",
        "Animator が付いたオブジェクト。パスはこのノード基準で解決",
        "Object with Animator; paths resolve relative to this node",
        "Animator가 붙은 오브젝트. 경로는 이 노드 기준으로 해석");

    public static string FieldManualPrefix => T("手动路径前缀", "手動パス接頭辞", "Manual Path Prefix", "수동 경로 접두사");

    public static string FieldManualPrefixTip => T(
        "自动匹配失败时，为所有失效路径统一添加此前缀（如 NewParent/）",
        "自動一致に失敗した場合、無効なパスに一律で付与（例: NewParent/）",
        "When auto-match fails, prepend this to all broken paths (e.g. NewParent/)",
        "자동 매칭 실패 시 모든 무효 경로에 접두사 추가 (예: NewParent/)");

    public static string BtnDetectFromSelection => T("从选中识别", "選択から認識", "Detect from Selection", "선택에서 인식");

    public static string BtnAnalyze => T("分析", "分析", "Analyze", "분석");

    public static string BtnApplyFix => T("应用修复", "修復を適用", "Apply Fix", "수정 적용");

    public static string SyncNoAnimator => T(
        "选中物体上未找到 Animator（已搜索自身、子级、父级）。",
        "選択オブジェクトに Animator が見つかりません（自身・子・親を検索済み）。",
        "No Animator found on selection (searched self, children, and parents).",
        "선택 오브젝트에서 Animator를 찾을 수 없습니다(자신·자식·부모 검색).");

    public static string SyncControllerOnly(string controllerName) => T(
        $"已同步 Project 中的 Controller：{controllerName}\n请在层级中选中带 Animator 的物体。",
        $"Project の Controller を同期しました：{controllerName}\nヒエラルキーで Animator 付きオブジェクトを選択してください。",
        $"Synced Controller from Project: {controllerName}\nSelect an object with Animator in the Hierarchy.",
        $"Project의 Controller 동기화: {controllerName}\nHierarchy에서 Animator가 있는 오브젝트를 선택하세요.");

    public static string SyncSelectObject => T(
        "请在层级中选中带 Animator 的物体。",
        "ヒエラルキーで Animator 付きオブジェクトを選択してください。",
        "Select an object with Animator in the Hierarchy.",
        "Hierarchy에서 Animator가 있는 오브젝트를 선택하세요.");

    public static string SyncAnimatorAndController(string hierarchyPath, string controllerName) => T(
        $"Animator：{hierarchyPath}\nController：{controllerName}",
        $"Animator：{hierarchyPath}\nController：{controllerName}",
        $"Animator: {hierarchyPath}\nController: {controllerName}",
        $"Animator: {hierarchyPath}\nController: {controllerName}");

    public static string SyncOverrideController(string hierarchyPath, string overrideName, string baseName) => T(
        $"Animator：{hierarchyPath}\nOverrideController：{overrideName}\n基础 Controller：{baseName}",
        $"Animator：{hierarchyPath}\nOverrideController：{overrideName}\nベース Controller：{baseName}",
        $"Animator: {hierarchyPath}\nOverrideController: {overrideName}\nBase Controller: {baseName}",
        $"Animator: {hierarchyPath}\nOverrideController: {overrideName}\n기본 Controller: {baseName}");

    public static string None => T("（无）", "（なし）", "(none)", "(없음)");

    public static string SyncUnknownControllerType(string hierarchyPath, string typeName, string assetName) => T(
        $"Animator：{hierarchyPath}\n已挂载 {typeName}「{assetName}」，请手动指定 Animator Controller。",
        $"Animator：{hierarchyPath}\n{typeName}「{assetName}」が割り当て済み。Animator Controller を手動指定してください。",
        $"Animator: {hierarchyPath}\nAssigned {typeName} \"{assetName}\". Please assign Animator Controller manually.",
        $"Animator: {hierarchyPath}\n{typeName}「{assetName}」할당됨. Animator Controller를 수동 지정하세요.");

    public static string SyncNoRuntimeController(string hierarchyPath) => T(
        $"Animator：{hierarchyPath}\n未挂载 Runtime Animator Controller，请手动指定。",
        $"Animator：{hierarchyPath}\nRuntime Animator Controller が未割り当て。手動指定してください。",
        $"Animator: {hierarchyPath}\nNo Runtime Animator Controller assigned. Please assign manually.",
        $"Animator: {hierarchyPath}\nRuntime Animator Controller 미할당. 수동 지정하세요.");

    public static string DialogTitle => T("动画路径修复", "アニメパス修復", "Animation Path Fixer", "애니메이션 경로 복구");

    public static string DialogOk => T("确定", "OK", "OK", "확인");

    public static string DialogNoAnimatorOnReference => T(
        "参考物体上未找到 Animator。",
        "参照オブジェクトに Animator が見つかりません。",
        "No Animator found on the reference object.",
        "참조 오브젝트에서 Animator를 찾을 수 없습니다.");

    public static string DialogFixCompleteTitle => T(
        "动画路径修复完成",
        "アニメパス修復完了",
        "Animation Path Fix Complete",
        "애니메이션 경로 복구 완료");

    public static string DialogClipsProcessed(int count) => T(
        $"处理片段数: {count}",
        $"処理クリップ数: {count}",
        $"Clips processed: {count}",
        $"처리한 클립 수: {count}");

    public static string DialogBindingsFixed(int count) => T(
        $"修复绑定数: {count}",
        $"修復バインド数: {count}",
        $"Bindings fixed: {count}",
        $"복구한 바인딩 수: {count}");

    public static string DialogStillBroken(int count) => T(
        $"仍失效绑定: {count}",
        $"未修復バインド: {count}",
        $"Still broken bindings: {count}",
        $"여전히 무효인 바인딩: {count}");

    public static string DialogNothingToFix => T(
        "未发现需要修复的路径。",
        "修復が必要なパスは見つかりませんでした。",
        "No paths needed fixing.",
        "수정이 필요한 경로가 없습니다.");

    public static string ReportSectionTitle => T(
        "分析 / 修复结果",
        "分析 / 修復結果",
        "Analyze / Fix Results",
        "분석 / 수정 결과");

    public static string ReportBindings(int total, int broken, int fixedCount) => T(
        $"绑定: {total}  失效: {broken}  已修复: {fixedCount}",
        $"バインド: {total}  無効: {broken}  修復: {fixedCount}",
        $"Bindings: {total}  Broken: {broken}  Fixed: {fixedCount}",
        $"바인딩: {total}  무효: {broken}  복구: {fixedCount}");

    public static string ReportMoreMessages(int count) => T(
        $"  ... 另有 {count} 条",
        $"  ... 他 {count} 件",
        $"  ... {count} more",
        $"  ... 외 {count}개");

    public static string PathCannotAutoMatch(string path) => T(
        $"{path} (无法自动匹配)",
        $"{path} (自動一致不可)",
        $"{path} (cannot auto-match)",
        $"{path} (자동 매칭 불가)");

    public static string PathFixed(string oldPath, string newPath) => T(
        $"[已修复] {oldPath} → {newPath}",
        $"[修復済] {oldPath} → {newPath}",
        $"[Fixed] {oldPath} → {newPath}",
        $"[복구됨] {oldPath} → {newPath}");

    public static string PathPreview(string oldPath, string newPath) => $"{oldPath} → {newPath}";

    private static string T(string zh, string ja, string en, string ko) => _current switch
    {
        Language.Japanese => ja,
        Language.English => en,
        Language.Korean => ko,
        _ => zh
    };
}
#endif
