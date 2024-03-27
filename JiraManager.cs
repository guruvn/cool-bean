// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace DockerScan
{
    public class JiraManager
    {
        private readonly IConfigurationRoot _configuration;

        public JiraManager(IConfigurationRoot configuration)
            => _configuration = configuration;

        public async Task Post(string dockerImageName) {

            Console.WriteLine("Started:");

            const string jiraBaseUrl = "https://classltd.atlassian.net";

            var inputText = "*As a system owner*, I would like all Docker images to be signed. This way, I can ensure that all images we're deploying originate from a trusted source and haven't been tampered with. Without signing, there's a risk of deploying malicious or compromised images, which can lead to serious security breaches and compromise the integrity of your infrastructure. By signing Docker images, you establish a chain of trust, which is crucial for maintaining the security of your containerised applications and environments.\n\n\n" 
                            + "*Given* the critical importance of maintaining the integrity and security of our containerised applications.\n"
                            + $"*When* {dockerImageName} is created.\n"
                            + "*Then*, it is imperative to implement image signing to ensure that images originate from trusted sources and have not been tampered with.\n";
            var aiGeneratedText = await GenerateAIText.InvokeClaudeAsync(inputText).ConfigureAwait(false);

            var newIssue = new JiraIssue {
                ProjectKey = "CB",
                Summary = $"Docker Image {dockerImageName} Not Signed",
                DescriptionType = "doc",
                Text = aiGeneratedText
            };
            
            try
            {
                var response = await newIssue.CreateStoryAsync(
                    jiraBaseUrl, 
                    _configuration["jira:username"], 
                    _configuration["jira:token"]);
                Console.WriteLine("Issue created successfully. Response:");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating issue: {ex.Message}");
            }

            Console.WriteLine("Finished");
        }
    }

    public class JiraIssue
    {
        public required string ProjectKey { get; set; }
        public required string Summary { get; set; }
        public required string DescriptionType { get; set; }
        public required string Text { get; set; }

        public async Task<string> CreateStoryAsync(string jiraBaseUrl, string? username, string? apiToken)
        {
            string url = $"{jiraBaseUrl}/rest/api/3/issue";
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{apiToken}"));

            JiraIssueRequest newIssue = new JiraIssueRequest
            {
                fields = new Fields
                {
                    project = new Project { key = ProjectKey },
                    summary = Summary,
                    description = new Description
                    {
                        type = DescriptionType,
                        version = 1,
                        content = new List<Content>
                    {
                        new Content
                        {
                            type = "paragraph",
                            content = new List<InnerContent>
                            {
                                new InnerContent
                                {
                                    type = "text",
                                    text = Text
                                }
                            }
                        }
                    }
                    },
                    issuetype = new Issuetype { name = "Story" },
                    priority = new Priority { name = "High" },
                    labels = new List<string> { "label1", "label2" }
                }
            };


            var jsonIssue = JsonSerializer.Serialize(newIssue);
            var content = new StringContent(jsonIssue, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create issue. Status code: {response.StatusCode}. Error: {errorMessage}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
    }

    public class JiraIssueRequest
    {
        public Fields fields { get; set; }
    }

    public class Fields
    {
        public Project project { get; set; }
        public string summary { get; set; }
        public Description description { get; set; }
        public Issuetype issuetype { get; set; }
        public Priority priority { get; set; }
        public List<string> labels { get; set; }
    }

    public class Project
    {
        public string key { get; set; }
    }

    public class Description
    {
        public string type { get; set; }
        public int version { get; set; }
        public List<Content> content { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public List<InnerContent> content { get; set; }
    }

    public class InnerContent
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class Issuetype
    {
        public string name { get; set; }
    }

    public class Priority
    {
        public string name { get; set; }
    }

    

}