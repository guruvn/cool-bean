// See https://aka.ms/new-console-template for more information
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerScan;
using Newtonsoft.Json;

JiraManager jiraManager = new JiraManager();
await jiraManager.Post();

const string registryAddress = "unix:///var/run/docker.sock";;

try
{
    using var client = new DockerClientConfiguration(new Uri(registryAddress)).CreateClient();
    await ListAndInspectImages(client);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

static async Task ListAndInspectImages(IDockerClient client)
{
    var images = await client.Images.ListImagesAsync(new ImagesListParameters { All = true });
    var defaultColor = Console.ForegroundColor;

    foreach (var image in images)
    {
        var inspectResponse = await client.Images.InspectImageAsync(image.ID);
        var labels = inspectResponse.Config.Labels;

        // Check if the image has a label indicating it's signed
        if (inspectResponse.RepoDigests?.Count == 0) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{image.RepoTags[0]} is not signed.");
        }
        else {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(JsonConvert.SerializeObject(inspectResponse.RepoDigests));
        }
        Console.ForegroundColor = defaultColor;
    }
}
