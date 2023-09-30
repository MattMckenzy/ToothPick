namespace ToothPick.Models
{
    /// <summary>
    /// Custom blazor component base. Helps queue actions to be run after render.
    /// </summary>
    public class RenderComponentBase : ComponentBase
    {
        private List<Func<Task>> ActionsToRunAfterRender { get; } = [];
        private SemaphoreSlim ActionsSemaphore { get; } = new(1);

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                await ActionsSemaphore.WaitAsync();

                foreach (var actionToRun in ActionsToRunAfterRender)
                {
                    await ActionsSemaphore.WaitAsync(0);


                    await actionToRun();
                }

                ActionsToRunAfterRender.Clear();
            }
            finally
            {
                ActionsSemaphore.Release();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Run an action once after the component is rendered.
        /// </summary>
        /// <param name="action">Action to invoke after render.</param>
        protected async void RunAfterRender(Func<Task> action)
        {
            try
            {
                await ActionsSemaphore.WaitAsync();

                ActionsToRunAfterRender.Add(action);
            }
            finally
            {
                ActionsSemaphore.Release();
            }
        }
    }
}