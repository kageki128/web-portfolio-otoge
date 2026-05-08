using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace MyProject.Actor
{
    public class RootActorHub : MonoBehaviour
    {
        [SerializeField] ScrollBackgroundActor scrollBackground;
        [SerializeField] StandardSliderActor audioSlider;
        [SerializeField] AudioButtonActor audioButton;

        readonly CompositeDisposable disposables = new();

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);

            InitializeActors();
            BindAudioActors();
            await InitialShowAsync(ct);
        }

        void InitializeActors()
        {
            scrollBackground.Initialize();
            audioSlider.Initialize();
            audioButton.Initialize();
        }

        UniTask InitialShowAsync(CancellationToken ct)
        {
            return UniTask.WhenAll
            (
                scrollBackground.InitialShowAsync(ct),
                audioSlider.InitialShowAsync(ct),
                audioButton.InitialShowAsync(ct)
            );
        }

        void BindAudioActors()
        {
            disposables.Clear();

            var audioPlayer = AudioPlayer.Instance;
            var volume = audioPlayer.BgmVolume.CurrentValue;

            audioSlider.SetValue(volume);
            audioButton.SetVolume(volume);
            audioPlayer.SetBgmVolume(volume);
            audioPlayer.SetSeVolume(volume);

            audioSlider.ValueChanged
                .Subscribe(value =>
                {
                    audioPlayer.SetBgmVolume(value);
                    audioPlayer.SetSeVolume(value);
                })
                .AddTo(disposables);
            audioSlider.HandleDoubleClicked
                .Subscribe(_ =>
                {
                    audioPlayer.ResetBgmVolume();
                    audioPlayer.ResetSeVolume();
                })
                .AddTo(disposables);
            audioButton.VolumeRequested
                .Subscribe(value =>
                {
                    audioPlayer.SetBgmVolume(value);
                    audioPlayer.SetSeVolume(value);
                })
                .AddTo(disposables);

            audioPlayer.BgmVolume
                .Subscribe(value =>
                {
                    audioSlider.SetValue(value);
                    audioButton.SetVolume(value);
                })
                .AddTo(disposables);
        }

        void OnDestroy()
        {
            disposables.Dispose();
        }
    }
}
