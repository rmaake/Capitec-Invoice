namespace Capitec.DAL.CustomExceptions
{
    public sealed class IdempotencyConflictException(string message) : Exception(message);
}
