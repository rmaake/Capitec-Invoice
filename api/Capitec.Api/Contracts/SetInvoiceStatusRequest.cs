using Capitec.Business.Enums;
using System.ComponentModel.DataAnnotations;

namespace Capitec.Api.Contracts
{
    public sealed class SetInvoiceStatusRequest
    {
        [EnumDataType(typeof(InvoiceStatus))]
        public InvoiceStatus Status { get; init; }
    }
}
