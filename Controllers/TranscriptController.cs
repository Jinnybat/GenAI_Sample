using Microsoft.AspNetCore.Mvc;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;


namespace GenAI_Sample.Controllers;

public class TranscriptController : Controller
{
    private readonly IOpenAIService openAIService;

    public TranscriptController(IOpenAIService openAIService)
    {
        this.openAIService = openAIService;
    }

    [HttpPost]
    public async Task<JsonResult> Sentiment([FromBody] Request request)
    {
        try
        {
            string prompt=
            $"""
            Create bulletpoints with heading 'Major Points' from this '{request.Message}'
            include place, time or any specific thing discussed in the message
            and overall sentiment of the text is Positive or Negative with heading 'Overall Sentiment'
            """;

            var result = await openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Model = OpenAI.ObjectModels.Models.Gpt_3_5_Turbo,
                Temperature = 02f,
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromUser(prompt)
                }
            });

            if(!result.Successful)
            {
                return Json("Error in communicating with OpenAI api");
            }
            return Json(result.Choices.FirstOrDefault()?.Message.Content??"");
        }
        catch (System.Exception)
        {
            return Json("Error in constructing OpenAI request");
        }

        // await Task.CompletedTask;
        // return Json($"This is a sample output ! your message was {request.Message}");
    }
}

public class Request
{
    public string? Message{get;set;}
}