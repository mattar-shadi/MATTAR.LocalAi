using MATTAR.LocalAi.MauiBlazorHybrid.Shared.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Markdig;

namespace MATTAR.LocalAi.MauiBlazorHybrid.Shared.Pages
{
    public partial class Chat
    {
        private string userMessage = string.Empty;
        private readonly Conversation conversation = new();
        private ElementReference inputChat;
        private ElementReference chatContainer;
        private ElementReference stopButton;
        private ElementReference sendButton;
        private CancellationTokenSource _cancellationTokenSource = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            conversation.Title = "New Chat";
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!firstRender)
                return;

            Message assistantMsg = new()
            {
                Autors = "Assistant",
                Content = "Hello, I'm your assistant. How can I help you ?"
            };
            assistantMsg.HtmlContent = Markdown.ToHtml(assistantMsg.Content);
            conversation.Messages.Add(assistantMsg);

            await Task.Run(async () =>
            {
                await InvokeAsync(StateHasChanged);
            });
        }

        private async Task DisplayAssistantMessage()
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            Message userQueryMessage = new() { Autors = "User", Content = userMessage };
            Message assistantMsg = new() { Autors = "Assistant", Content = string.Empty };

            conversation.Messages.Add(userQueryMessage);
            conversation.Messages.Add(assistantMsg);

            userMessage = string.Empty;
            
            await AssistantBeginAnswer();

            if (ChatService is null)
                throw new ArgumentException("Chat is not initialized.");

            try
            {
                await ChatService.Run(
                    userQ: userQueryMessage.Content,
                    action: async (s) =>
                    {
                        assistantMsg.Content += s;
                        assistantMsg.HtmlContent = Markdown.ToHtml(assistantMsg.Content);
                        await JSRuntime.InvokeVoidAsync("scrollToEnd", [chatContainer]);
                        await InvokeAsync(StateHasChanged);
                    },
                    cancellationToken: _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }
            finally
            {
                await AssitantEndAnswer();
            }
        }

        private async Task StopGeneration()
        {
            await _cancellationTokenSource.CancelAsync();
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await DisplayAssistantMessage();
            }
        }

        private async Task AssistantBeginAnswer()
        {
            await JSRuntime.InvokeVoidAsync("disableInput", [inputChat]);
            await JSRuntime.InvokeVoidAsync("scrollToEnd", [chatContainer]);
            await JSRuntime.InvokeVoidAsync("addHideClass", [sendButton]);
            await JSRuntime.InvokeVoidAsync("removeHideClass", [stopButton]);
            await JSRuntime.InvokeVoidAsync("scrollToEnd", [chatContainer]);
            await InvokeAsync(StateHasChanged);
        }

        private async Task AssitantEndAnswer()
        {
            await JSRuntime.InvokeVoidAsync("enableInput", [inputChat]);
            await JSRuntime.InvokeVoidAsync("addHideClass", [stopButton]);
            await JSRuntime.InvokeVoidAsync("removeHideClass", [sendButton]);
        }
    }
}
