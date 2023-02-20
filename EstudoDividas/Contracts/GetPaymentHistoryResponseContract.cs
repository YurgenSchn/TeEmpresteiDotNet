using EstudoDividas.Contracts.ReturnTypes;
using EstudoDividas.Data.MySQL.Entities;

namespace EstudoDividas.Contracts
{
    public class GetPaymentHistoryResponseContract : MinimalResponseContract
    {
        public List<PaymentReturnType> payments { get; set; }
    }
}
