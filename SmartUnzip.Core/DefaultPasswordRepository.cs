namespace SmartUnzip.Core;

public class DefaultPasswordRepository : IPasswordRepository, ISingletonDependency
{
    private readonly HashSet<UnzipPassword> passwords = [];
    private readonly object @lock = new();

    public virtual bool IsPasswordExists(string password)
    {
        lock (@lock)
        {
            return passwords.Any(x => x.Value == password);
        }
    }

    public virtual List<UnzipPassword> GetAllPasswords()
    {
        lock (@lock)
        {
            return passwords.ToList();
        }
    }

    public virtual void AddPassword(UnzipPassword password)
    {
        lock (@lock)
        {
            passwords.Add(password);
        }
    }

    public virtual void AddPasswords(IEnumerable<UnzipPassword> passwordList)
    {
        foreach (var password in passwordList)
        {
            AddPassword(password);
        }
    }

    public virtual UnzipPassword? GetPassword(UnzipPassword password)
    {
        lock (@lock)
        {
            return passwords.FirstOrDefault(p => p.Equals(password));
        }
    }

    public virtual UnzipPassword? GetPassword(string passwordValue)
    {
        lock (@lock)
        {
            return passwords.FirstOrDefault(p => p.Value == passwordValue);
        }
    }

    public virtual void UpdatePassword(UnzipPassword password)
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

    public virtual void UpdatePasswords(IEnumerable<UnzipPassword> passwordList)
    {
        foreach (var password in passwordList)
        {
            UpdatePassword(password);
        }
    }

    public virtual void RemovePassword(UnzipPassword password)
    {
        lock (@lock)
        {
            passwords.Remove(password);
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

    public virtual void Clear()
    {
        lock (@lock)
        {
            passwords.Clear();
        }
    }
}