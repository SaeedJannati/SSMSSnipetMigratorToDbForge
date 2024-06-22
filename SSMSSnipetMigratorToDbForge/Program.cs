
using Newtonsoft.Json;


// Specify the directory containing JSON files
string jsonDirectoryPath = @"D:\Source\Snippets";

// Check if directory exists
if (!Directory.Exists(jsonDirectoryPath))
{
    Console.WriteLine($"Directory does not exist: {jsonDirectoryPath}");
    return;
}

// Get all JSON files in the directory
string[] jsonFiles = Directory.GetFiles(jsonDirectoryPath, "*.json");

// Process each JSON file
foreach (var jsonFile in jsonFiles)
{
    try
    {
        // Read the JSON file
        string jsonContent = File.ReadAllText(jsonFile);

        // Deserialize the JSON content
        var snippetData = JsonConvert.DeserializeObject<SnippetData>(jsonContent);

        // Create the .snippet content
        string snippetContent = CreateSnippetContent(snippetData);

        // Define the .snippet file path
        string snippetFilePath = Path.Combine(jsonDirectoryPath, Path.GetFileNameWithoutExtension(jsonFile) + ".snippet");

        // Write the .snippet file
        File.WriteAllText(snippetFilePath, snippetContent);

        Console.WriteLine($"Converted {jsonFile} to {snippetFilePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing {jsonFile}: {ex.Message}");
    }
}


static string CreateSnippetContent(SnippetData data)
{
    return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<CodeSnippets>
  <CodeSnippet Format=""1.0.0"">
    <Header>
      <Title>{data.prefix}</Title>
      <Shortcut>{data.prefix.ToLower().Replace(' ', '_')}</Shortcut>
      <Description>{data.description}</Description>
      <Author>Saeed Jannati</Author>
      <SnippetTypes>
        <SnippetType>Expansion</SnippetType>
      </SnippetTypes>
    </Header>
    <Snippet>
      <Declarations>
        <Literal>
          <ID>schema</ID>
          <Default>dbo</Default>
        </Literal>
        <Literal>
          <ID>table_name</ID>
          <Default>table_name</Default>
        </Literal>
        <Literal>
          <ID>column_name</ID>
          <Default>column_name</Default>
        </Literal>
        <Literal>
          <ID>data_type</ID>
          <Default>VARCHAR(20)</Default>
        </Literal>
        <Literal>
          <ID>null</ID>
          <Default>NULL</Default>
        </Literal>
        <Literal>
          <ID>collate</ID>
          <Default>Latin1_General_CI_AS</Default>
        </Literal>
        <Literal>
          <ID>default</ID>
          <Default>NULL</Default>
        </Literal>
      </Declarations>
      <Code Language=""SQL (Devart)"" Kind=""MS SQL""><![CDATA[{data.body}]]></Code>
    </Snippet>
  </CodeSnippet>
</CodeSnippets>";

}

public class SnippetData
{
    public string prefix { get; set; }
    public string body { get; set; }
    public string description { get; set; }
}

