using System;
using System.Collections.Generic;

namespace SmartUnzip.Core;

public interface IPasswordRepository : IDisposable
{
    bool IsPasswordExists(string password);
    List<IUnzipPassword> GetAllPasswords();
    void AddPassword(IUnzipPassword password);
    void AddPasswords(IEnumerable<IUnzipPassword> passwordList);
    IUnzipPassword? GetPassword(IUnzipPassword password);
    IUnzipPassword? GetPassword(string passwordValue);
    void UpdatePassword(IUnzipPassword password);
    void UpdatePasswords(IEnumerable<IUnzipPassword> passwordList);
    void RemovePassword(IUnzipPassword password);
    void RemovePassword(string passwordValue);
    void Clear();
    string PasswordJsonPath { get; }
    void SavePassword();
    void LoadPassword(string? path = null);
}