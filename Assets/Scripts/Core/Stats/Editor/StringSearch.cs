#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class StringSearch : ScriptableObject, ISearchWindowProvider
{
    public List<string> Keys;
    public Action<string> Callback;
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var entries = new List<SearchTreeEntry>();
        var groupEntry = new SearchTreeGroupEntry(new GUIContent("List"), 0);
        entries.Add(groupEntry);
        Keys.Sort((a, b) => string.Compare(a, b, StringComparison.Ordinal));
        foreach(var str in Keys)
        {
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(str))
            {
                level = 1,
                userData = str
            };

            entries.Add(entry);
        }

        return entries;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        Callback?.Invoke((string)SearchTreeEntry.userData);
        return true;
    }


}
#endif