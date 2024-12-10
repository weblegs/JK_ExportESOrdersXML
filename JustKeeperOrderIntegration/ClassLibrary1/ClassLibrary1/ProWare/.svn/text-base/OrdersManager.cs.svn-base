using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeblegsClasses.api.prolog3pl.ProWaresService;
namespace WeblegsClasses.ProWare
{
    public class OrdersManager
    {
        private ProWaresService prowaresSevice = new ProWaresService();
        private  String systemId ;//= "clientdev";//"999system";
        private String password;//= "prolog";
        public OrdersManager(string strSystemID,string strPassword)
        {
            systemId = strSystemID;
            password = strPassword;
        }

        /// <summary>
        /// Submit Orders to ProWare
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public PLSubmitOrderResult PLSubmitOrders(List<PLOrder> orders)
        {
            try
            {
                // Set the orders on the arg object.
                PLSubmitOrderArgs args = new PLSubmitOrderArgs();
                args.SystemId = systemId;
                args.Password = password;

                args.Orders = orders.ToArray();

                // Call the service to submit the orders.
                //PLSubmitOrderResult result = prowaresSevice.PLSubmitOrder(args);

                return prowaresSevice.PLSubmitOrder(args);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public PLOrderStatusHeader[] PLGetOrderStatus(List<string> orderNumbers)
        {
            
            PLGetOrderStatusArgs args = new PLGetOrderStatusArgs();
            args.SystemId = systemId;
            args.Password = password;

            // Set the order numbers we want status updates on
            args.OrderNumbers = orderNumbers.ToArray();

            // Make the call
            PLGetOrderStatusResult result = prowaresSevice.PLGetOrderStatus(args);

            // Handle the result
            if (result.ProLogCode != PLGetOrderStatusCode.SUCCESS)
            {
                return null;
            }
            else
            {
                // Loop through the orders.
                return result.Orders;
                //foreach (PLOrderStatusHeader order in result.Orders)
                //{
                //    // Here is the order header info.
                //    //Console.WriteLine("Order: " + order.OrderNumber + " - " + order.CompletedDate.ToString());

                //    foreach (PLOrderStatusShipment shipment in order.Shipments)
                //    {
                //        foreach (PLOrderStatusPackage package in shipment.Packages)
                //        {
                //            // Here are the tracking numbers.
                //            //Console.WriteLine("\t" + package.TrackingNumber);
                //        }
                //    }
                //}
            }
        }

    }
}
