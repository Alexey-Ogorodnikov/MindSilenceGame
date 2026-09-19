using System;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Menu dialog flag and training navigation. How to train is overlay state,
    /// not a route. Does not reference AppNavigator.
    /// </summary>
    public sealed class MenuController
    {
        public bool ShowHowToTrain { get; private set; }

        public event Action Changed;

        public event Action NavigateToTraining;

        public void OpenHowToTrain()
        {
            if (ShowHowToTrain)
            {
                return;
            }

            ShowHowToTrain = true;
            Changed?.Invoke();
        }

        public void DismissHowToTrain()
        {
            if (!ShowHowToTrain)
            {
                return;
            }

            ShowHowToTrain = false;
            Changed?.Invoke();
        }

        public void OpenTraining()
        {
            NavigateToTraining?.Invoke();
        }
    }
}
