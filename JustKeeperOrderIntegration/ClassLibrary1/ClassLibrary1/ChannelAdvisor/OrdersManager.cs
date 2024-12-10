using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using WeblegsClasses.api.channeladvisor.OrderService;
namespace WeblegsClasses.ChannelAdvisor
{
    public class OrdersManager : Credentials
    {
        private string Account = string.Empty;
        string ordersNotExportedFileName = string.Empty;
        string currentPath = string.Empty;
        public OrdersManager(string accountID)
        {
            Account = accountID;
            ordersNotExportedFileName = "ClientOrderIdentifiersNotExported.csv";
            currentPath = Directory.GetCurrentDirectory();
            SetOrderValues();
        }

        // Will return all Orders fetched from Channel
        public OrderResponseItem[] GetOrdersList(OrderCriteria OrdCrit)
        {
            try
            {
                List<OrderResponseItem> OrderResponseItemLst = new List<OrderResponseItem>();
                APIResultOfArrayOfOrderResponseItem Result;
                do
                {
                    Result = OrdSer.GetOrderList(Account, OrdCrit);
                    OrderResponseItemLst = OrderResponseItemLst.Concat(Result.ResultData.ToList()).ToList();

                    OrdCrit.PageNumberFilter = OrdCrit.PageNumberFilter + 1;

                } while (Result.ResultData.Length % OrdCrit.PageSize == 0 && Result.ResultData.Length != 0);

                return OrderResponseItemLst.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        /// <summary>
        /// Will set Order status to exported
        /// </summary>
        /// <param name="Account"></param>
        /// <param name="clientOrderIdentifiers"></param>
        /// <param name="markAsExported"></param>
        /// <returns></returns>
        public APIResultOfArrayOfBoolean SetOrdersExportStatus(string[] clientOrderIdentifiers, bool markAsExported)
        {
            StreamWriter clientOrderIdentifiersFile = null;
            APIResultOfArrayOfBoolean Result = null;
            try
            {
                string SKU = "";
                Result = OrdSer.SetOrdersExportStatus(Account, clientOrderIdentifiers, markAsExported);

                //if (File.Exists(currentPath + "/" + ordersNotExportedFileName))
                //{
                //    clientOrderIdentifiersFile = new StreamWriter(currentPath + "/" + ordersNotExportedFileName, true);
                //}
                //else
                //{
                //    clientOrderIdentifiersFile = new StreamWriter(currentPath + "/" + ordersNotExportedFileName, false);
                //    clientOrderIdentifiersFile.WriteLine("clientOrderIdentifiers,markAsExported");
                //}

                //for (int countStatus = 0; countStatus < Result.ResultData.Length; countStatus++)
                //{
                //    if (!Result.ResultData[countStatus])
                //    {
                //        SKU = clientOrderIdentifiers[countStatus];
                //        clientOrderIdentifiersFile.WriteLine("\"" + SKU + "\",\"" + markAsExported + "\"");
                //    }
                //}
                //clientOrderIdentifiersFile.Close();
            }
            catch (Exception ex)
            {

                if (File.Exists(currentPath + "/" + ordersNotExportedFileName))
                {
                    clientOrderIdentifiersFile = new StreamWriter(currentPath + "/" + ordersNotExportedFileName, true);
                }
                else
                {
                    clientOrderIdentifiersFile = new StreamWriter(currentPath + "/" + ordersNotExportedFileName, false);
                    clientOrderIdentifiersFile.WriteLine("clientOrderIdentifiers,markAsExported");
                }

                foreach (string clientOrderIdentifier in clientOrderIdentifiers)
                {
                    clientOrderIdentifiersFile.WriteLine("\"" + clientOrderIdentifier + "\",\"" + markAsExported + "\"");
                }
                clientOrderIdentifiersFile.Close();
                throw ex;
            }
            return Result;
        }

        /// <summary>
        /// To Update Order Export status which were existing in CSV file,
        /// </summary>
        public void ProcessPendingExportStatus()
        {
            try
            {
                if (File.Exists(currentPath + "/" + ordersNotExportedFileName))
                {
                    DataSet ds = Operations.ReadCSVFile(currentPath, ordersNotExportedFileName);
                    File.Delete(currentPath + "/" + ordersNotExportedFileName);
                    string[] clientOrderIdentifiers;

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        clientOrderIdentifiers = (from row in ds.Tables[0].AsEnumerable()
                                                  where row.Field<string>("markAsExported").ToLower() == "true"
                                                  select row.Field<string>("clientOrderIdentifiers")).ToArray();
                        if (clientOrderIdentifiers.Length > 0)
                            SetOrdersExportStatus(clientOrderIdentifiers, true);

                        clientOrderIdentifiers = null;
                        clientOrderIdentifiers = (from row in ds.Tables[0].AsEnumerable()
                                                  where row.Field<string>("markAsExported").ToLower() == "false"
                                                  select row.Field<string>("clientOrderIdentifiers")).ToArray();

                        if (clientOrderIdentifiers.Length > 0)
                            SetOrdersExportStatus(clientOrderIdentifiers, true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Will Update Order and Return its Status Array
        /// </summary>
        /// <param name="OrdUpSubArr"></param>
        /// <returns></returns>
        public OrderUpdateResponse[] UpdateOrderList(OrderUpdateSubmit[] OrdUpSubArr)
        {
            try
            {
                return OrdSer.UpdateOrderList(Account, OrdUpSubArr).ResultData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


      



    }
}
