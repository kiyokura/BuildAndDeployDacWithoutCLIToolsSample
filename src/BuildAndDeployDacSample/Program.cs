using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;
using Microsoft.SqlServer.Dac;
using System;
using System.IO;
using System.Reflection;

namespace BuildAndDeployDacSample
{
  class Program
  {
    static void Main()
    {
      var SolutionPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\..\");

      // -------------------------------------------------------------
      // Build "ConsoleApp project" using Microsoft.Build.Evaluation
      // -------------------------------------------------------------
      //   Need "Microsot.Build" and "Microsoft.Build.Tasks.Core" nuget package to build csproj.
      var p1Path = Path.Combine(SolutionPath, @"MyClassLib\MyClassLib.csproj");
      var p1 = ProjectCollection.GlobalProjectCollection.LoadProject(p1Path);
      var p1Result = p1.Build(new[] { new ConsoleLogger() });
      Console.WriteLine($"p1 result : {p1Result}");

      // -------------------------------------------------------------
      // Build "SQL Database project" to create dacpac using Microsoft.Build.Evaluation
      // -------------------------------------------------------------
      // Need "Microsoft.SqlServer.DACFx" nuget package additional to build sqlproj.
      var p2Path = Path.Combine(SolutionPath, @"MyDatabase\MyDatabase.sqlproj");
      var p2 = ProjectCollection.GlobalProjectCollection.LoadProject(p2Path);
      p2.SetProperty("OutputPath", @"C:\hoge\");
      p2.SetProperty("SqlTargetName", "output");
      var p2Result = p2.Build(new[] { new ConsoleLogger() });
      Console.WriteLine($"p2 result : {p2Result}");

      // -------------------------------------------------------------
      // Create LocalDbInstance
      // -------------------------------------------------------------
      // Need "MartinCostello.SqlLocalDb" nuget package and Installing SQL Server LocalDB.
      LocalDbUtility.CreateInstance("TestDbInstance", "13.0", true);
      LocalDbUtility.CreateDatabase("TestDbInstance", "MyTestDB");
      var connectionString = LocalDbUtility.GetConnectionString("TestDbInstance", "MyTestDB");

      // -------------------------------------------------------------
      // Deploy dacpac to LocalDb
      // -------------------------------------------------------------
      // Need "Microsoft.SqlServer.DACFx" nuget package
      var dac = new DacServices(connectionString);
      var dacpac = DacPackage.Load(@"C:\hoge\output.dacpac");
      dac.Deploy(dacpac, "MyTestDB", true);
    }
  }
}
