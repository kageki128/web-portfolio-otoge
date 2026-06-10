using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyProject.Core;
using unityroom.Api;

namespace MyProject.Infrastructure
{
    public class UnityroomRankingRegisterer : IRankingRegisterer
    {
        readonly IUnityroomApiClient unityroomApiClient;

        const int NormalBoardNo = 1;
        const int HardBoardNo = 2;

        public UnityroomRankingRegisterer(IUnityroomApiClient unityroomApiClient)
        {
            this.unityroomApiClient = unityroomApiClient;
        }

        public UniTask RegisterAsync(ResultCore result, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            unityroomApiClient.SendScore(GetBoardNo(result.BeatmapType), result.Score, ScoreboardWriteMode.HighScoreDesc);
            return UniTask.CompletedTask;
        }

        static int GetBoardNo(BeatmapType beatmapType)
        {
            return beatmapType switch
            {
                BeatmapType.Normal => NormalBoardNo,
                BeatmapType.Hard => HardBoardNo,
                _ => throw new ArgumentOutOfRangeException(nameof(beatmapType), beatmapType, "This beatmap type does not have ranking board."),
            };
        }
    }
}
