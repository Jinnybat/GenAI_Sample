using System.Net;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;


namespace GenAI_Sample.Controllers;

public class TranscriptController : Controller
{
    private readonly IConfiguration configuration;
    private readonly IOpenAIService openAIService;
    private readonly OpenAIClient client;

    public TranscriptController(IConfiguration configuration, IOpenAIService openAIService, OpenAIClient client)
    {
        this.configuration = configuration;
        this.openAIService = openAIService;
        this.client = client;
    }

    [HttpPost]
    public async Task<JsonResult> Sentiment([FromBody] Request request)
    {
        try
        {
            // var client = new OpenAIClient(configuration.GetSection("OpenAI:ApiKey").Value!);

            var chatCompletion = new ChatCompletionsOptions()
            {
                Temperature = 0.2f,
                DeploymentName = "gpt-3.5-turbo-16k",
                Messages =
                {
                    // new ChatRequestSystemMessage("You are a beautiful assitant. You will talk like a data analyst"),
                    new ChatRequestUserMessage("You are a data analyst and you will translate user message in english if is not and then generate two headings, first one is 'Major Points' which have summary with at minimum five and maximum ten bullet points  and second heading 'Overall Sentiments' have only one word 'Positve Or Negative'"),
                    new ChatRequestUserMessage(request.Message),
                }
            };
            Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletion);
            ChatResponseMessage? responseMessage = response.Value.Choices.FirstOrDefault()?.Message;
            var msg = responseMessage?.Content.ToString();

            return Json(msg);
        }
        catch (Exception ex)
        {
            return Json("Error in constructing OpenAI request");
        }
    }
    [HttpPost]
    [NonAction]
    public async Task<JsonResult> GenerateImage([FromBody] Request request)
    {
        try
        {
            var client = new OpenAIClient(configuration.GetSection("OpenAI:ApiKey").Value!);

            Response<ImageGenerations> response = await client.GetImageGenerationsAsync(
                new ImageGenerationOptions()
                {
                    // DeploymentName = request.Message,
                    DeploymentName = "dall-e-3",
                    Prompt = "a happy monkey eating a banana, in watercolor",
                    Size = ImageSize.Size1024x1024,
                    Quality = ImageGenerationQuality.Standard
                });

            ImageGenerationData generatedImage = response.Value.Data[0];
            // if (!string.IsNullOrEmpty(generatedImage.RevisedPrompt))
            // {
            //     Console.WriteLine($"Input prompt automatically revised to: {generatedImage.RevisedPrompt}");
            // }
            // Console.WriteLine($"Generated image available at: {generatedImage.Url.AbsoluteUri}");

            return Json($"Generated image available at: {generatedImage.Url.AbsoluteUri}");
        }
        catch (Exception ex)
        {
            return Json("Error in constructing OpenAI request");
        }
    }

    [HttpPost]
    [NonAction]
    public async Task<JsonResult> BetalgoSentiment([FromBody] Request request)
    {
        try
        {
            string prompt =
            $"""
                    Translate in english if not and then generate two headings, first one is 'Major Points' which have summary in bullet points and second one is 'Overall Sentiments' Positive or Negative only
                """;

            var result = await openAIService.ChatCompletion.CreateCompletion(
                new ChatCompletionCreateRequest
                {
                    Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo_16k,
                    Temperature = 02f,
                    Messages = new List<ChatMessage>
                    {
                            ChatMessage.FromSystem(prompt),
                            ChatMessage.FromUser(request.Message)
                    }
                });

            if (!result.Successful)
            {
                if (result.HttpStatusCode == HttpStatusCode.Unauthorized)
                {
                    return Json("Request is unauthorized");
                }
                return Json("Error in communicating with OpenAI api");
            }
            return Json(result.Choices.FirstOrDefault()?.Message.Content ?? "");
        }
        catch (Exception ex)
        {
            return Json("Error in constructing OpenAI request");
        }
    }
}
public class Request
{
    public string? Message { get; set; }
}