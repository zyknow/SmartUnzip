namespace SmartUnzip.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }

    public MenuViewModel()
    {
        MenuItems = new ObservableCollection<MenuItemViewModel>()
        {
            new() { MenuHeader = MenuKeys.Home, Key = MenuKeys.Home,MenuIconName = "mdi-home"},
            new() { MenuHeader = MenuKeys.PasswordManager, Key = MenuKeys.PasswordManager ,MenuIconName = "mdi-lock-plus"},
            new() { MenuHeader = MenuKeys.Settings, Key = MenuKeys.Settings,MenuIconName = "mdi-cog"},
        };
    }
}

public static class MenuKeys
{
    public const string Home = "主页";
    public const string PasswordManager = "密码管理";
    public const string Settings = "设置";
}