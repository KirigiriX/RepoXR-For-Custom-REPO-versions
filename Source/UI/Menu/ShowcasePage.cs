using TMPro;
using UnityEngine;

namespace RepoXR.UI.Menu;

public class ShowcasePage : MonoBehaviour
{
    public TextMeshProUGUI versionText;
    public TextMeshProUGUI commitText;

    private void Awake()
    {
#if DEBUG
        versionText.text = $"v{Plugin.PLUGIN_VERSION}-dev";
#else
        versionText.text = $"v{Plugin.PLUGIN_VERSION}";
#endif

        commitText.text = $"Commit: {Plugin.GetCommitHash()}";
    }

    public void MadeByClicked()
    {
        Application.OpenURL("https://github.com/DaXcess");
    }

    public void GitHubClicked()
    {
        Application.OpenURL("https://github.com/DaXcess/RepoXR");
    }

    public void DiscordClicked()
    {
        Application.OpenURL("https://discord.gg/2DxNgpPZUF");
    }

    public void KoFiClicked()
    {
        Application.OpenURL("https://ko-fi.com/daxcess");
    }
}