### Introduction

In this guide, we will tell you how to create a WhatsApp bot with C# by using our API WhatsApp gateway.

The bot will receive commands in the form of regular WhatsApp messages and respond to them. A test bot’s functionality will be limited to the following features:

*   Showing a welcome message in response to commands the bot doesn’t have and displaying the bot menu
*   Displaying the current chat ID (in private messages or group chats)
*   Sending files of different formats (pdf, jpg, doc, mp3, etc.)
*   Sending voice messages (*.ogg files)
*   Sending Geolocation
*   Creating a separate group chat between a user and a bot

For our bot, we will use the ASP.Net technology that allows for launching a server that will process user requests and respond to them.

[Get free access to WhatsApp API](https://app.chat-api.com)  

## Chapter 1\. Creating an ASP.Net Project

Open Visual Studio and create a project with the name "ASP.NET Core Web App".

Next, choose an empty project template. You can also choose an API template that already includes all the necessary controllers — then you will only have to edit them. In our example though, for illustrative purposes, we will develop the project from scratch.

Open the **Startup.cs** file and implement the Configure method:

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
             if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

This will allow you to use navigation via controller. Next up, you’ll need to write the controller itself.

To do this, create a Controllers folder within the project — this is where you will create a WebHookController class.

The controller must inherit the **ControllerBase** class and be marked by the attributes

    using Microsoft.AspNetCore.Mvc;
    namespace WaBot.Controllers
        {
            [ApiController]
            [Route("/")]
            public class WebHookController : ControllerBase
                {

                }
        }

The **Route** attribute is responsible for the address which the controller will serve. Now specify the domain base path.

At this stage, the controller is practically ready. You need to add methods of working with API WA and other utility classes that you will use.

### Phone Authorization

Now you are supposed to connect WhatsApp to your script so you could check the code while you are writing it. To do this, go to your [User Account](https://app.chat-api.com) and get the QR code. Then open WhatsApp on your phone and go to Settings -> WhatsApp Web -> Scan the QR Code.

## Chapter 2\. The API Class

In this chapter, we will look at how to write the class responsible for communication with our API gateway. You can check the documentation [here](https://chat-api.com/en/docs.html). Here is how you create the **WaApi** class:

    public class WaApi
        {
            private string APIUrl = "";
            private string token = "";

            public WaApi(string aPIUrl, string token)
                {
                    APIUrl = aPIUrl;
                    this.token = token;
                }
        }

This class will contain the **APIUrl** and **token** fields that are necessary for working with API. You can get them in your user account.

The same applies to the constructor that assigns values to the fields and allows you to have several objects representing different bots in case you want to run multiple bots simultaneously.

### The Method of Sending Requests

Now it’s time to add an async method to the class. This will be responsible for sending POST requests:

    public async Task<string> SendRequest(string method, string data)
        {
            string url = $"{APIUrl}{method}?token={token}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                var result = await client.PostAsync("", content);
                return await result.Content.ReadAsStringAsync();
            }
        }

The method receives two arguments.

*   _method_ — the name of the method according to the [documentation](https://chat-api.com/en/docs.html)
*   _data_ — the JSON string for sending data

In the method, form a URL string where requests will be sent. Send a POST request to the address using System.Net.Http.HttpClient and return the server’s response.

The method allows you to build all the functionality necessary for the bot’s work.

### Sending Messages

    public async Task<string> SendMessage(string chatID, string text)
        {
            var data = new Dictionary<string, string>()
            {
                {"chatId",chatID },
                { "body", text }
            };
            return await SendRequest("sendMessage", JsonConvert.SerializeObject(data));
        }

The method’s parameters are:

*   chatId — the ID of the chat where the message is to be sent
*   text — the text of the message to be sent

You can form the JSON string with the help of the handy [Newtonsoft.Json](https://www.newtonsoft.com/json) library. Create a dictionary where, according to the [documentation](https://chat-api.com/en/docs.html), the string with the corresponding JSON field will be the key and your parameters — the values. To do this, just call the **JsonConvert.SerializeObject** method, add your dictionary to it, and the JSON string will be formed. Then, call **SendRequest** with two parameters: the name of the method of sending messages and the JSON string.

So, this is how your bot will respond to users.

### Sending Voice Messages

    public async Task<string> SendOgg(string chatID)
        {
            string ogg = "https://firebasestorage.googleapis.com/v0/b/chat-api-com.appspot.com/o/audio_2019-02-02_00-50-42.ogg?alt=media&token=a563a0f7-116b-4606-9d7d-172426ede6d1";
            var data = new Dictionary<string, string>
            {
                {"audio", ogg },
                {"chatId", chatID }
            };

            return await SendRequest("sendAudio", JsonConvert.SerializeObject(data));
        }

The rest of the methods are built according to the same logic. You are supposed to look up the [documentation](https://chat-api.com/en/docs.html) and send the necessary data to the server by calling the corresponding methods.

To send a voice message, use the link to the .ogg file and the **sendAudio** method.

### Method of sending the geolocation

    public async Task<string> SendGeo(string chatID)
        {
            var data = new Dictionary<string, string>()
            {
                { "lat", "55.756693" },
                { "lng", "37.621578" },
                { "address", "Your address" },
                { "chatId", chatID}
            };
            return await SendRequest("sendLocation", JsonConvert.SerializeObject(data));
        }

### Creating a Group

The method creates a conference for you and the bot.

    public async Task<string> CreateGroup(string author)
        {
            var phone = author.Replace("@c.us", "");
            var data = new Dictionary<string, string>()
            {
                { "groupName", "Group C#"},
                { "phones", phone },
                { "messageText", "This is your group." }
            };
            return await SendRequest("group", JsonConvert.SerializeObject(data));
        }

### The Method of Sending Files

The method uses such parameters as:

*   chatID — the ID of the chat
*   format — the format of the file to be sent.

According to the documentation, you can send files by either of the following ways:

*   Providing a link to the file
*   Providing a string containing the file encoded via the Base64 method

**We recommend that you use the second method**, that is, encoding files in the Base64 format. We will talk more about it in **Chapter 4**. For the moment though, we have created the static **Base64String** class with properties containing test files of all necessary formats. These properties are used to send test files to the server.

    public async Task<string> SendFile(string chatID, string format)
        {
            var availableFormat = new Dictionary<string, string>()
            {
                {"doc", Base64String.Doc },
                {"gif",Base64String.Gif },

                { "jpg",Base64String.Jpg },
                { "png", Base64String.Png },
                { "pdf", Base64String.Pdf },
                { "mp4",Base64String.Mp4 },
                { "mp3", Base64String.Mp3}
            };

            if (availableFormat.ContainsKey(format))
            {
                var data = new Dictionary<string, string>(){
                    { "chatId", chatID },
                    { "body", availableFormat[format] },
                    { "filename", "yourfile" },
                    { "caption", $"My file!" }
                };

                return await SendRequest("sendFile", JsonConvert.SerializeObject(data));
            }

            return await SendMessage(chatID, "No file with this format");
        }

Now that we have covered the basic functionality of the API class, let’s move on and join the API with the controller from **Chapter 1**.

## Chapter 3\. Processing Requests

Coming back to the controller from Chapter 1, let’s create a method that will process post requests coming to the server from chat-api.com. Let’s name the method **Post** (you can also give it any other name) and mark it with the attribute **[HttpPost]** which means that it will react to Post requests.

    [HttpPost]
    public async Task<string> Post(Answer data)
        {
            return "";
        }

The method will accept the **Answer** class, i.e., the deserialized object we got from the JSON string. In order to implement the **Answer** class, we’ll have to know what JSON we will receive.

To do this, you can use the handy **Testing - Webhook Simulation** section in your user account.

The JSON body you will receive will show on the right.

You can also use the [Conversion of JSON to C#](http://json2csharp.com/) service or create the class yourself by using the attributes of the [Newtonsoft.Json](https://www.newtonsoft.com/json) library:

    public partial class Answer
        {
            [JsonProperty("instanceId")]
            public string InstanceId { get; set; }

            [JsonProperty("messages")]
            public Message[] Messages { get; set; }
        }

    public partial class Message
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("senderName")]
            public string SenderName { get; set; }

            [JsonProperty("fromMe")]
            public bool FromMe { get; set; }

            [JsonProperty("author")]
            public string Author { get; set; }

            [JsonProperty("time")]
            public long Time { get; set; }

            [JsonProperty("chatId")]
            public string ChatId { get; set; }

            [JsonProperty("messageNumber")]
            public long MessageNumber { get; set; }
        }

Now that you have an object representation of the incoming request, it’s time to process it in the controller.

Inside the controller, create a static field which will serve as both an API link and a token.

    private static readonly WaApi api = new WaApi("https://eu115.chat-api.com/instance12345/", "123456789token");

In the method’s cycle, process all the incoming messages checking if they are not your own. In this way, you will make sure your bot is not looped on itself. If you see a message from yourself, just skip it:

    [HttpPost]
    public async Task<string> Post(Answer data)
        {
            foreach (var message in data.Messages)
            {
                if (message.FromMe)
                    continue;
            }
        }

Next, you need to get the command from the incoming message. To do this, extract the first word from message.Body using Split() and convert it to the lower case using toLower(). And, finally, process the command using the switch statement.

    [HttpPost]
    public async Task<string> Post(Answer data)
        {
            foreach (var message in data.Messages)
            {
                if (message.FromMe)
                    continue;

                switch (message.Body.Split()[0].ToLower())
                {
                    case "chatid":
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

Write into the **case** all the commands you need and call methods that realize them from your API’s object. As for **default**, it will process commands that don’t exist and send messages from the bot’s menu to users.

## WhatsApp Bot on Csharp

Your bot is ready! It can process and respond to user commands. All that is left for you to do now is add it to the hosting and specify your domain as a webhook in your chat-api.com user account.

The source code of the bot will be available at GitHub via the link: [https://github.com/chatapi/whatsapp-csharp-bot-en](https://github.com/chatapi/whatsapp-csharp-bot-en "Download sources of whatsapp bot on Csharp"). Don’t forget to insert your token and instance number from your user account.

[Get API key](https://app.chat-api.com)  

### Chapter 4\. Base64

**Base64** is a standard for encoding data. It allows for converting files to strings and sending them as such.

In your user account, you can find the [service](https://app.chat-api.com/base64) that helps generate strings of this format. When writing the bot, we had to declare some static properties for testing which is why we inserted the received strings into the utility class and called them from the code. However, you may prefer to encode files “on the go”.

You can also generate strings like these by using built-in C# tools.

### Chapter 5\. Server Publishing

To install a server as a webhook, you need to upload the server to the Internet. To do this, you can use services that offer hosting, VPS, or VDS servers. You will need to choose and pay for the service that supports the ASP.Net technology.

### Problems you May Encounter on your Way

*   You can’t connect to the hosting server and publish your server. Solution: Contact Technical Support and ask them to enable Web Deploy for you.

*   **HTTP ERROR 500.0** [The solution is here](https://stackoverflow.com/questions/55731142/error-trying-to-host-an-asp-net-2-2-website-on-plesk-onyx-17-8-11-http-error-5)