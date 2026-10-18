using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions.CustomExceptions
{
    public sealed class IdempotencyConflictException(string message) : Exception(message);
}
