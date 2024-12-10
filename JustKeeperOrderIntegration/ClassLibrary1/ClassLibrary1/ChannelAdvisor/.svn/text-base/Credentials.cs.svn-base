using System;
using System.Collections.Generic;
using System.Text;
using WeblegsClasses.api.channeladvisor.OrderService;
using WeblegsClasses.api.channeladvisor.InventoryService;
using WeblegsClasses.api.channeladvisor.ShippingService;
namespace WeblegsClasses
{
     public abstract class Credentials
    {
        private  string DeveloperKey = "46b524bb-a4d6-4fbf-8120-924ac56e0805";
        private  string Password = "Tortoise1?";
        
        protected InventoryService InvSer;
        protected OrderService OrdSer;
        protected ShippingService ShipSer;
        //Setting values for InventoryService Object
        protected void SetInventoryValues()
        {
           
            InvSer =  new InventoryService();
            InvSer.APICredentialsValue = new WeblegsClasses.api.channeladvisor.InventoryService.APICredentials();
            InvSer.APICredentialsValue.DeveloperKey = DeveloperKey;
            InvSer.APICredentialsValue.Password = Password;
            InvSer.Url = " https://api.channeladvisor.com/ChannelAdvisorAPI/v5/InventoryService.asmx";
        }

        //Setting values for OrderService Object
        protected void SetOrderValues()
        { 
            OrdSer = new OrderService();
            OrdSer.APICredentialsValue = new WeblegsClasses.api.channeladvisor.OrderService.APICredentials();
            OrdSer.APICredentialsValue.DeveloperKey = DeveloperKey;
            OrdSer.APICredentialsValue.Password = Password;
            OrdSer.Url = "https://api.channeladvisor.com/ChannelAdvisorAPI/v5/OrderService.asmx";
        }

        //Setting values for OrderService Object
        protected void SetShippingValues()
        {
            ShipSer = new ShippingService();
            ShipSer.APICredentialsValue = new WeblegsClasses.api.channeladvisor.ShippingService.APICredentials();
            ShipSer.APICredentialsValue.DeveloperKey = DeveloperKey;
            ShipSer.APICredentialsValue.Password = Password;
            ShipSer.Url = "https://api.channeladvisor.com/ChannelAdvisorAPI/v5/ShippingService.asmx";
        }
    }

}
