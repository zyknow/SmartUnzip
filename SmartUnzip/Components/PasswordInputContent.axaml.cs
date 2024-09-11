using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using SmartUnzip.Core;

namespace SmartUnzip.Components;

public partial class PasswordInputContentDataContext : ObservableObject
{
    public ObservableCollection<PasswordModel> Passwords => App.Settings.Passwords;

    [ObservableProperty] string? _newPassword;

    [ObservableProperty] bool _isPasswordValid = true;

    public string? PasswordAlertText => IsPasswordValid ? null : @$"{NewPassword} 已经存在";

    [RelayCommand]
    public void AddPassword()
    {
        if (NewPassword is null) return;
        IsPasswordValid = App.Settings.AddPassword(NewPassword);
        OnPropertyChanged(nameof(PasswordAlertText));
        NewPassword = null;
    }
}

public partial class PasswordInputContent : UserControl
{
    public PasswordInputContent()
    {
        InitializeComponent();
        DataContext = new PasswordInputContentDataContext();
    }
}