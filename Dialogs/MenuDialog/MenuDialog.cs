using CoreBot.Helpers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.BotBuilderSamples.Dialogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs.MenuDialog
{
    public class MenuDialog : BaseDialog
    {
        private IStatePropertyAccessor<ConversationData> _conversationStateAccessor;

        public MenuDialog(ConversationState conversationState) : base(nameof(MenuDialog))
        {
            _conversationStateAccessor = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                AskCaseStepAsync,
                GiveInformationStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> AskCaseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversation = await _conversationStateAccessor.GetAsync(stepContext.Context);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { 
                Prompt = MessageFactory.Text("What you need information about."), 
                Choices = SetChoices(conversation.Location),
                Style = ListStyle.SuggestedAction
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> GiveInformationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversation = await _conversationStateAccessor.GetAsync(stepContext.Context);
            conversation.OptionSelected = ((FoundChoice)stepContext.Result).Value.ToString();

            var message = GetInformation(((FoundChoice)stepContext.Result).Value.ToString());

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("What you need information about."),
                Choices = new List <Choice>  { ChoiceHelper("More information", new List<string> { "More information" }), ChoiceHelper("Menu", new List<string> { "Menu" }) },
                Style = ListStyle.SuggestedAction
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversation = await _conversationStateAccessor.GetAsync(stepContext.Context);

            if (((FoundChoice)stepContext.Result).Value.ToString().Equals("More information")) { 
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"For get more information about {conversation.OptionSelected} service I transfer you to an operator. See you soon."), cancellationToken);
            }

            return await stepContext.EndDialogAsync(stepContext.Result, cancellationToken);
        }

        private Choice ChoiceHelper(string value, List<string> synonymus) => new Choice { Value = value, Synonyms = synonymus };

        private List<Choice> SetChoices(string location)
        {
            List<Choice> result = new List<Choice>
            {
                ChoiceHelper(Menu.menu_agility, new List<string> { "Agility", "What leasing options do you offer?" }),
                ChoiceHelper(Menu.menu_others, new List<string> { "other", "other location" })
            };

            if (location.Equals(Constant.Location.uk.ToString()))
            {
                result.Add(ChoiceHelper(Menu.menu_personal_operating_lease, new List<string> { "Personal Operating Lease", "Can you explain Personal Operating Lease" }));
                result.Add(ChoiceHelper(Menu.menu_personal_contract_hire, new List<string> { "Personal Contract Hire", "What is contract hire financing?" }));
            }

            if (location.Equals(Constant.Location.south_africa.ToString()))
                result.Add(ChoiceHelper(Menu.menu_flexifix, new List<string> { "FlexiFix", "I´m looking for information about Flexfix" }));
            
            return result;
        }

        private string GetInformation(string option)
        {
            string message = string.Empty;
            switch (option)
            {
                case Menu.menu_agility:
                    message = "Agility is a flexible method of financing a vehicle over a fixed term.The agreement defers your decision of whether you purchase, hand back or part - exchange your vehicle until the end of your agreement.";
                    break;
                case Menu.menu_personal_operating_lease:
                    message = "Personal Operating Lease is a solution for those who want to drive one of our vehicles over a fixed term, with lower monthly rentals and without the worries or commitment of ownership.";
                    break;
                case Menu.menu_personal_contract_hire:
                    message = "Personal Contract Hire gives you the ability to enjoy driving Mercedes - Benz without having to take on full ownership.With this option you lease your vehicle for a fixed period and for a fixed monthly.This rental includes the cost of your vehicle's Road Fund Licence for the duration of your agreement.";
                    break;
                case Menu.menu_flexifix:
                    message = "FlexiFix provides peace of mind by fixing your monthly instalments, yet allowing the benefit of potential interest rate reductions over the term.";
                    break;
                default:
                    break;
            }

            return message;
        }
    }
}

    



