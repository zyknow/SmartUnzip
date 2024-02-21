using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Bing.Extensions;
using Microsoft.Extensions.Options;

namespace SmartUnzip.Core;

public class DefaultPasswordRepository : IPasswordRepository, IDisposable
{
    private readonly IOptions<SmartUnzipOptions> _smartUnzipOptions;
    private readonly HashSet<IUnzipPassword> passwords = [];
    private readonly object @lock = new();

    public virtual string PasswordJsonPath => "passwords.json";

    public DefaultPasswordRepository(IOptions<SmartUnzipOptions> smartUnzipOptions)
    {
        _smartUnzipOptions = smartUnzipOptions;
        LoadPassword();
    }

    public virtual bool IsPasswordExists(string password)
    {
        lock (@lock)
        {
            return passwords.Any(x => x.Value == password);
        }
    }

    public virtual List<IUnzipPassword> GetAllPasswords()
    {
        lock (@lock)
        {
            return passwords.ToList();
        }
    }

    public virtual void AddPassword(IUnzipPassword password)
    {
        lock (@lock)
        {
            if (password.Value.IsEmpty() || passwords.Any(x => x.Value == password.Value))
                return;
            passwords.Add(password);
        }
    }

    public virtual void AddPasswords(IEnumerable<IUnzipPassword> passwordList)
    {
        foreach (var password in passwordList)
        {
            AddPassword(password);
        }
    }

    public virtual IUnzipPassword? GetPassword(IUnzipPassword password)
    {
        lock (@lock)
        {
            return passwords.FirstOrDefault(p => p.Equals(password));
        }
    }

    public virtual IUnzipPassword? GetPassword(string passwordValue)
    {
        lock (@lock)
        {
            return passwords.FirstOrDefault(p => p.Value == passwordValue);
        }
    }

    public virtual void UpdatePassword(IUnzipPassword password)
    {
        lock (@lock)
        {
            var oldPassword = passwords.FirstOrDefault(p => p.Equals(password));
            if (oldPassword != null)
            {
                passwords.Remove(oldPassword);
                passwords.Add(password);
            }
        }
    }

    public virtual void UpdatePasswords(IEnumerable<IUnzipPassword> passwordList)
    {
        foreach (var password in passwordList)
        {
            UpdatePassword(password);
        }
    }

    public virtual void RemovePassword(IUnzipPassword password)
    {
        lock (@lock)
        {
            passwords.RemoveAll(x => x.Value == password.Value);
        }
    }

    public virtual void RemovePassword(string passwordValue)
    {
        lock (@lock)
        {
            var password = passwords.FirstOrDefault(p => p.Value == passwordValue);
            if (password != null)
            {
                passwords.Remove(password);
            }
        }
    }


    public virtual void SavePassword()
    {
        lock (@lock)
        {
            var passwords = GetAllPasswords();
            var passwordJson = JsonSerializer.Serialize(passwords);
            File.WriteAllText(PasswordJsonPath, passwordJson ?? string.Empty);
        }
    }

    public virtual void LoadPassword(string? path = null)
    {
        var passwordPath = path ?? PasswordJsonPath;

        lock (@lock)
        {
            if (!File.Exists(passwordPath))
                return;
            var passwordJson = File.ReadAllText(passwordPath);

            Type listType =
                typeof(List<>).MakeGenericType(new Type[] {_smartUnzipOptions.Value.UnzipPasswordDefineType});

            var obj = JsonSerializer.Deserialize(passwordJson, listType);

            var enumerable = obj as IEnumerable;


            foreach (var o in enumerable)
            {
                var password = o as IUnzipPassword;
                AddPassword(password);
            }
        }
    }

    public virtual void Clear()
    {
        lock (@lock)
        {
            passwords.Clear();
        }
    }

    public void Dispose()
    {
        SavePassword();
    }
}