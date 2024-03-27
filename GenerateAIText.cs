using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon.Util;
using System.Text.Json.Nodes;
using Amazon;

namespace DockerScan
{
    public class GenerateAIText
    {
        /// <summary>
        /// Asynchronously invokes the Anthropic Claude 2 model to run an inference based on the provided input.
        /// </summary>
        /// <param name="prompt">The prompt that you want Claude to complete.</param>
        /// <returns>The inference response from the model</returns>
        /// <remarks>
        /// The different model providers have individual request and response formats.
        /// For the format, ranges, and default values for Anthropic Claude, refer to:
        ///     https://docs.aws.amazon.com/bedrock/latest/userguide/model-parameters-claude.html
        /// </remarks>
        public static async Task<string> InvokeClaudeAsync(string prompt)
        {
            const string claudeModelId = "anthropic.claude-v2";

            // Claude requires you to enclose the prompt as follows:
            var enclosedPrompt = $"Human: Rewrite the following paragraphs to make it more nuance, easy to read and strongly obey given/when/then format.\n\n{prompt}\n\nAssistant:";
            var generatedText = "";

            using AmazonBedrockRuntimeClient client = new(RegionEndpoint.USEast1);
            var payload = new JsonObject {
                { "prompt", enclosedPrompt },
                { "max_tokens_to_sample", 200 },
                { "temperature", 0.5 },
                { "stop_sequences", new JsonArray("\n\nHuman:") }
            }.ToJsonString();
            
            try
            {
                var response = await client.InvokeModelAsync(new InvokeModelRequest {
                    ModelId = claudeModelId,
                    Body = AWSSDKUtils.GenerateMemoryStreamFromString(payload),
                    ContentType = "application/json",
                    Accept = "application/json"
                });

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return JsonNode.ParseAsync(response.Body).Result?["completion"]?.GetValue<string>() ?? "";
                
                else
                    Console.WriteLine("InvokeModelAsync failed with status code " + response.HttpStatusCode);
            }
            catch (AmazonBedrockRuntimeException e)
            {
                var defaultColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(@"Booyahhhhh! => AWS Bedrock \(^o^)/");
                Console.ForegroundColor = Console.BackgroundColor;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = defaultColor;
            }
            
            return string.IsNullOrWhiteSpace(generatedText) ? prompt : generatedText;
        }



    }
}
