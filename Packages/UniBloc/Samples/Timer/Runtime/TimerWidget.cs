using System.Globalization;
using Cysharp.Threading.Tasks;
using TMPro;
using UniBloc.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Timer.Runtime
{
    public class TimerWidget : PooledBlocWidget<TimerBloc, TimerEvent, TimerState>
    {
        [SerializeField] private TextMeshProUGUI timerLabel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button replayButton;

        protected override TimerBloc CreateBloc() => new();

        protected override void OnCreated()
        {
            OnClick(playButton, OnClickPlay);
            OnClick(pauseButton, OnClickPause);
            OnClick(replayButton, OnClickReplay);
        }

        private void OnClickPlay(AsyncUnit _)
        {
            switch (State)
            {
                case TimerInitial:
                    Add<TimerStarted>(started => started.Duration = State.Duration);
                    break;
                case TimerRunPause:
                    Add<TimerResumed>();
                    break;
            }
        }

        private void OnClickPause(AsyncUnit obj)
        {
            switch (State)
            {
                case TimerRunInProgress:
                    Add<TimerPaused>();
                    break;
            }
        }

        private void OnClickReplay(AsyncUnit obj)
        {
            switch (State)
            {
                case TimerRunInProgress:
                case TimerRunPause:
                case TimerRunComplete:
                    Add<TimerReset>();
                    break;
            }
        }

        protected override void Render(TimerState state)
        {
            var duration = state.Duration;
            var minutes = Mathf.Floor((float) duration / 60 % 60)
                .ToString(CultureInfo.InvariantCulture)
                .PadLeft(2, '0');

            var seconds = Mathf.Floor((float) duration % 60)
                .ToString(CultureInfo.InvariantCulture)
                .PadLeft(2, '0');

            timerLabel.text = $"{minutes}:{seconds}";

            var enabledPlay = state is TimerInitial or TimerRunPause;
            var enabledPause = state is TimerRunInProgress;
            var enabledReplay = state is TimerRunInProgress or TimerRunPause or TimerRunComplete;

            playButton.gameObject.SetActive(enabledPlay);
            pauseButton.gameObject.SetActive(enabledPause);
            replayButton.gameObject.SetActive(enabledReplay);
        }

        private void Update()
        {
            if (State is TimerRunInProgress)
            {
                Bloc.Tick(Time.deltaTime);
            }
        }
    }
}