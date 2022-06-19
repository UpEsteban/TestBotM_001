using CoreBot.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.BotBuilderSamples.Dialogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs.IdentificationDialog
{
    public class IdentificationDialog : BaseDialog
    {
        public IdentificationDialog() : base(nameof(IdentificationDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                AskLocationStepAsync,
                CheckLocationStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }



        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AskLocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = "In order to customize your experience I need you to select one of the available locations.";
            var promptMessage = MessageFactory.Text(messageText, messageText);
            var choices = new List<Choice> { 
                ChoiceHelper("UK", new List<string> { "🇬🇧", "UK", "united kingdom" }), 
                ChoiceHelper("South Africa", new List<string> { "🇿🇦", "South Africa" }), 
                ChoiceHelper("others", new List<string> { "other", "other location" }) };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { Prompt = promptMessage, Choices  = choices }, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckLocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string result = string.Empty;
            switch (((FoundChoice)stepContext.Result).Value.ToString())
            {
                case "UK":
                    result = Constant.Location.uk.ToString();
                    break;
                case "South Africa":
                    result = Constant.Location.south_africa.ToString();
                    break;
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("For a better service I transfer you to an operator. See you soon."));
                    break;
            }
            return await stepContext.NextAsync(result);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(stepContext.Result, cancellationToken);
        }

        private Choice ChoiceHelper(string value, List<string> synonymus) => new Choice { Value = value, Synonyms = synonymus };
    }
}
