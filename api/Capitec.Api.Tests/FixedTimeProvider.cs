using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Api.Tests
{
    public sealed class FixedTimeProvider(DateTimeOffset currentTime) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => currentTime;
    }

}
