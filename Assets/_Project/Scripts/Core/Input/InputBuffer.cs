namespace HollowSpirit.Core.Input
{
    /// <summary>Actions that can be buffered.</summary>
    public enum BufferedAction { None, Attack, Roll, Interact, Heal }

    /// <summary>Remembers the latest pressed action for a short window so it can be consumed once.</summary>
    public sealed class InputBuffer
    {
        private readonly float windowSeconds;
        private BufferedAction action;
        private float pressTime;

        public InputBuffer(float windowSeconds)
        {
            this.windowSeconds = windowSeconds;
        }

        /// <summary>Stores the action, replacing any older one.</summary>
        public void Press(BufferedAction action, float time)
        {
            this.action = action;
            pressTime = time;
        }

        /// <summary>Returns true once if the action was pressed within the window.</summary>
        public bool TryConsume(BufferedAction action, float time)
        {
            if (action == BufferedAction.None || this.action != action)
            {
                return false;
            }

            bool inWindow = time - pressTime <= windowSeconds;
            this.action = BufferedAction.None;
            return inWindow;
        }

        public void Clear()
        {
            action = BufferedAction.None;
        }
    }
}
