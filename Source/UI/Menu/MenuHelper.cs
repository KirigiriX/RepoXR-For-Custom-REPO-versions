using RepoXR.Assets;

namespace RepoXR.UI.Menu;

internal static class MenuHelper
{
    public static void RegisterCustomMenus(MenuManager manager)
    {
        manager.menuPages.AddRange([
            new MenuManager.MenuPages
            {
                menuPageIndex = (MenuPageIndex)RepoXRMenuPage.VRSettings,
                menuPage = AssetCollection.MenuSettings,
            },
            new MenuManager.MenuPages
            {
                menuPageIndex = (MenuPageIndex)RepoXRMenuPage.VRSettingsCategory,
                menuPage = AssetCollection.MenuSettingsCategory
            },
            new MenuManager.MenuPages
            {
                menuPageIndex = (MenuPageIndex)RepoXRMenuPage.VRShowcase,
                menuPage = AssetCollection.MenuShowcase
            }
        ]);
    }

    public static void PageOpenOnTop(RepoXRMenuPage page)
    {
        MenuManager.instance.PageOpenOnTop((MenuPageIndex)page);
    }

    public static MenuPage PageAddOnTop(RepoXRMenuPage page)
    {
        MenuManager.instance.PageAddOnTop((MenuPageIndex)page);
        
        return MenuManager.instance.addedPagesOnTop.Find(p =>
            p.menuPageIndex == (MenuPageIndex)page && p.currentPageState != MenuPage.PageState.Closing);
    }

    public enum RepoXRMenuPage
    {
        VRSettings = 37449,
        VRSettingsCategory,
        VRShowcase
    }
}