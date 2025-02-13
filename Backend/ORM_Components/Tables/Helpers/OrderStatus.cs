using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components.Tables.Helpers
{
    public enum OrderStatus
    {
        WaitingForPay,
        Accepted,
        Denied,
        Ready,
        WaitingForDelivery,
        CourierOnPlace,
        Delivered
    }
}
