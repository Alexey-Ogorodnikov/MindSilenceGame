using System;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Navigation is this state. Child panels do not receive this type;
    /// the host calls <see cref="OpenTraining"/>, <see cref="LeaveTraining"/>,
    /// <see cref="OpenHighScores"/>, and <see cref="LeaveHighScores"/>.
    /// Flags are not persisted; a new instance after process death is the menu.
    /// </summary>
    public sealed class AppNavigator
    {
        public bool InTraining { get; private set; }

        public bool ShowHighScores { get; private set; }

        public AppUiState State => new(InTraining, ShowHighScores);

        public bool ShowMenu => State.ShowMenu;

        public bool ShowTraining => State.ShowTraining;

        public event Action Changed;

        public void OpenTraining()
        {
            InTraining = true;
            Changed?.Invoke();
        }

        public void LeaveTraining()
        {
            InTraining = false;
            Changed?.Invoke();
        }

        public void OpenHighScores()
        {
            ShowHighScores = true;
            Changed?.Invoke();
        }

        public void LeaveHighScores()
        {
            ShowHighScores = false;
            Changed?.Invoke();
        }
    }
}
