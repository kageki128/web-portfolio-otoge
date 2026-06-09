using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MyProject.Actor
{
    public class SelectInputKeysActor : ActorBase
    {
        [SerializeField] KeyIconActor keyIcon0;
        [SerializeField] KeyIconActor keyIcon1;
        [SerializeField] KeyIconActor keyIcon2;
        [SerializeField] KeyIconActor keyIcon3;
        [SerializeField] KeyIconActor keyIcon4;
        [SerializeField] KeyIconActor keyIcon5;
        [SerializeField] KeyIconActor keyIcon6;
        [SerializeField] KeyIconActor keyIcon7;
        [SerializeField] KeyIconActor keyIcon8;

        KeyIconActor[] keyIcons;

        public override void Initialize()
        {
            keyIcons = new[]
            {
                keyIcon0,
                keyIcon1,
                keyIcon2,
                keyIcon3,
                keyIcon4,
                keyIcon5,
                keyIcon6,
                keyIcon7,
                keyIcon8
            };

            foreach (var keyIcon in keyIcons)
            {
                keyIcon.Initialize();
            }

            gameObject.SetActive(false);
        }

        public override UniTask ShowAsync(CancellationToken ct)
        {
            gameObject.SetActive(true);
            return UniTask.WhenAll(keyIcons.Select(keyIcon => keyIcon.ShowAsync(ct)));
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            await UniTask.WhenAll(keyIcons.Select(keyIcon => keyIcon.HideAsync(ct)));
            gameObject.SetActive(false);
        }

        public void LightUpKey(int key)
        {
            keyIcons[key].LightUp();
        }

        public void LightDownKey(int key)
        {
            keyIcons[key].LightDown();
        }
    }
}
