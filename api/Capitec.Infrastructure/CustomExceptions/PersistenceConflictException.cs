namespace Capitec.DAL.CustomExceptions
{
    public sealed class PersistenceConflictException(string message, Exception innerException)
    : PersistenceException(message, innerException);
}
