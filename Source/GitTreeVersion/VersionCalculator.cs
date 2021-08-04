﻿using System.Text.RegularExpressions;
using GitTreeVersion.Context;
using GitTreeVersion.Paths;
using GitTreeVersion.VersionStrategies;
using Semver;

namespace GitTreeVersion
{
    public class VersionCalculator
    {
        public SemVersion GetVersion(FileGraph graph)
        {
            return GetVersion(graph, graph.VersionRootPath);
        }

        public SemVersion GetVersion(FileGraph graph, AbsoluteDirectoryPath versionRootPath)
        {
            IVersionConfiguration versionConfiguration = new SemanticVersioningVersionConfiguration();

            if (graph.VersionRootConfigs[versionRootPath].Mode == VersionMode.CalendarVersion)
            {
                versionConfiguration = new CalendarVersioningVersionConfiguration();
            }

            var relevantPaths = graph.GetRelevantPathsForVersionRoot(versionRootPath);
            var prerelease = GetPrerelease(graph, versionRootPath, relevantPaths);

            var majorComponent = versionConfiguration.Major.GetVersionComponent(versionRootPath, relevantPaths, null);
            var minorComponent = versionConfiguration.Minor.GetVersionComponent(versionRootPath, relevantPaths, majorComponent.Range);
            var patchComponent = versionConfiguration.Patch.GetVersionComponent(versionRootPath, relevantPaths, minorComponent.Range);

            return new SemVersion(majorComponent.Version, minorComponent.Version, patchComponent.Version, prerelease);
        }

        private static string? GetPrerelease(FileGraph graph, AbsoluteDirectoryPath versionRootPath, AbsoluteDirectoryPath[] relevantPaths)
        {
            var prerelease = graph.BuildEnvironment?.GetPrerelease(versionRootPath, relevantPaths);

            if (!string.IsNullOrWhiteSpace(prerelease))
            {
                return prerelease;
            }

            if (graph.CurrentBranch is { IsDetached: false } branch)
            {
                return branch.Name switch
                {
                    "main" => null,
                    "master" => null,
                    var name => SanitizeBranchName(name),
                };
            }

            return null;
        }

        private static string SanitizeBranchName(string branchName)
        {
            branchName = Regex.Replace(branchName, @"[^a-zA-Z0-9.-]", "-");
            while (branchName.Contains("--"))
            {
                branchName = branchName.Replace("--", "-");
            }
            return branchName.Trim();
        }
    }
}
