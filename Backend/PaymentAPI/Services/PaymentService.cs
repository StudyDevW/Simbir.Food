using Middleware_Components.Broker;
using ORM_Components;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using PaymentAPI.Interfaces;

namespace PaymentAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger _logger;
        private readonly DataContext _dbcontext;
        private readonly IRabbitMQService _rabbitMQService;

        public PaymentService(IRabbitMQService rabbitMQService, DataContext dbcontext)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("database-service-logger");
            _dbcontext = dbcontext;
            _rabbitMQService = rabbitMQService;
        }

        private async Task CardValidation(Payment_Release dtoObj)
        {
            var selectedCard = _dbcontext.bankCardTable
                .Where(c => c.card_number == dtoObj.card_number)
                .FirstOrDefault();

            if (selectedCard == null)
            {
                throw new Exception("card_not_exist");
            }

            if (selectedCard.cvv != dtoObj.cvv)
            {
                PayTable payOperated = new PayTable()
                {
                    card_number = selectedCard.card_number,
                    pay_status = PayStatus.CardUnverified,
                    date = DateTime.UtcNow,
                    user_id = dtoObj.user_id
                };

                _dbcontext.payTable.Add(payOperated);
                await _dbcontext.SaveChangesAsync();

                throw new Exception("card_not_verified");
            }

            if (selectedCard.money_value < dtoObj.money_value)
            {
                PayTable payOperated = new PayTable()
                {
                    card_number = selectedCard.card_number,
                    pay_status = PayStatus.MoneyNotExist,
                    date = DateTime.UtcNow,
                    user_id = dtoObj.user_id
                };

                _dbcontext.payTable.Add(payOperated);
                await _dbcontext.SaveChangesAsync();

                throw new Exception("low_money_on_card");
            }
            else
            {
                selectedCard.money_value -= dtoObj.money_value;
                await _dbcontext.SaveChangesAsync();
            }
        }

        public async Task MoneyBack(PaymentOut dtoObj)
        {
            var selectedCard = _dbcontext.bankCardTable.Where(c => c.card_number == dtoObj.card_number).FirstOrDefault();

            if (selectedCard != null)
            {
                selectedCard.money_value += dtoObj.money_value;
                await _dbcontext.SaveChangesAsync();

                PayTable payOperated = new PayTable()
                {
                    card_number = selectedCard.card_number,
                    pay_status = PayStatus.MoneyBack,
                    date = DateTime.UtcNow,
                    user_id = dtoObj.user_id
                };

                _dbcontext.payTable.Add(payOperated);
                await _dbcontext.SaveChangesAsync();

                _rabbitMQService.SendMessage("payment_to_client_access_moneyback", new Payment_Success_Queue()
                {
                    money_value = dtoObj.money_value,
                    user_id = dtoObj.user_id
                });
            }
            else
            {
                MoneyBackError(dtoObj);
            }
        }

        public void MoneyBackError(PaymentOut dtoObj)
        {
            _rabbitMQService.SendMessage("payment_to_client_error", new Payment_Error_Queue()
            {
                user_id = dtoObj.user_id
            });
        }

        public async Task Pay(Payment_Release dtoObj)
        {
            await CardValidation(dtoObj);

            PayTable payOperated = new PayTable()
            {
                pay_status = PayStatus.Success,
                card_number = dtoObj.card_number,
                date = DateTime.UtcNow,
                user_id = dtoObj.user_id
            };

            _dbcontext.payTable.Add(payOperated);   
            await _dbcontext.SaveChangesAsync();

            //При успешной оплате пополняем счет клиента
            _rabbitMQService.SendMessage("payment_to_client", new Payment_Success_Queue()
            {
                money_value = dtoObj.money_value,
                user_id = dtoObj.user_id
            });
        }
    }
}
