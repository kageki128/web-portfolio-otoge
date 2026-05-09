using MyProject.Actor;
using MyProject.Core;
using MyProject.Director;
using MyProject.Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MyProject.CompositionRoot
{
    public class MainLifeTimeScope : LifetimeScope
    {
        [Header("Actor")]
        [SerializeField] RootActorHub rootActorHub;
        [SerializeField] SelectActorHub selectActorHub;
        [SerializeField] GameActorHub gameActorHub;
        [SerializeField] ResultActorHub resultActorHub;
        [Header("Config")]
        [SerializeField] GameConfigSO gameConfig;
        [SerializeField] BeatmapFilesSO beatmapFiles;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterCore(builder);
            RegisterActor(builder);
            RegisterDirector(builder);
            RegisterInfrastructure(builder);
        }

        void RegisterCore(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameConfig);
        }

        void RegisterActor(IContainerBuilder builder)
        {
            builder.Register<SelectActions>(Lifetime.Singleton);
            builder.Register<GameActions>(Lifetime.Singleton);
            builder.Register<ResultActions>(Lifetime.Singleton);
            builder.Register<SelectActionsObserver>(Lifetime.Singleton);
            builder.Register<GameActionsObserver>(Lifetime.Singleton);
            builder.Register<ResultActionsObserver>(Lifetime.Singleton);
            builder.RegisterComponent(rootActorHub);
            builder.RegisterComponent(selectActorHub);
            builder.RegisterComponent(gameActorHub);
            builder.RegisterComponent(resultActorHub);
        }

        void RegisterDirector(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MainEntryPoint>(Lifetime.Singleton);
            builder.Register<RootDirector>(Lifetime.Singleton);
            builder.Register<SelectSceneDirector>(Lifetime.Singleton);
            builder.Register<GameSceneDirector>(Lifetime.Singleton);
            builder.Register<ResultSceneDirector>(Lifetime.Singleton);
        }

        void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.Register<PlayerPrefsSaveDataRepository>(Lifetime.Singleton)
                .As<ISaveDataRepository>();
            builder.Register<ServerRankingRegisterer>(Lifetime.Singleton)
                .As<IRankingRegisterer>();
            builder.Register<BeatmapRepository>(Lifetime.Singleton)
                .As<IBeatmapRepository>();
            builder.RegisterInstance(beatmapFiles);
        }
    }
}
