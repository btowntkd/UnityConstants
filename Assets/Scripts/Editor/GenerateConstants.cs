using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Generate 
/// </summary>
public static class GenerateScriptConstants
{
    #region Configurable Constants
    /// <summary>
    /// The Unity folder path, in which the generated files will be saved.
    /// Modify this value if you wish to save the generated files to a different location.
    /// </summary>
    public const string BaseFolderPath = "Scripts/Constants";

    /// <summary>
    /// The filename in which the resulting "Scenes" script constants will be saved.
    /// </summary>
    public const string ScenesFilename = "Scenes.cs";
    /// <summary>
    /// The filename in which the resulting "Tags" script constants will be saved.
    /// </summary>
    public const string TagsFilename = "Tags.cs";
    /// <summary>
    /// The filename in which the resulting "Layers" script constants will be saved.
    /// </summary>
    public const string LayersFilename = "Layers.cs";
    /// <summary>
    /// The filename in which the resulting "Sorting Layers" script constants will be saved.
    /// </summary>
    public const string SortingLayersFilename = "SortingLayers.cs";
    /// <summary>
    /// The filename in which the resulting "Input Axes" script constants will be saved.
    /// </summary>
    public const string InputAxesFilename = "InputAxes.cs";
    /// <summary>
    /// The filename in which the resulting "Audio Mixer Parameters" script constants will be saved.
    /// </summary>
    public const string AudioMixerParametersFilename = "AudioMixerParameters.cs";
    /// <summary>
    /// The filename in which the resulting "Animator Parameters" script constants will be saved.
    /// </summary>
    public const string AnimatorParametersFilename = "AnimatorParameters.cs";

    /// <summary>
    /// The default namespace in which the generated static classes will reside.
    /// By default, no namespace is used (blank string).  
    /// Change this value if you want the generated classes to be grouped into a namespace.
    /// </summary>
    public const string Namespace = "";

    /// <summary>
    /// Generated script files will be indented using this string.
    /// By default, indents are 4 spaces.
    /// </summary>
    public const string IndentString = "    ";

    #endregion

    #region Menu Items

