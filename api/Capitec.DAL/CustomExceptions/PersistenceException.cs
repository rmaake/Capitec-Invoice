namespace Capitec.DAL.CustomExceptions
{
    public class PersistenceException(string message, Exception innerException)
    : Exception(message, innerException);
}
