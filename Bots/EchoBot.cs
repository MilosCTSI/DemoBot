// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Botina.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly IStorage _myStorage;
        //private static readonly MemoryStorage _myStorage = new MemoryStorage();
        public EchoBot(IStorage myStorage)
        {
            _myStorage = myStorage;

        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var utterance = turnContext.Activity.Text;
            await HandleChatHistory(utterance, cancellationToken);
        
            var builder = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    "milosLearnDeployment",
                    "https://miloslearn.openai.azure.com/",
                    "f579d75ecedd48b4a887368a70971284"
                );

            Kernel kernel = builder.Build();


            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var searchClient = CreateAzureAiSearchClient();

            await FillAzureAISerachWithData(searchClient);


            SearchOptions options = new SearchOptions() { Size = 3};

            SearchResults<KnowledgeBaseEntry> response = await searchClient.SearchAsync<KnowledgeBaseEntry>(utterance, options);

            foreach(SearchResult<KnowledgeBaseEntry> item in  response.GetResults())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(item.Document.id + ": " + item.Document.Body), cancellationToken);
            }

            var history = new ChatHistory();
            history.AddUserMessage(utterance);

            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: kernel);
            await turnContext.SendActivityAsync(MessageFactory.Text(result.Content, result.Content), cancellationToken);
            
        }
        private static async Task FillAzureAISerachWithData(SearchClient searchClient)
        {
            string filename = "data2.json";
            string jsonString = File.ReadAllText(filename);
            var dataEntries = JsonSerializer.Deserialize<List<KnowledgeBaseEntry>>(jsonString);

            Response<IndexDocumentsResult> idxresult = await searchClient.UploadDocumentsAsync(dataEntries);
        }

        private SearchClient CreateAzureAiSearchClient()
        {
            //search credentials
            string indexName = "knowledgebaseindex";
            Uri endpoint = new Uri("https://miloslearnseach.search.windows.net");
            string key = "4Uj1fJN3Ue44kIGUFsSm38AQcOZGrMassDrcxspBjXAzSeBeLI3c";
            var searchCredential = new AzureKeyCredential(key);

            var indexClient = new SearchIndexClient(endpoint, searchCredential);

            return indexClient.GetSearchClient(indexName);
        }

        private async Task HandleChatHistory(string utterance, CancellationToken cancellationToken)
        {
            UtteranceLog logItems = null;
            string[] utteranceList = { "Tekst" };
            logItems = _myStorage.ReadAsync<UtteranceLog>(utteranceList).Result?.FirstOrDefault().Value;

            if (logItems is null)
            {
                logItems = new UtteranceLog();
                logItems.UtteranceList.Add(utterance);

                logItems.TurnNumber++;

                var changes = new Dictionary<string, object>();
                {
                    changes.Add("Tekst", logItems);
                }
                await _myStorage.WriteAsync(changes, cancellationToken);
            }
            else
            {
                logItems.UtteranceList.Add(utterance);
                logItems.TurnNumber++;

                //await turnContext.SendActivityAsync($"{logItems.TurnNumber}: The list is now: {string.Join(", ", logItems.UtteranceList)}");

                var changes = new Dictionary<string, object>();
                {
                    changes.Add("Tekst", logItems);
                };

                await _myStorage.WriteAsync(changes, cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome to SOMI's bot!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
