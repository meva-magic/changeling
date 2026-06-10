public interface CursorController
{
    void LockForGameplay();
    void UnlockForUI();
    void SaveAndUnlock();
    void Restore();
}
