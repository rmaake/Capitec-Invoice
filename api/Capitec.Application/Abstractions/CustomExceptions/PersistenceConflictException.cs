using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions.CustomExceptions
{
    public sealed class PersistenceConflictException(string message, Exception innerException)
    : PersistenceException(message, innerException);
}
