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
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The modal's body.
        /// </summary>
        public MarkupString Body { get; set; } = new();

        /// <summary>
        /// The text displayed on the dismiss button of the modal.
        /// </summary>
        public string CancelChoice { get; set; } = string.Empty;

        /// <summary>
        /// The text displayed on the accept button of the modal.
        /// </summary>
        public string Choice { get; set; } = string.Empty;

        /// <summary>
        /// The bootstrap color of the choice button.
        /// </summary>
        public string ChoiceColour { get; set; } = string.Empty;

        /// <summary>
        /// The delegate <see cref="Action"/> that is invoked when the accept button is chosen.
        /// </summary>
        public Action ChoiceAction { get; set; } = null;
    }
}