    [MenuItem("Edit/Generate Script Constants/All", priority = 0)]
    public static void GenerateAll()
    {
        EditorUtility.DisplayProgressBar("Generating Scripts", "Generating scripts with hard-coded constants...", -1f);
        GenerateScenes();
        GenerateTags();
        GenerateLayers();
        GenerateSortingLayers();
        GenerateInputAxes();
        GenerateAudioMixerParameters();
        GenerateAnimatorParameters();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Edit/Generate Script Constants/Scenes", priority = 100)]
    public static void GenerateScenes()
    {
        const string ClassName = "Scenes";
        using (var writer = CreateUnityScript(Path.Combine(BaseFolderPath, ScenesFilename)))
        {
            var indent = string.Empty;
            WriteScriptHeader(writer);
            BeginNamespaceAndClass(writer, Namespace, ClassName, ref indent);

            var scenes = EditorBuildSettings.scenes;
            for (int x = 0; x < scenes.Length; x++)
            {
                var scene = scenes[x];
                var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                WriteIntegerConstant(writer, FormatVariableName(sceneName), x, indent);
            }

            EndNamespaceAndClass(writer, Namespace, ClassName, ref indent);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Edit/Generate Script Constants/Tags", priority = 100)]
    public static void GenerateTags()
    {
        const string ClassName = "Tags";
        using (var writer = CreateUnityScript(Path.Combine(BaseFolderPath, TagsFilename)))
        {
            var indent = string.Empty;
            WriteScriptHeader(writer);
            BeginNamespaceAndClass(writer, Namespace, ClassName, ref indent);

            var tags = UnityEditorInternal.InternalEditorUtility.tags;
            foreach(var tag in tags)
            {
                WriteStringConstant(writer, FormatVariableName(tag), tag, indent);
            }

            EndNamespaceAndClass(writer, Namespace, ClassName, ref indent);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Edit/Generate Script Constants/Layers", priority = 100)]
    public static void GenerateLayers()
    {
        const string ClassName = "Layers";
        using (var writer = CreateUnityScript(Path.Combine(BaseFolderPath, LayersFilename)))
        {
            var indent = string.Empty;
            WriteScriptHeader(writer);
            BeginNamespaceAndClass(writer, Namespace, ClassName, ref indent);

            // Unity has 32 layers
            for (int x = 0; x < 32; x++)
            {
                var layerName = UnityEditorInternal.InternalEditorUtility.GetLayerName(x);
                var layerMask = 1 << x;
                if (!string.IsNullOrEmpty(layerName))
                {
                    WriteStringConstant(writer, FormatVariableName(layerName), layerName, indent);
                    WriteIntegerConstant(writer, FormatVariableName(layerName + "Mask"), layerMask, indent);
                }
            }

            EndNamespaceAndClass(writer, Namespace, ClassName, ref indent);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Edit/Generate Script Constants/Sorting Layers", priority = 100)]
    public static void GenerateSortingLayers()
    {
        const string ClassName = "SortingLayers";
        using (var writer = CreateUnityScript(Path.Combine(BaseFolderPath, SortingLayersFilename)))
        {
            var indent = string.Empty;
            WriteScriptHeader(writer);
            BeginNamespaceAndClass(writer, Namespace, ClassName, ref indent);

            var sortingLayers = SortingLayer.layers;
            foreach (var sortingLayer in sortingLayers)
            {
                WriteIntegerConstant(writer, FormatVariableName(sortingLayer.name), sortingLayer.id, indent);
            }

            EndNamespaceAndClass(writer, Namespace, ClassName, ref indent);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Edit/Generate Script Constants/Input Axes", priority = 100)]
    public static void GenerateInputAxes()
    {
        const string ClassName = "InputAxes";
        using (var writer = CreateUnityScript(Path.Combine(BaseFolderPath, InputAxesFilename)))
        {
            var indent = string.Empty;
            WriteScriptHeader(writer);
            BeginNamespaceAndClass(writer, Namespace, ClassName, ref indent);
            var alreadySeen = new HashSet<string>();

            var inputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axisArray = inputManager.FindProperty("m_Axes");
            for (int x = 0; x < axisArray.arraySize; x++)
            {
                var axis = axisArray.GetArrayElementAtIndex(x);
                var axisName = axis.FindPropertyRelative("m_Name").stringValue;

                if (alreadySeen.Contains(axisName))
                    continue;

                alreadySeen.Add(axisName);
                WriteStringConstant(writer, FormatVariableName(axisName), axisName, indent);
            }
            EndNamespaceAndClass(writer, Namespace, ClassName, ref indent);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Edit/Generate Script Constants/Audio Mixer Parameters", priority = 100)]
    public static void GenerateAudioMixerParameters()
    {
        const string ClassName = "AudioMixerParameters";
        using (var writer = CreateUnityScript(Path.Combine(BaseFolderPath, AudioMixerParametersFilename)))
        {
            var indent = string.Empty;
            WriteScriptHeader(writer);
            BeginNamespaceAndClass(writer, Namespace, ClassName, ref indent);
            var mixerAssets = AssetDatabase.FindAssets("t:AudioMixer");
            var allMixerNamesAndParameters = new Dictionary<string, HashSet<string>>();
            foreach (var mixerGuid in mixerAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(mixerGuid);
                var mixerName = Path.GetFileNameWithoutExtension(path);
                var mixerObj = new SerializedObject(AssetDatabase.LoadAssetAtPath<AudioMixer>(path));

                HashSet<string> currentMixerParams;
                if (!allMixerNamesAndParameters.TryGetValue(mixerName, out currentMixerParams))
                {
                    currentMixerParams = new HashSet<string>();
                    allMixerNamesAndParameters.Add(mixerName, currentMixerParams);
                }

                foreach (SerializedProperty param in mixerObj.FindProperty("m_ExposedParameters"))
                {
                    var paramName = param.FindPropertyRelative("name").stringValue;
                    if (!currentMixerParams.Contains(paramName))
                        currentMixerParams.Add(paramName);
                }
            }

            foreach (var mixer in allMixerNamesAndParameters)
            {
                BeginClass(writer, mixer.Key, ref indent);
                foreach (var mixerParam in mixer.Value)
                {
                    WriteStringConstant(writer, FormatVariableName(mixerParam), mixerParam, indent);
                }
                EndClass(writer, mixer.Key, ref indent);
            }

            EndNamespaceAndClass(writer, Namespace, ClassName, ref indent);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Edit/Generate Script Constants/Animator Parameters", priority = 100)]
    public static void GenerateAnimatorParameters()
    {
        const string ClassName = "AnimatorParameters";
        using (var writer = CreateUnityScript(Path.Combine(BaseFolderPath, AnimatorParametersFilename)))
        {
            var indent = "";
            BeginNamespaceAndClass(writer, Namespace, ClassName, ref indent);

            var animatorAssets = AssetDatabase.FindAssets("t:AnimatorController");
            var allAnimatorNamesAndParameters = new Dictionary<string, HashSet<string>>();
            foreach (var animatorGuid in animatorAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(animatorGuid);
                var animatorName = Path.GetFileNameWithoutExtension(path);
                var animatorObj = new SerializedObject(AssetDatabase.LoadAssetAtPath<AnimatorController>(path));

                HashSet<string> currentAnimatorParams;
                if (!allAnimatorNamesAndParameters.TryGetValue(animatorName, out currentAnimatorParams))
                {
                    currentAnimatorParams = new HashSet<string>();
                    allAnimatorNamesAndParameters.Add(animatorName, currentAnimatorParams);
                }

                foreach (SerializedProperty param in animatorObj.FindProperty("m_AnimatorParameters"))
                {
                    var paramName = param.FindPropertyRelative("m_Name").stringValue;
                    if(!currentAnimatorParams.Contains(paramName))
                        currentAnimatorParams.Add(paramName);
                }
            }

            foreach (var animator in allAnimatorNamesAndParameters)
            {
                BeginClass(writer, animator.Key, ref indent);
                foreach (var animatorParam in animator.Value)
                {
                    WriteStringConstant(writer, FormatVariableName(animatorParam), animatorParam, indent);
                    WriteIntegerConstant(writer, FormatVariableName(animatorParam + "Hash"), Animator.StringToHash(animatorParam), indent);
                }
                EndClass(writer, animator.Key, ref indent);
            }

            EndNamespaceAndClass(writer, Namespace, ClassName, ref indent);
        }
        AssetDatabase.Refresh();
    }

    #endregion

    #region Helper Functions

    private static StreamWriter CreateUnityScript(string assetFilePath)
    {
        var absoluteFilePath = Path.Combine(Application.dataPath, assetFilePath);
        var directoryName = Path.GetDirectoryName(absoluteFilePath);
        Directory.CreateDirectory(directoryName);
        return new StreamWriter(absoluteFilePath);
    }

    private static void WriteScriptHeader(StreamWriter writer)
    {
        writer.WriteLine("// This file is automatically generated.  Changes will be overwritten.");
    }

    private static void BeginNamespaceAndClass(StreamWriter writer, string namespaceName, string className, ref string indent)
    {
        BeginNamespace(writer, namespaceName, ref indent);
        BeginClass(writer, className, ref indent);
    }

    private static void EndNamespaceAndClass(StreamWriter writer, string namespaceName, string className, ref string indent)
    {
        EndClass(writer, className, ref indent);
        EndNamespace(writer, namespaceName, ref indent);
    }

    private static void BeginNamespace(StreamWriter writer, string namespaceName, ref string indent)
    {
        if(string.IsNullOrEmpty(namespaceName))
            return;

        writer.WriteLine("{0}namespace {1}", indent, FormatVariableName(namespaceName));
        writer.WriteLine("{0}{{", indent);
        PushIndent(ref indent);
    }
    private static void EndNamespace(StreamWriter writer, string namespaceName, ref string indent)
    {
        if(string.IsNullOrEmpty(namespaceName))
            return;

        PopIndent(ref indent);
        writer.WriteLine("{0}}}", indent);
    }
    private static void BeginClass(StreamWriter writer, string className, ref string indent)
    {
        if (string.IsNullOrEmpty(className))
            return;

        writer.WriteLine("{0}public static class {1}", indent, FormatVariableName(className));
        writer.WriteLine("{0}{{", indent);
        PushIndent(ref indent);
    }
    private static void EndClass(StreamWriter writer, string className, ref string indent)
    {
        if (string.IsNullOrEmpty(className))
            return;

        PopIndent(ref indent);
        writer.WriteLine("{0}}}", indent);
    }

    private static void WriteSummaryDescription(StreamWriter writer, string description, string indent)
    {
        writer.WriteLine("{0}/// <summary>", indent);
        writer.WriteLine("{0}/// {1}", indent, description);
        writer.WriteLine("{0}/// </summary>", indent);
    }

    private static void WriteStringConstant(StreamWriter writer, string name, string value, string indent)
    {
        writer.WriteLine("{0}public const string {1} = \"{2}\";", indent, FormatVariableName(name), value);
    }
    private static void WriteIntegerConstant(StreamWriter writer, string name, int value, string indent)
    {
        writer.WriteLine("{0}public const int {1} = {2};", indent, FormatVariableName(name), value);
    }

    /// <summary>
    /// Given an indent string, add another level of indentation.
    /// </summary>
    private static void PushIndent(ref string indent)
    {
        indent = IndentString + indent;
    }

    /// <summary>
    /// Given an indent string, remove one level of indentation.
    /// </summary>
    private static void PopIndent(ref string indent)
    {
        if (indent.Length <= IndentString.Length)
            indent = string.Empty;
        else
            indent = indent.Substring(0, indent.Length - IndentString.Length);
    }

    private static string FormatVariableName(string input)
    {
        var result = Regex.Replace(input, "[^a-zA-Z0-9]", "");
        if (char.IsDigit(result[0]))
        {
            result = "_" + result;
        }
        return result.ToString();
    }

    #endregion
}
