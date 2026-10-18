using Capitec.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capitec.Application.Abstractions
{
    public interface INotificationRepository
    {
        Task<bool> ExistsAsync(int invoiceId, int policyWindow, CancellationToken cancellationToken);
        Task<IReadOnlyList<SimulatedNotification>> GetLatestAsync(int limit, CancellationToken cancellationToken);
        Task<int> CountAsync(CancellationToken cancellationToken);
        void Add(SimulatedNotification notification);
    }
}
