namespace Capitec.DAL.CustomExceptions
{

    public sealed class CustomerEmailConflictException(string message) : Exception(message);
}
