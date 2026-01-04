#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using UnityEditor;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;


namespace Stats.Editor
{
    [CustomEditor(typeof(StatsController), true)]
    public class StatsControllerEditor : UnityEditor.Editor
    {
        private VisualElement _Root;
        private VisualElement _Head;
        private VisualElement _Body;

        private const string _errorMessage = "ID Not Found";
        private const string _alllAttribute = "All Attributes";
        private const string _allStats = "All Stats";

        private string _search;

        private string text;
        private Dictionary<string, StatsDataHolder> dataBase;
        //private List<CharacterStatConfig> characterStatConfigs;

        //private StatsDataHolder statsHolder;
        //private CharacterStatConfig statsHolder;
        private SerializedProperty _idProperty;

        public override VisualElement CreateInspectorGUI()
        {
            _Root = new VisualElement();
            _Head = new VisualElement();
            _Body = new VisualElement();

            _Root.Add(_Head );
            _Root.Add(_Body);

            try
            {
                text = File.ReadAllText("Assets/Data/GameConfig/CharacterStat.json");
            }
            catch(Exception)
            {
                return null;
            }

            //dataBase = Json.DeserializeObject<Dictionary<string, StatsDataHolder>>(text);
            //characterStatConfigs = Json.DeserializeObject<List<CharacterStatConfig>>(text);

            StringSearch search = ScriptableObject.CreateInstance<StringSearch>();

            search.Keys = new List<string>();
            search.Callback = (value) =>
            {
                _idProperty.stringValue = value;
                _idProperty.serializedObject.ApplyModifiedProperties();
            };

            //foreach(var key in characterStatConfigs)
            //{
            //    search.Keys.Add(key.ID);
            //}

            var button = new Button(() =>
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility
                    .GUIToScreenPoint(Event.current.mousePosition)), search);
            });

            button.style.height = 20;
            button.text = "Search ID";
            var idField = new TextField();
            _idProperty = serializedObject.FindProperty("<EntityID>k__BackingField");
            idField.BindProperty(_idProperty);
            RefreshBody();

            if(!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _Head.style.flexDirection = FlexDirection.Column;
                _Head.Add(button);
                _Head.Add(idField);
            }

            idField.RegisterValueChangedCallback((e) =>
            {
                RefreshBody();
            });

            return _Root;
        }

        private void RefreshBody()
        {
            serializedObject.Update();
            _Body.Clear();

            //statsHolder = characterStatConfigs.Find(x => x.ID == _idProperty.stringValue);
            //if (statsHolder == null)
            //{
            //    _Body.Add(new HelpBox(_errorMessage, HelpBoxMessageType.Error));
            //    return;
            //}

            if (!target) return;

            bool playMode = EditorApplication.isPlayingOrWillChangePlaymode &&
                !PrefabUtility.IsPartOfPrefabAsset(target);

            StatsController controller = target as StatsController;

            if (playMode)
            {
                _Body.Add(new StatViewEditor(controller));
                return;
            }

            //InitBodyInEditor(statsHolder, controller);
        }

        //private void InitBodyInEditor(CharacterStatConfig statsHolder, StatsController controller)
        //{
        //    try
        //    {
        //        var statsView = new StatViewEditor();

        //        var labelStats = new Label(_allStats);
        //        labelStats.style.fontSize = 12;
        //        labelStats.style.unityFontStyleAndWeight = FontStyle.Bold;
        //        statsView.Body.Add(labelStats);

        //        //foreach (StatType key in statsHolder.Stats.Keys)
        //        //{
        //        //    labelStats.text += key.ToString() + " | ";
        //        //}

        //        _Body.Add(statsView);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogException(e);
        //    }
        //}
    }
}
#endif


