public interface MinigameStarter
{
    void StartMinigame(MinigameConfiguration config);
    void CancelCurrentMinigame();
    bool IsMinigameActive { get; }
}
