using Capitec.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Services.Abstractions
{
    public interface IOverdueNotificationService
    {
        Task<IReadOnlyList<NotificationDto>> GetLatestAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<NotificationDto>> SimulateAsync(CancellationToken cancellationToken);
    }
}
