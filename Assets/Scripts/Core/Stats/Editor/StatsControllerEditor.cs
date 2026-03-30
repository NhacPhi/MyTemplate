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

        private const string _errorMessage = "UUID Not Found";
        private const string _allAttributes = "All Attributes: ";
        private const string _allStats = "All Substats: ";

        private string _search;

        private string text;
        private Dictionary<string, StatsDataHolder> dataBase;

        private StatsDataHolder statsHolder;

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
                text = File.ReadAllText("Assets/Data/GameConfig/CharacterConfig.json");
            }
            catch(Exception)
            {
                return null;
            }

            dataBase = Json.DeserializeObject<Dictionary<string, StatsDataHolder>>(text);

            StringSearch search = ScriptableObject.CreateInstance<StringSearch>();

            search.Keys = new List<string>();
            search.Callback = (value) =>
            {
                _idProperty.stringValue = value;
                _idProperty.serializedObject.ApplyModifiedProperties();
            };

            foreach (var key in dataBase)
            {
                search.Keys.Add(key.Key);
            }

            var button = new Button(() =>
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility
                    .GUIToScreenPoint(Event.current.mousePosition)), search);
            });

            button.style.height = 20;
            button.text = "Search UUID";
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

            if (!dataBase.TryGetValue(_idProperty.stringValue, out statsHolder))
            {
                _Body.Add(new HelpBox(_errorMessage, HelpBoxMessageType.Error));
                return;
            }

            if (!target) return;

            bool playMode = EditorApplication.isPlayingOrWillChangePlaymode &&
                !PrefabUtility.IsPartOfPrefabAsset(target);

            StatsController controller = target as StatsController;

            if (playMode)
            {
                _Body.Add(new AttributeViewEditor(controller));
                _Body.Add(new StatViewEditor(controller));
                return;
            }

            InitBodyInEditor(statsHolder, controller);
        }

        private void InitBodyInEditor(StatsDataHolder statsHolder, StatsController controller)
        {
            try
            {
                var attributesView = new AttributeViewEditor();
                var statsView = new StatViewEditor();

                var labelAttribites = new Label(_allAttributes);
                labelAttribites.style.fontSize = 12;
                labelAttribites.style.unityFontStyleAndWeight = FontStyle.Bold;
                attributesView.Body.Add(labelAttribites);

                var labelStats = new Label(_allStats);
                labelStats.style.fontSize = 12;
                labelStats.style.unityFontStyleAndWeight = FontStyle.Bold;
                statsView.Body.Add(labelStats);

                foreach (AttributeType key in statsHolder.Attributes.Keys)
                {
                    labelAttribites.text += key.ToString() + " | ";
                }

                foreach (StatType key in statsHolder.Stats.Keys)
                {
                    labelStats.text += key.ToString() + " | ";
                }

                _Body.Add(attributesView);
                _Body.Add(statsView);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif


