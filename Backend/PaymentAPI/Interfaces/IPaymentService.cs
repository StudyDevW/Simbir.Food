using ORM_Components.DTO.PaymentAPI;

namespace PaymentAPI.Interfaces
{
    public interface IPaymentService
    {
        public Task Pay(Payment_Release dtoObj);

        public Task MoneyBack(PaymentOut dtoObj);

        void MoneyBackError(PaymentOut dtoObj);
    }
}
