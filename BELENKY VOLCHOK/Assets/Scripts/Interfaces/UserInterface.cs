public interface UserInterface
{
    void ShowHint(string hintKey);
    void HideHint();
    void ShowMessage(string messageKey, float duration = 3f);
    void HideMessage();
}
