using System.Collections.ObjectModel;
using System.Linq;
using Bing.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartUnzip.Core;

namespace SmartUnzip.Avalonia.ViewModels;

public partial class PasswordManagerViewModel : ObservableObject
{
    private readonly IPasswordRepository _passwordRepository;

    [ObservableProperty]
    ObservableCollection<UnzipPassword> passwords = [];


    [ObservableProperty]
    private string addPassword;

    public PasswordManagerViewModel(IPasswordRepository passwordRepository)
    {
        _passwordRepository = passwordRepository;
        LoadPasswords();
    }

    public void Delete(string password)
    {
        var unzipPw = new UnzipPassword(password);
        _passwordRepository.RemovePassword(unzipPw);
        LoadPasswords();
    }

    public void Add(string password)
    {
        var unzipPw = new UnzipPassword(password);
        _passwordRepository.AddPassword(unzipPw);
        AddPassword = "";
        LoadPasswords();
    }

    public void LoadPasswords()
    {
        Passwords.Clear();
        Passwords.AddRange(_passwordRepository.GetAllPasswords().OfType<UnzipPassword>());
    }
}