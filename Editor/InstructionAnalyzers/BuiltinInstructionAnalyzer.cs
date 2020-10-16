using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.ProjectAuditor.Editor.CodeAnalysis;

namespace Unity.ProjectAuditor.Editor.InstructionAnalyzers
{
    internal class BuiltinInstructionAnalyzer : IInstructionAnalyzer
    {
        private Dictionary<string, ProblemDescriptor> m_Descriptors; // type+method name as key
        private Dictionary<string, ProblemDescriptor> m_WholeNamespaceDescriptors; // namespace as key

        public void Initialize(IAuditor auditor)
        {
            m_Descriptors = auditor.GetDescriptors().Where(descriptor => !descriptor.method.Equals("*") && !string.IsNullOrEmpty(descriptor.type)).ToDictionary(descriptor => descriptor.type + "." + descriptor.method);
            m_WholeNamespaceDescriptors = auditor.GetDescriptors().Where(descriptor => descriptor.method.Equals("*")).ToDictionary(d => d.type);
        }

        public ProjectIssue Analyze(MethodDefinition methodDefinition, Instruction inst)
        {
            var callee = (MethodReference)inst.Operand;

            // replace root with callee node
            var calleeNode = new CallTreeNode(callee);
            var description = string.Empty;

            ProblemDescriptor descriptor;
            var methodName = callee.Name;
            if (methodName.StartsWith("get_"))
                methodName = methodName.Substring("get_".Length);

            m_Descriptors.TryGetValue(callee.DeclaringType.FullName + "." + methodName, out descriptor);
            if (descriptor != null)
            {
                // by default use descriptor issue description
                description = descriptor.description;
            }
            else
            {
                // Are we trying to warn about a whole namespace?
                m_WholeNamespaceDescriptors.TryGetValue(callee.DeclaringType.Namespace, out descriptor);
                if (descriptor == null)
                    // no issue found
                    return null;

                // use callee name since it's more informative
                description = calleeNode.prettyName;
            }

            return new ProjectIssue
            (
                descriptor,
                description,
                IssueCategory.Code,
                calleeNode
            );
        }

        public IEnumerable<OpCode> GetOpCodes()
        {
            yield return OpCodes.Call;
            yield return OpCodes.Callvirt;
        }
    }
}
