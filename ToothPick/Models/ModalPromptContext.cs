namespace ToothPick.Models
{
    /// <summary>
    /// Context model used to help create a blazor <see cref="ModalPrompt"/> component.
    /// </summary>
    public class ModalPromptContext
    {
        /// <summary>
        /// The modal's title.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// The modal's body.
        /// </summary>
        public MarkupString? Body { get; set; }

        /// <summary>
        /// The text displayed on the dismiss button of the modal.
        /// </summary>
        public string? CancelChoice { get; set; }

        /// <summary>
        /// The text displayed on the accept button of the modal.
        /// </summary>
        public string? Choice { get; set; }

        /// <summary>
        /// The bootstrap color of the choice button.
        /// </summary>
        public string? ChoiceColour { get; set; }

        /// <summary>
        /// The delegate <see cref="Action"/> that is invoked when the accept button is chosen.
        /// </summary>
        public Action? ChoiceAction { get; set; }

        /// <summary>
        /// The text displayed on the other accept button of the modal.
        /// </summary>
        public string? OtherChoice { get; set; }

        /// <summary>
        /// The bootstrap color of the other choice button.
        /// </summary>
        public string? OtherChoiceColour { get; set; }

        /// <summary>
        /// The delegate <see cref="Action"/> that is invoked when the other accept button is chosen.
        /// </summary>
        public Action? OtherChoiceAction { get; set; }
    }
}
