namespace Finance.Web;

public class AuthState
{
    public string? Token { get; private set; }
    public bool IsLoggedIn
    {
        get
        {
            return Token != null;
        }
    }

    public void Login(string token)
    {
        Token = token;
    }
    public void Logout()
    {
        Token = null;
    }
}