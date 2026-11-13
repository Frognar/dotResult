namespace DotResult.CompilerContractTests;

/// <summary>
/// Wraps members in a consumer file that imports DotResult the documented way.
/// </summary>
internal static class Consumer
{
    public static string File(string members, string inNamespace = "Consumer") => $$"""
          using System;
          using System.Threading.Tasks;
          using DotResult;
          using static DotResult.Prelude;

          namespace {{inNamespace}};

          public static class Sample
          {
          {{members}}
          }
          """;
}
