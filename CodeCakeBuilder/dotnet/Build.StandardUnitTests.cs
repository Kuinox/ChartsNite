using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Test;
using Cake.Common.Tools.NUnit;
using CK.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CodeCake
{
    public partial class Build
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalInfo"></param>
        /// <param name="testProjects"></param>
        /// <param name="useNUnit264ForNet461">Will use nunit 264 for Net461 project if true.</param>
        void StandardUnitTests( StandardGlobalInfo globalInfo, IEnumerable<SolutionProject> testProjects )
        {
            string memoryFilePath = $"CodeCakeBuilder/UnitTestsDone.{globalInfo.GitInfo.CommitSha}.txt";

            void WriteTestDone( Cake.Core.IO.FilePath test )
            {
                if( globalInfo.GitInfo.IsValid ) File.AppendAllLines( memoryFilePath, new[] { test.ToString() } );
            }

            bool CheckTestDone( Cake.Core.IO.FilePath test )
            {
                bool done = File.Exists( memoryFilePath )
                            ? File.ReadAllLines( memoryFilePath ).Contains( test.ToString() )
                            : false;
                if( done )
                {
                    if( !globalInfo.GitInfo.IsValid )
                    {
                        Cake.Information( "Dirty commit: tests are run again (base commit tests were successful)." );
                        done = false;
                    }
                    else
                    {
                        Cake.Information( "Test already successful on this commit." );
                    }
                }
                return done;
            }

            foreach( SolutionProject project in testProjects )
            {
                NormalizedPath projectPath = project.Path.GetDirectory().FullPath;
                NormalizedPath binDir = projectPath.AppendPart( "bin" ).AppendPart( globalInfo.BuildConfiguration );
                foreach( NormalizedPath buildDir in Directory.GetDirectories( binDir ) )
                {
                    string framework = buildDir.LastPart;
                    string fileWithoutExtension = buildDir.AppendPart( project.Name );
                    string testBinariesPath = fileWithoutExtension + ".exe"; ;
                    if( File.Exists( testBinariesPath ) )
                    {
                        //we are with nunitLite
                        Cake.Information( $"Testing via NUnitLite ({framework}): {testBinariesPath}" );
                        if( CheckTestDone( testBinariesPath ) ) return;
                        Cake.DotNetCoreExecute( testBinariesPath );
                    }
                    else
                    {
                        testBinariesPath = fileWithoutExtension + ".dll";
                        //VS Tests
                        Cake.Information( $"Testing via VSTest ({framework}): {testBinariesPath}" );
                        if( CheckTestDone( testBinariesPath ) ) return;
                        Cake.DotNetCoreTest( projectPath, new DotNetCoreTestSettings()
                        {
                            Configuration = globalInfo.BuildConfiguration,
                            Framework = framework,
                            NoRestore = true,
                            NoBuild = true,
                            Logger = "trx"
                        } );
                    }
                    WriteTestDone( testBinariesPath );
                }
            }
        }
    }
}
