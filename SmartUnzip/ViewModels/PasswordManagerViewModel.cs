using CommunityToolkit.Mvvm.Input;
using SmartUnzip.Core;

namespace SmartUnzip.ViewModels;

public partial class PasswordManagerViewModel : ViewModelBase
{
    public ObservableCollection<PasswordModel> Passwords => App.Settings.Passwords;

    [RelayCommand]
    public void RemovePassword(PasswordModel password)
    {
        App.Settings.RemovePassword(password.Value);
    }
}