// See https://aka.ms/new-console-template for more information
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerScan;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

try
{
    using var client = new DockerClientConfiguration(new Uri(ResolveDockerRegistryUrl())).CreateClient();
    await ListAndInspectImages(client);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

static string ResolveDockerRegistryUrl()
{
    var platform = Environment.OSVersion.Platform;

        // Check if the platform is Windows
        return platform == PlatformID.Win32NT || platform == PlatformID.Win32S ||
               platform == PlatformID.Win32Windows || platform == PlatformID.WinCE
               ? "npipe://./pipe/docker_engine"
               : "unix:///var/run/docker.sock";
}

static async Task ListAndInspectImages(IDockerClient client)
{
    var config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();
    
    var images = await client.Images.ListImagesAsync(new ImagesListParameters { All = true });
    var defaultColor = Console.ForegroundColor;
    var jiraManager = new JiraManager(config);

    foreach (var image in images)
    {
        var inspectResponse = await client.Images.InspectImageAsync(image.ID);
        var labels = inspectResponse.Config.Labels;

        // Check if the image has a label indicating it's signed
        if (inspectResponse.RepoDigests?.Count == 0) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{image.RepoTags[0]} is not signed.");
            await jiraManager.Post(image.RepoTags[0]).ConfigureAwait(false);
        }
        else {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(JsonConvert.SerializeObject(inspectResponse.RepoDigests));
        }
        Console.ForegroundColor = defaultColor;
    }
}
