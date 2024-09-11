using Avalonia.Controls;
using SmartUnzip.Views;

namespace SmartUnzip.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private object? _content;

    public MenuViewModel Menus { get; set; } = new();

    private Dictionary<string, (UserControl View, ViewModelBase ViewModel)> Views { get; } = new();

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<MainViewModel, string>(this, OnNavigation);
        OnNavigation(this, MenuKeys.Home);
    }

    private void OnNavigation(MainViewModel vm, string s)
    {
        if (Views.TryGetValue(s, out var view))
        {
            Content = view.View;
            return;
        }

        switch (s)
        {
            case MenuKeys.Home:
                Views[s] = (new HomeView(), new HomeViewModel());
                break;
            case MenuKeys.PasswordManager:
                Views[s] = (new PasswordManagerView(), new PasswordManagerViewModel());
                break;
            case MenuKeys.Settings:
                Views[s] = (new SettingsView(), new SettingsViewModel());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Views[s].View.DataContext = Views[s].ViewModel;
        Content = Views[s].View;
        
    }
}