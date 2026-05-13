namespace MyProject.Actor
{
    public abstract class OtogeSharedActorBase : ActorBase
    {
        // OtogeTypeに応じたTransformや長さなどの切り替えを行う
        public abstract void SetState(OtogeType otogeType);
    }
}
