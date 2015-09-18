namespace SimpleDnsCrypt.Tools
{
    /// <summary>
    ///     Interface for the message box overlay.
    /// </summary>
    public interface IShell
    {
        void ShowOverlay();
        void HideOverlay();
    }
}