using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

public class SnippetData
{
    public string prefix { get; set; }
    public string description { get; set; }
    public string body { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        string sourceDir = Environment.ExpandEnvironmentVariables(@"~/source/migrateddata/SnippetSource")
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        string dbForgeDest = Environment.ExpandEnvironmentVariables(@"~/source/migrateddata/dbforgesnippets")
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        string dataGripDest = Environment.ExpandEnvironmentVariables(@"~/source/migrateddata/datagripxml/ImportedSnippets.xml")
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        BatchConvertToDbForgeSnippets(sourceDir, dbForgeDest);
        BatchConvertToSingleDataGripHiveSnippet(sourceDir, dataGripDest);
    }

    private static void BatchConvertToDbForgeSnippets(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        var files = Directory.GetFiles(sourceDir, "*.*")
            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
        foreach (var file in files)
        {
            try
            {
                string content = File.ReadAllText(file);
                SnippetData snippetData = file.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    ? JsonConvert.DeserializeObject<SnippetData>(content)
                    : ParseSsmsSnippet(content);

                string snippetContent = CreateSnippetContent(snippetData); // dbForge format
                string destFile = Path.Combine(destDir, Path.GetFileNameWithoutExtension(file) + ".snippet");
                File.WriteAllText(destFile, snippetContent);
                Console.WriteLine($"[dbForge] Converted {file} to {destFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[dbForge] Error processing {file}: {ex.Message}");
            }
        }
    }

    private static void BatchConvertToSingleDataGripHiveSnippet(string sourceDir, string destFile)
    {
        var files = Directory.GetFiles(sourceDir, "*.*")
            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
        var sb = new StringBuilder();
        sb.AppendLine(@"<templateSet group=""UserCreatedGroup"">");

        foreach (var file in files)
        {
            try
            {
                string content = File.ReadAllText(file);
                SnippetData snippetData = file.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                    ? JsonConvert.DeserializeObject<SnippetData>(content)
                    : ParseSsmsSnippet(content);

                sb.AppendLine($@"  <template name=""{System.Security.SecurityElement.Escape(snippetData.prefix)}"" value=""{System.Security.SecurityElement.Escape(snippetData.body)}"" description=""{System.Security.SecurityElement.Escape(snippetData.description)}"" toReformat=""false"" toShortenFQNames=""true"">
    <context>
      <option name=""SQL"" value=""true"" />
    </context>
  </template>");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hive] Error processing {file}: {ex.Message}");
            }
        }

        sb.AppendLine("</templateSet>");
        File.WriteAllText(destFile, sb.ToString());
        Console.WriteLine($"[Hive] All snippets written to {destFile}");
    }

    public static SnippetData ParseSsmsSnippet(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var header = doc.Descendants("Header").FirstOrDefault();
        var snippet = doc.Descendants("Snippet").FirstOrDefault();
        var code = snippet?.Descendants("Code").FirstOrDefault();

        return new SnippetData
        {
            prefix = header?.Element("Title")?.Value ?? "Snippet",
            description = header?.Element("Description")?.Value ?? "",
            body = code?.Value ?? ""
        };
    }

    public static string CreateSnippetContent(SnippetData data)
    {
        return $@"<CodeSnippet Format=""1.0.0"">
  <Header>
    <Title>{System.Security.SecurityElement.Escape(data.prefix)}</Title>
    <Description>{System.Security.SecurityElement.Escape(data.description)}</Description>
  </Header>
  <Snippet>
    <Code Language=""SQL""><![CDATA[{data.body}]]></Code>
  </Snippet>
</CodeSnippet>";
    }

    public static string CreateDataGripSnippetContent(SnippetData data)
    {
        return $@"<templateSet group=""user"">
  <template name=""{System.Security.SecurityElement.Escape(data.prefix)}"" value=""{System.Security.SecurityElement.Escape(data.body)}"" description=""{System.Security.SecurityElement.Escape(data.description)}"" toReformat=""true"" toShortenFQNames=""true"">
    <context>
      <option name=""SQL"" value=""true"" />
    </context>
  </template>
</templateSet>";
    }
}
