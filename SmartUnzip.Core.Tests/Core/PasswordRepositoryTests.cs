using System.Collections.Generic;
using SmartUnzip.Core.Models;

namespace SmartUnzip.Core.Tests.Core;

public sealed class PasswordRepositoryTests : SmartUnzipCoreTestBase
{
    private readonly IPasswordRepository _passwordRepository;

    public PasswordRepositoryTests()
    {
        _passwordRepository = GetRequiredService<IPasswordRepository>();
    }

    [Fact]
    public void TestIsPasswordExists()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        Assert.True(_passwordRepository.IsPasswordExists("test"));
    }

    [Fact]
    public void TestGetAllPasswords()
    {
        var password1 = new UnzipPassword("test1");
        var password2 = new UnzipPassword("test2");
        _passwordRepository.AddPassword(password1);
        _passwordRepository.AddPassword(password2);
        var allPasswords = _passwordRepository.GetAllPasswords();
        Assert.Contains(password1, allPasswords);
        Assert.Contains(password2, allPasswords);
    }

    [Fact]
    public void TestAddPassword()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        Assert.True(_passwordRepository.IsPasswordExists("test"));
    }

    [Fact]
    public void TestAddPasswords()
    {
        var password1 = new UnzipPassword("test1");
        var password2 = new UnzipPassword("test2");
        _passwordRepository.AddPasswords(new List<UnzipPassword> {password1, password2});
        Assert.True(_passwordRepository.IsPasswordExists("test1"));
        Assert.True(_passwordRepository.IsPasswordExists("test2"));
    }

    [Fact]
    public void TestGetPassword()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        var retrievedPassword = _passwordRepository.GetPassword(password);
        Assert.Equal(password, retrievedPassword);
    }

    [Fact]
    public void TestGetPasswordByValue()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        var retrievedPassword = _passwordRepository.GetPassword("test");
        Assert.Equal(password, retrievedPassword);
    }

    [Fact]
    public void TestUpdatePassword()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        password.ManualSort = 100;
        _passwordRepository.UpdatePassword(password);
        var retrievedPassword = _passwordRepository.GetPassword("test");
        Assert.Equal(password.ManualSort, retrievedPassword.ManualSort);
    }

    [Fact]
    public void TestUpdatePasswords()
    {
        var password1 = new UnzipPassword("test1");
        var password2 = new UnzipPassword("test2");
        _passwordRepository.AddPasswords(new List<UnzipPassword> {password1, password2});
        password1.ManualSort = 100;
        password2.ManualSort = 200;
        _passwordRepository.UpdatePasswords(new List<UnzipPassword> {password1, password2});
        var retrievedPassword1 = _passwordRepository.GetPassword("test1");
        var retrievedPassword2 = _passwordRepository.GetPassword("test2");
        Assert.Equal(password1.ManualSort, retrievedPassword1.ManualSort);
        Assert.Equal(password2.ManualSort, retrievedPassword2.ManualSort);
    }

    [Fact]
    public void TestRemovePassword()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        _passwordRepository.RemovePassword(password);
        Assert.False(_passwordRepository.IsPasswordExists("test"));
    }

    [Fact]
    public void TestRemovePasswordByValue()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        _passwordRepository.RemovePassword("test");
        Assert.False(_passwordRepository.IsPasswordExists("test"));
    }

    [Fact]
    public void TestClear()
    {
        var password = new UnzipPassword("test");
        _passwordRepository.AddPassword(password);
        _passwordRepository.Clear();
        Assert.False(_passwordRepository.IsPasswordExists("test"));
    }
}