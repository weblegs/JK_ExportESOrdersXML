using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeblegsClasses.api.channeladvisor.ShippingService;
namespace WeblegsClasses.ChannelAdvisor
{
    public class ShippingManager :Credentials
    {
        string accountID;
        public ShippingManager(string accountID)
        {
            this.accountID = accountID;
            SetShippingValues();
        }
        public ShipmentResponse[] SubmitOrderShipmentList(OrderShipmentList ShipmentList)
        {
            try
            {
                return ShipSer.SubmitOrderShipmentList(accountID, ShipmentList).ResultData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
