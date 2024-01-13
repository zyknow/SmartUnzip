namespace SmartUnzip.Core;

public interface IPasswordRepository
{
    bool IsPasswordExists(string password);
    List<UnzipPassword> GetAllPasswords();
    void AddPassword(UnzipPassword password);
    void AddPasswords(IEnumerable<UnzipPassword> passwordList);
    UnzipPassword? GetPassword(UnzipPassword password);
    UnzipPassword? GetPassword(string passwordValue);
    void UpdatePassword(UnzipPassword password);
    void UpdatePasswords(IEnumerable<UnzipPassword> passwordList);
    void RemovePassword(UnzipPassword password);
    void RemovePassword(string passwordValue);
    void Clear();
}