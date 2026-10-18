using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions.CustomExceptions
{
    public class PersistenceException(string message, Exception innerException)
    : Exception(message, innerException);
}
