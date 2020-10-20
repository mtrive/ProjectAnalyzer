using System;
using System.Collections.Generic;
using Unity.ProjectAuditor.Editor.CodeAnalysis;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor.IMGUI.Controls;

namespace Unity.ProjectAuditor.Editor.UI
{
    internal class CallHierarchyView : TreeView
    {
        private readonly Dictionary<int, CallTreeNode> m_CallTreeDictionary = new Dictionary<int, CallTreeNode>();
        private CallTreeNode m_CallTree;
        private Action<Location> m_OnDoubleClick;

        public CallHierarchyView(TreeViewState treeViewState, Action<Location> onDoubleClick)
            : base(treeViewState)
        {
            m_OnDoubleClick = onDoubleClick;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Hidden Root"};
            var allItems = new List<TreeViewItem>();

            if (m_CallTree != null)
            {
                m_CallTreeDictionary.Clear();
                BuildNode(allItems, m_CallTree, 0);
            }

            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;
        }

        public void SetCallTree(CallTreeNode callTree)
        {
            m_CallTree = callTree;
        }

        private void BuildNode(List<TreeViewItem> items, CallTreeNode callTree, int depth)
        {
            var id = items.Count;
            items.Add(new TreeViewItem {id = id, depth = depth, displayName = callTree.GetPrettyName(true)});

            m_CallTreeDictionary.Add(id, callTree);

            // if the tree is too deep, serialization will exceed the 7 levels limit.
            if (!callTree.HasValidChildren())
                items.Add(new TreeViewItem {id = id + 1, depth = depth + 1, displayName = "<Serialization Limit>"});
            else
                for (int i = 0; i < callTree.GetNumChildren(); i++)
                {
                    BuildNode(items, callTree.GetChild(i), depth + 1);
                }
        }

        protected override void DoubleClickedItem(int id)
        {
            if (m_CallTreeDictionary.ContainsKey(id))
            {
                var node = m_CallTreeDictionary[id];
                if (node.location != null)
                    m_OnDoubleClick(node.location);
            }
        }
    }
}
