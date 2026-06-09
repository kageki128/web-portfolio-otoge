using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MyProject.Actor
{
    public class DifficultySelectActor : ActorBase
    {
        [SerializeField] KeyIconActor downKey;
        [SerializeField] KeyIconActor upKey;
        [SerializeField] TMP_Text valueText;

        public override void Initialize()
        {
            downKey.Initialize();
            upKey.Initialize();
            gameObject.SetActive(false);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            await UniTask.WhenAll(
                downKey.ShowAsync(ct),
                upKey.ShowAsync(ct)
            );
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await UniTask.WhenAll(
                downKey.HideAsync(ct),
                upKey.HideAsync(ct)
            );
            gameObject.SetActive(false);
        }

        public void SetValue(string value)
        {
            valueText.text = value;
        }

        public void LightUpUpKey()
        {
            upKey.LightUp();
        }

        public void LightDownUpKey()
        {
            upKey.LightDown();
        }

        public void LightUpDownKey()
        {
            downKey.LightUp();
        }

        public void LightDownDownKey()
        {
            downKey.LightDown();
        }
    }
}
