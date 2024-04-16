using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Diagnostics;

using System.IO;
using System.Linq;

public static class SwiftPostProcessor
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            var projPath = buildPath + "/Unity-Iphone.xcodeproj/project.pbxproj";
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var targetGuid = proj.GetUnityMainTargetGuid();//TargetGuidByName(PBXProject.GetUnityTestTargetName());
            
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/iOSVision/Source/iOSVision-Bridging-Header.h");

            proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "iOSVision-Swift.h");

            proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(PROJECT_DIR)/lib/$(CONFIGURATION) $(inherited)");
            proj.AddBuildProperty(targetGuid, "FRAMERWORK_SEARCH_PATHS",
                "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks");
            proj.AddBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            proj.AddBuildProperty(targetGuid, "DYLIB_INSTALL_NAME_BASE", "@rpath");
            proj.AddBuildProperty(targetGuid, "LD_DYLIB_INSTALL_NAME",
                "@executable_path/../Frameworks/$(EXECUTABLE_PATH)");
            proj.AddBuildProperty(targetGuid, "DEFINES_MODULE", "YES");
            proj.AddBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            proj.AddBuildProperty(targetGuid, "COREML_CODEGEN_LANGUAGE", "None");

            proj.AddBuildProperty(proj.TargetGuidByName("GameAssembly"), "COREML_CODEGEN_LANGUAGE", "None");

            string rootFolder = "/Assets";//"/Packages";
            var unityFrameworkGuid = proj.GetUnityFrameworkTargetGuid();

            var resourceTarget = proj.GetResourcesBuildPhaseByTarget(unityFrameworkGuid);
            var ModelPath = Directory.GetCurrentDirectory() + rootFolder + "/Plugins/iOS/iOSVision/Source/FastSAM-s.mlpackage";
            var xcodeTarget = "/Libraries/Plugins/iOS/iOSVision/Source/FastSAM-s.mlpackage";
            var copyTarget = buildPath + xcodeTarget;
            
            CopyDirectory(ModelPath, copyTarget, true);

            var fileGUID = proj.AddFile(copyTarget, xcodeTarget);
            
            proj.RemoveFileFromBuild(unityFrameworkGuid, fileGUID);
            proj.AddFileToBuildSection(unityFrameworkGuid, resourceTarget, fileGUID);

            proj.WriteToFile(projPath);
        }
    }
    
    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        if(Directory.Exists(destinationDir))
        {
            Directory.Delete(destinationDir, true);
        }

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
