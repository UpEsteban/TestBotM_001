// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CoreBot;
using CoreBot.Dialogs.IdentificationDialog;
using CoreBot.Dialogs.MenuDialog;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;

        private IStatePropertyAccessor<ConversationData> _conversationStateAccessor;

        public MainDialog(ConversationState conversationState, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _conversationStateAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));

            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new IdentificationDialog());
            AddDialog(new MenuDialog(conversationState));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                IdentificationStepAsync,
                MenuStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> IdentificationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(IdentificationDialog));
        }

        private async Task<DialogTurnResult> MenuStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(!string.IsNullOrEmpty(stepContext.Result.ToString()))
            {
                await _conversationStateAccessor.SetAsync(stepContext.Context, new ConversationData { Location = stepContext.Result.ToString() });
                return await stepContext.BeginDialogAsync(nameof(MenuDialog));
            }
            else return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
