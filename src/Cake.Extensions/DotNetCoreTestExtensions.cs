﻿namespace Cake.Extensions
{
    using System;
    using System.Linq;
    using Common.Tools.DotNetCore;
    using Common.Tools.DotNetCore.Test;
    using Common.Tools.XUnit;
    using Core;
    using Core.Annotations;
    using Core.IO;

    public static class DotNetCoreTestExtensions
    {
        /// <summary>
        /// Runs DotNetCoreTest using the given Xunit Settings
        /// </summary>
        /// <param name="context">The Cake Context</param>
        /// <param name="project">DotNetCore Test Project Path</param>
        /// <param name="xunitSettings">XUnit2 DotNetCore Test Settings Configurer</param>
        /// <returns></returns>
        [CakeMethodAlias]
        public static void DotNetCoreTest(
            this ICakeContext context,
            FilePath project,
            XUnit2Settings xunitSettings)
        {
            DotNetCoreTest(context, new DotNetCoreTestSettings(), project, xunitSettings);
        }


        /// <summary>
        /// Appends XUnit2Settings to DotNetCoreTestSettings instance
        /// </summary>
        /// <param name="context">The Cake Context</param>
        /// <param name="settings">DotNetCore Test Settings</param>
        /// <param name="project">DotNetCore Test Project Path</param>
        /// <param name="xunitSettings">XUnit2 DotNetCore Test Settings Configurer</param>
        /// <returns></returns>
        [CakeMethodAlias]
        public static void DotNetCoreTest(
            this ICakeContext context,            
            DotNetCoreTestSettings settings,
            FilePath project,
            XUnit2Settings xunitSettings)
        {
            settings.ArgumentCustomization = args => ProcessArguments(context, args, project, xunitSettings);
            context.DotNetCoreTest(project.FullPath, settings);
        }

        private static ProcessArgumentBuilder ProcessArguments(
            ICakeContext cakeContext,
            ProcessArgumentBuilder builder,
            FilePath project, 
            XUnit2Settings settings)
        {
            // No shadow copy?
            if (!settings.ShadowCopy)
            {
                throw new CakeException("-noshadow is not supported in .netcoreapp");
            }

            // No app domain?
            if (settings.NoAppDomain)
            {
                throw new CakeException("-noappdomain is not supported in .netcoreapp");
            }

            // Generate NUnit Style XML report?
            if (settings.NUnitReport)
            {
                var reportFileName = new FilePath(project.GetDirectory().GetDirectoryName());                
                var assemblyFilename = reportFileName.AppendExtension(".xml");
                var outputPath = settings.OutputDirectory.MakeAbsolute(cakeContext.Environment).GetFilePath(assemblyFilename);

                builder.Append("-nunit");
                builder.AppendQuoted(outputPath.FullPath);
            }

            // Generate HTML report?
            if (settings.HtmlReport)
            {
                var reportFileName = new FilePath(project.GetDirectory().GetDirectoryName());
                var assemblyFilename = reportFileName.AppendExtension(".html");
                var outputPath = settings.OutputDirectory.MakeAbsolute(cakeContext.Environment).GetFilePath(assemblyFilename);

                builder.Append("-html");
                builder.AppendQuoted(outputPath.FullPath);
            }

            if (settings.XmlReportV1)
            {
                throw new CakeException("-xmlv1 is not supported in .netcoreapp");
            }

            // Generate XML report?
            if (settings.XmlReport)
            {
                var reportFileName = new FilePath(project.GetDirectory().GetDirectoryName());
                var assemblyFilename = reportFileName.AppendExtension(".xml");
                var outputPath = settings.OutputDirectory.MakeAbsolute(cakeContext.Environment).GetFilePath(assemblyFilename);

                builder.Append("-xml");
                builder.AppendQuoted(outputPath.FullPath);
            }

            // parallelize test execution?
            if (settings.Parallelism != ParallelismOption.None)
            {
                builder.Append("-parallel " + settings.Parallelism.ToString().ToLowerInvariant());
            }

            // max thread count for collection parallelization
            if (settings.MaxThreads.HasValue)
            {
                if (settings.MaxThreads.Value == 0)
                {
                    builder.Append("-maxthreads unlimited");
                }
                else
                {
                    builder.Append("-maxthreads " + settings.MaxThreads.Value);
                }
            }

            foreach (var trait in settings.TraitsToInclude
                .SelectMany(pair => pair.Value.Select(v => new { Name = pair.Key, Value = v })))
            {
                builder.Append("-trait \"{0}={1}\"", trait.Name, trait.Value);
            }

            foreach (var trait in settings.TraitsToExclude
                .SelectMany(pair => pair.Value.Select(v => new { Name = pair.Key, Value = v })))
            {
                builder.Append("-notrait \"{0}={1}\"", trait.Name, trait.Value);
            }

            return builder;
        }
    }
}
