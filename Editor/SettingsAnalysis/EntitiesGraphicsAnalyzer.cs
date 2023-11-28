using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.ProjectAuditor.Editor.Core;
using Unity.ProjectAuditor.Editor.Diagnostic;
using Unity.ProjectAuditor.Editor.Interfaces;
using Unity.ProjectAuditor.Editor.Modules;
using Unity.ProjectAuditor.Editor.Utils;

namespace Unity.ProjectAuditor.Editor.SettingsAnalysis
{
    class EntitiesGraphicsAnalyzer : ISettingsModuleAnalyzer
    {
        internal const string PAS1000 = nameof(PAS1000);
        internal const string PAS1013 = nameof(PAS1013);

        static BuildTarget s_Platform = BuildTarget.NoTarget;

        // Legacy: The Hybrid Renderer was replaced by Entities Graphics when Entities 0.51 was released in mid-2022.
        static readonly Descriptor k_HybridDescriptor = new Descriptor(
            PAS1000,
            "Player Settings: Static batching is enabled",
            Areas.CPU,
            "<b>Static Batching</b> is enabled in Player Settings and the package com.unity.rendering.hybrid is installed. Static batching is incompatible with the batching techniques used in the Hybrid Renderer and Scriptable Render Pipeline, and will result in poor rendering performance and excessive memory use.",
            "Disable static batching in Player Settings.")
        {
            fixer = (issue, analysisParams) =>
            {
                PlayerSettingsUtil.SetStaticBatchingEnabled(s_Platform, false);
            }
        };

        static readonly Descriptor k_EntitiesGraphicsDescriptor = new Descriptor(
            PAS1013,
            "Player Settings: Static batching is enabled",
            Areas.CPU,
            "<b>Static Batching</b> is enabled in Player Settings and the package com.unity.entities.graphics is installed. Static batching is incompatible with the batching techniques used in Entities Graphics and the Scriptable Render Pipeline, and will result in poor rendering performance and excessive memory use.",
            "Disable static batching in Player Settings.")
        {
            fixer = (issue, analysisParams) =>
            {
                PlayerSettingsUtil.SetStaticBatchingEnabled(s_Platform, false);
            }
        };

        public void Initialize(Module module)
        {
            module.RegisterDescriptor(k_HybridDescriptor);
            module.RegisterDescriptor(k_EntitiesGraphicsDescriptor);
        }

        public IEnumerable<ProjectIssue> Analyze(SettingsAnalysisContext context)
        {
            s_Platform = context.Params.Platform;

#if PACKAGE_ENTITIES_GRAPHICS
            if (PlayerSettingsUtil.IsStaticBatchingEnabled(context.Params.Platform))
            {
                yield return context.Create(IssueCategory.ProjectSetting, k_EntitiesGraphicsDescriptor.Id);
            }
#elif PACKAGE_HYBRID_RENDERER
            if (PlayerSettingsUtil.IsStaticBatchingEnabled(context.Params.Platform))
            {
                yield return context.Create(IssueCategory.ProjectSetting, k_HybridDescriptor.Id);
            }
#else
            yield break;
#endif
        }
    }
}
