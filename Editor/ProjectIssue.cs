using System;
using Unity.ProjectAuditor.Editor.CallAnalysis;
using Unity.ProjectAuditor.Editor.Utils;

namespace Unity.ProjectAuditor.Editor
{
    public enum Area{
        CPU,
        GPU,
        Memory,
        BuildSize,
        LoadTimes,
        
        All,        
    }

    public enum IssueCategory
    {
        ApiCalls,
        ProjectSettings,
        NumCategories
    }

    [Serializable]
    public class ProjectIssue
    {
        public ProblemDescriptor descriptor;
        public CallTreeNode callTree;
        public IssueCategory category;
        public Location location;
        public string assembly;

        public string description;

        public string filename
        {
            get
            {
                if (location == null)
                    return string.Empty;
                return location.filename;
            }
        }
        
        public string relativePath
        {
            get
            {
                if (location == null)
                    return string.Empty;
                return location.relativePath;
            }
        }

        public int line
        {
            get
            {
                if (location == null)
                    return 0;
                return location.line;
            }
        }
        
        public string callingMethod
        {
            get
            {
                if (callTree == null)
                    return string.Empty;
                if (!callTree.HasChildren())
                    return string.Empty;

                return callTree.GetChild().name;
            }
        }

        public bool isPerfCriticalContext
        {
            get
            {
                if (callTree == null)
                    return false;

                return callTree.IsPerfCriticalContext();
            }
        }
        
        public string name
        {
            get
            {
                if (callTree == null)
                    return string.Empty;
                if (callTree.prettyName.Equals(descriptor.description))
                {
                    // if name matches the descriptor's name, use caller's name instead
                    return string.IsNullOrEmpty(callingMethod) ? string.Empty : callTree.GetChild().prettyName;
                }
                else
                {
                    return callTree.prettyName;
                }
            }
        }

        private ProjectIssue()
        {}
        
        public ProjectIssue(ProblemDescriptor descriptor,
            string description,
            IssueCategory category,
            Location location = null)
        {
            this.descriptor = descriptor;
            this.description = description;
            this.category = category;
            this.location = location;
        }
        
        public ProjectIssue(ProblemDescriptor descriptor,
            string description,
            IssueCategory category,
            CallTreeNode callTreeNode)
        {
            this.descriptor = descriptor;
            this.description = description;
            this.category = category;
            this.callTree = callTreeNode;
        }
    }
}
