using UnityEngine;

namespace RepoXR.UI.Menu;

public class SettingsPage : MonoBehaviour
{
    private void Start()
    {
        MenuManager.instance.PageCloseAllAddedOnTop();
        MenuHelper.PageAddOnTop(MenuHelper.RepoXRMenuPage.VRShowcase);
    }

    public void ButtonEventOpenCategory(string category)
    {
        MenuManager.instance.PageCloseAllAddedOnTop();
        
        var page = MenuHelper.PageAddOnTop(MenuHelper.RepoXRMenuPage.VRSettingsCategory);
        var fish = page.GetComponent<SettingsCategoryPage>();

        fish.categoryName = category;
    }
        
    public void ButtonEventBack()
    {
        if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
        {
            MenuManager.instance.PageCloseAllExcept(MenuPageIndex.Main);
            MenuManager.instance.PageSetCurrent(MenuPageIndex.Main, MenuPageMain.instance.menuPage);
        }
        else if (RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
        {
            MenuManager.instance.PageCloseAllExcept(MenuPageIndex.Lobby);
            MenuManager.instance.PageSetCurrent(MenuPageIndex.Lobby, MenuPageLobby.instance.menuPage);
        }
        else
        {
            MenuManager.instance.PageCloseAll();
            MenuManager.instance.PageOpen(MenuPageIndex.Escape);
        }
    }
}