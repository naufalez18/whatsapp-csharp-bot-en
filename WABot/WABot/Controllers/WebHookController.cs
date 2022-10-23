using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WABot.Api;
using WABot.Helpers.Json;

namespace WABot.Controllers
{
    /// <summary>
    /// Controller for processing requests coming from chat-api.com
    /// </summary>
    [ApiController]
    [Route("/")]
    public class WebHookController : ControllerBase
    {
        /// <summary>
        /// A static object that represents the API for a given controller.
        /// </summary>
        private static readonly WaApi api = new WaApi("https://eu115.chat-api.com/instance12345/", "123456789token");
        private static readonly string welcomeMessage = "Bot's menu: \n" +
                                                        "1. chatid - Get chatid\n" +
                                                        "2. file doc/gif,jpg,png,pdf,mp3,mp4 - Get a file in the desired format\n" +
                                                        "3. ogg - Get a voice message\n" +
                                                        "4. geo - Get the geolocation\n" +
                                                        "5. group - Create a group with a bot";

        /// <summary>
        /// Handler of post requests received from chat-api
        /// </summary>
        /// <param name="data">Serialized json object</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> Post(Answer data)
        {
            foreach (var message in data.Messages)
            {
                if (message.FromMe)
                    continue;

                switch (message.Body.Split()[0].ToLower())
                {
                    case "Biller":
                        "Update database function Here"
                        return await api.SendMessage(message.ChatId, $"Your ID: {message.ChatId}");
                    case "file":
                        var texts = message.Body.Split();
                        if (texts.Length > 1)
                            return await api.SendFile(message.ChatId, texts[1]);
                        break;
                    case "ogg":
                        return await api.SendOgg(message.ChatId);
                    case "geo":
                        return await api.SendGeo(message.ChatId);
                    case "group":
                        return await api.CreateGroup(message.Author);
                    default:
                        return await api.SendMessage(message.ChatId, welcomeMessage);
                }             
            }
            return "";          
        }  
    }
}
