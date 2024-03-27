using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DockerScan
{
    public class JiraManager {

        public async Task Post(string dockerImaegeName) {

            Console.WriteLine("Started:");

            string jiraBaseUrl = "https://classltd.atlassian.net";
            string username = "tala.rajabi@class.com.au";
            string apiToken = "ATATT3xFfGF0hbSPu0HbhQNI0DcIxa6QA2GEBVd0wV81SUsxoYzC8p7P_u4m8MW1YfKhRKjsT6fKPNXjD0Lf1AIvmaz-lOFIp4EwJuqpBgfyFw6JE8YfekG1RkGG24IczLo--8_4HcemsZyXjA85GMys_1Wl9r5V0E6lLydkUnOGxe-VQ2XrOSU=16E9C057";

            JiraIssue newIssue = new JiraIssue
            {
                ProjectKey = "CB",
                Summary = $"Docker Image {dockerImaegeName} Not Signed",
                DescriptionType = "doc",
                Text = $"As a system owner, I would like all Docker images to be signed. This way, I can ensure that all images we're deploying originate from a trusted source and haven't been tampered with. Without signing, there's a risk of deploying malicious or compromised images, which can lead to serious security breaches and compromise the integrity of your infrastructure. By signing Docker images, you establish a chain of trust, which is crucial for maintaining the security of your containerised applications and environments.\r\n\r\nGiven the critical importance of maintaining the integrity and security of our containerised applications,\r\n\r\nWhen {dockerImaegeName} is created,\r\n\r\nThen, it is imperative to implement image signing to ensure that images originate from trusted sources and have not been tampered wit"
            };


            try
            {
                string response = await newIssue.CreateStoryAsync(jiraBaseUrl, username, apiToken);
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

        public async Task<string> CreateStoryAsync(string jiraBaseUrl, string username, string apiToken)
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


            string jsonIssue = JsonSerializer.Serialize(newIssue);
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
