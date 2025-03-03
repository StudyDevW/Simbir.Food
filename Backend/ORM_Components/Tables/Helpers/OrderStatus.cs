using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables.Helpers
{
    public enum OrderStatus
    {
        AfterPay,
        Accepted,
        Denied,
        Ready,
        WaitingForDelivery,
        CourierOnPlace,
        Delivered
    }
}
