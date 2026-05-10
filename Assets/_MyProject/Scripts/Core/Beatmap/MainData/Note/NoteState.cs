namespace MyProject.Core
{
    public enum NoteState
    {
        BeforeJudge, // まだ判定が行われていない
        Missed, // Holdが一度も押されずに見逃されている
        Holding, // Holdが押されている
        Released, // ホールドが途中で離された
        AfterJudge, // 判定が終了した
    }
}
