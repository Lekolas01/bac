using HeuristicLab.PluginInfrastructure;

namespace EmptyAlgorithm {
  [Plugin("HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction", "1.0")]
  [PluginFile("HeuristicLab.Algorithms.DataAnalysis.FastFunctionExtraction.dll", PluginFileType.Assembly)] // each plugin represents a collection of files. The minimum is one file; the assembly.

  // Usually your plugin references other HeuristicLab dlls. If you are referencing files (e.g. assemblies)
  // from another plugin the corresponding plugin should be added as a dependency.
  // Usually, if this information is incorrect you will still be able to use you plugin, but HL functionality
  // which uses plugin dependency resolution will not work correctly. For instance if plugin dependencies are
  // not correct then your plugin cannot be used on HeuristicLab.Hive
  // 
  [PluginDependency("HeuristicLab.Collections", "3.3")]
  [PluginDependency("HeuristicLab.Common", "3.3")]
  [PluginDependency("HeuristicLab.Core", "3.3")]
  [PluginDependency("HeuristicLab.Data", "3.3")]
  [PluginDependency("HeuristicLab.Encodings.BinaryVectorEncoding", "3.3")]
  [PluginDependency("HeuristicLab.Optimization", "3.3")]
  [PluginDependency("HeuristicLab.Parameters", "3.3")]
  [PluginDependency("HeuristicLab.Persistence", "3.3")]
  [PluginDependency("HeuristicLab.Problems.Binary", "3.3")]
  [PluginDependency("HeuristicLab.Random", "3.3")]

  // HL plugin infrastructure discovers plugins on startup by trying to load all .dll and .exe files and looking for 
  // classes deriving from PluginBase. The meta-information for the plugin class is specified in the attributes
  // above, and used by plugin infrastructure primarily for plugin dependency resolution.

  // Steps:
  // (1) Check out HL source code (e.g. the trunk version)
  // (2) Build external libraries HeuristicLab.ExtLibs.sln using the Build.cmd (in the path of the HL source code)
  // (3) Build HeuristicLab 3.3.sln using the Build.cmd
  // (4) Build this project. The output path for binaries is set to "".
  //     this assumes you have the following folder structure
  //    <ROOT>
  //    |..hl
  //       |..branches
  //       |  |..Templates
  //       |     |..EmptyPlugin
  //       |..trunk
  //          |.. bin 

  //  (5) Check that the output file has been added to the HL binaries folder (hl/trunk/bin/EmptyPlugin.dll)
  //  (6) Start hl/trunk/bin/HeuristicLab.exe and open the "Plugin Manager".
  //      Make sure your EmptyPlugin appears in the list of loaded plugins 
  public class Plugin : PluginBase {
  }
}
