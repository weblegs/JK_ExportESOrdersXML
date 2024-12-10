using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using WeblegsClasses.api.channeladvisor.InventoryService;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Configuration;
namespace WeblegsClasses.ChannelAdvisor
{

    public class InventoryManager : Credentials
    {
        private static string accountID;
        private string currentPath = string.Empty;
        static InventoryService Serv;
        public InventoryManager(string AccountID)
        {
            accountID = AccountID;
            SetInventoryValues();
            currentPath = Directory.GetCurrentDirectory();
            Serv = InvSer;
        }

        public string[] GetFilteredSKUS(InventoryItemCriteria InvCrit)
        {
            try
            {
                return InvSer.GetFilteredSkuList(accountID, InvCrit, InventoryItemSortField.Sku, SortDirection.Ascending).ResultData;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public AttributeInfo[] GetInventoryItemAttributeList(string sku)
        {
            try
            {
                return InvSer.GetInventoryItemAttributeList(accountID, sku).ResultData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public SynchInventoryItemResponse[] SynchInventoryItemList(InventoryItemSubmit[] InvItemList)
        //{
        //    try
        //    {
        //        return InvSer.SynchInventoryItemList(accountID, InvItemList).ResultData;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        public void SynchInventoryItemList(InventoryItemSubmit[] InvItemList)
        {
            try
            {
                List<InventoryItemSubmit> LstInventoryItemSubmit = new List<InventoryItemSubmit>();
                int counter = 0;
                foreach (InventoryItemSubmit Item in InvItemList)
                {
                    LstInventoryItemSubmit.Add(Item);
                    counter++;
                    if (counter == 100)
                    {
                        counter = 0;

                        AsyncItemSubmitList Updater = new AsyncItemSubmitList(AsyncSynchInventoryItemList);
                        Updater.BeginInvoke(LstInventoryItemSubmit, new AsyncCallback(CallbackMethod_SynchInventoryItemList), "");
                        Thread.Sleep(1);

                        LstInventoryItemSubmit = new List<InventoryItemSubmit>();

                    }
                }
                if (LstInventoryItemSubmit.Count > 0)
                {
                    AsyncItemSubmitList Updater = new AsyncItemSubmitList(AsyncSynchInventoryItemList);
                    Updater.BeginInvoke(LstInventoryItemSubmit, new AsyncCallback(CallbackMethod_SynchInventoryItemList), "");

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public InventoryItemResponse[] GetInventoryItemList(string[] skuList)
        {
            try
            {

                int pageCount = (int)Math.Ceiling((double.Parse((skuList.Length.ToString())) / 100));
                string[] skuListToBeSent;
                List<string> lstSKU = new List<string>();
                lstSKU = skuList.ToList();
                List<InventoryItemResponse> lstResponse = new List<InventoryItemResponse>();
                for (int count = 0; count < pageCount; count++)
                {
                    if (count == (pageCount - 1))
                        skuListToBeSent = lstSKU.GetRange(count * 100, (lstSKU.Count - (count * 100))).ToArray();
                    else
                        skuListToBeSent = lstSKU.GetRange(count * 100, 100).ToArray();

                    try
                    {

                        lstResponse = lstResponse.Concat(InvSer.GetInventoryItemList(accountID, skuListToBeSent).ResultData.ToList()).ToList();
                    }
                    catch
                    {

                    }
                }
                return lstResponse.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public InventoryItemResponse[] GetFilteredInventoryItemList(InventoryItemCriteria IICInvCrit, InventoryItemDetailLevel IIDLDetailLevl)
        {
            try
            {
                IICInvCrit.PageNumber = 0;
                IICInvCrit.PageSize = 100;
                InventoryItemResponse[] ArrResponse = null;
                List<InventoryItemResponse> lstResp = new List<InventoryItemResponse>();

                do
                {
                    IICInvCrit.PageNumber = IICInvCrit.PageNumber + 1;
                    try
                    {
                        ArrResponse = InvSer.GetFilteredInventoryItemList(accountID, IICInvCrit, IIDLDetailLevl, InventoryItemSortField.Sku, SortDirection.Ascending).ResultData;
                        lstResp = lstResp.Concat(ArrResponse.ToList()).ToList();
                    }
                    catch { }
                } while (ArrResponse.Length == 100);

                return lstResp.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public QuantityInfoResponse GetInventoryItemQuantityInfo(string sku)
        {
            try
            {
                return InvSer.GetInventoryItemQuantityInfo(accountID, sku).ResultData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ImageInfoResponse[] GetInventoryItemImageList(string sku)
        {
            try
            {
                return InvSer.GetInventoryItemImageList(accountID, sku).ResultData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public VariationInfo GetInventoryItemVariationInfo(string sku)
        {
            try
            {
                return InvSer.GetInventoryItemVariationInfo(accountID, sku).ResultData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateInventoryItemQuantityAndPriceList(InventoryItemQuantityAndPrice[] itemQtyAndPriceList)
        {

            APIResultOfArrayOfUpdateInventoryItemResponse UpdateResult = null;
            List<InventoryItemQuantityAndPrice> lstItemQuantityAndPrice = new List<InventoryItemQuantityAndPrice>();
            // taken List which will be converted into array afterwards

            InventoryItemQuantityAndPrice ItemQuantityAndPrice;
            int Counter = 0, timer = 0;

            foreach (InventoryItemQuantityAndPrice Item in itemQtyAndPriceList)
            {
                try
                {
                    Counter++;
                    timer++;
                    lstItemQuantityAndPrice.Add(Item);
                    if (Counter == 100)
                    {
                        Counter = 0;
                        //Calling CA API to Update Inventory 
                        try
                        {
                            
                            //AsyncQtyUpdater Updater = new AsyncQtyUpdater(AsyncUpdateInventoryItemQuantityAndPriceList);
                            //Updater.BeginInvoke(lstItemQuantityAndPrice, new AsyncCallback(CallbackMethod), "");
                            //Thread.Sleep(1);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncUpdateInventoryItemQuantityAndPriceList), lstItemQuantityAndPrice);
                            
                        }
                        catch { }
                        lstItemQuantityAndPrice = new List<InventoryItemQuantityAndPrice>();
                    }
                }
                catch (Exception ex)
                {
                    //throw ex;
                }
            }

            try
            {

                //Calling CA API to Update Inventory 
                //we have to send array of InventoryItemQuantityAndPrice 
                //Dont Uncomment below line of Code till we Test our softwar fully. 
                if (lstItemQuantityAndPrice.Count > 0)
                {
                    //AsyncQtyUpdater Updater = new AsyncQtyUpdater(AsyncUpdateInventoryItemQuantityAndPriceList);
                    //Updater.BeginInvoke(lstItemQuantityAndPrice, new AsyncCallback(CallbackMethod), "");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncUpdateInventoryItemQuantityAndPriceList), lstItemQuantityAndPrice);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

       static void AsyncUpdateInventoryItemQuantityAndPriceList(object lstItemQuantityAndPrice)
        {
            string conection = "";
            try
            {

                conection = ConfigurationSettings.AppSettings["Con_Stg"].ToString();

                APIResultOfArrayOfUpdateInventoryItemResponse UpdateResult = Serv.UpdateInventoryItemQuantityAndPriceList(accountID, ((List<InventoryItemQuantityAndPrice>)lstItemQuantityAndPrice).ToArray());
                string status = UpdateResult.Status.ToString();

                foreach (UpdateInventoryItemResponse Resp in UpdateResult.ResultData)
                {
                    if (Resp.Result)
                    {
                        foreach (InventoryItemQuantityAndPrice InvSub in ((List<InventoryItemQuantityAndPrice>)lstItemQuantityAndPrice))
                        {
                            
                            if (InvSub.Sku == Resp.Sku)
                            {
                                //save to db
                                SavePriceChangesToDb(InvSub);
                                
                                break;
                            }
                        }
                       
                    }
                }
                // string Message = UpdateResult.Message.ToString();
                //Operations.SendMail("nephrodoc2@gmail.com", "Brooks", status );
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("timed out"))
                    AsyncUpdateInventoryItemQuantityAndPriceList(lstItemQuantityAndPrice);
                //Operations.SendMail("yatin@weblegs.co.uk", "Brookes - core Error", ex.ToString() + "</br> conn - " + conection);
            }
        }
        static void CallbackMethod(IAsyncResult ar)
        {
            // Retrieve the delegate.
            AsyncResult result = (AsyncResult)ar;
            AsyncQtyUpdater caller = (AsyncQtyUpdater)result.AsyncDelegate;
            int threadId = 0;
            caller.EndInvoke(ar);
        }

        void AsyncSynchInventoryItemList(List<InventoryItemSubmit> lstInventoryItemSubmit)
        {
            try
            {

                APIResultOfArrayOfSynchInventoryItemResponse res = InvSer.SynchInventoryItemList(accountID, lstInventoryItemSubmit.ToArray());


                //if (res.Status == ResultStatus.Success)
                //{

                //}
                //else
                //{
                //    AsyncSynchInventoryItemList(lstInventoryItemSubmit);
                //}
                // string Message = UpdateResult.Message.ToString();
                //Operations.SendMail("nephrodoc2@gmail.com", "Brooks", status );
            }
            catch (Exception ex)
            {
                //if (ex.Message.Contains("timed out"))
                AsyncSynchInventoryItemList(lstInventoryItemSubmit);
                //Operations.SendMail("nephrodoc2@gmail.com", "Error", ex.ToString());
            }
        }
        static void CallbackMethod_SynchInventoryItemList(IAsyncResult ar)
        {
            // Retrieve the delegate.
            AsyncResult result = (AsyncResult)ar;
            AsyncItemSubmitList caller = (AsyncItemSubmitList)result.AsyncDelegate;
            //int threadId = 0;
            caller.EndInvoke(ar);
        }

        public static void SavePriceChangesToDb(InventoryItemQuantityAndPrice InvSub)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationSettings.AppSettings["Con_Stg"].ToString()))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateQuantity", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (cmd.Connection.State == ConnectionState.Closed)
                        cmd.Connection.Open();
                    cmd.Parameters.Add("@SKU", SqlDbType.NVarChar).Value = InvSub.Sku;
                    cmd.Parameters.Add("@Qty", SqlDbType.NVarChar).Value = InvSub.QuantityInfo.Total.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
    delegate void AsyncQtyUpdater(List<InventoryItemQuantityAndPrice> lstItemQuantityAndPrice);
    delegate void AsyncItemSubmitList(List<InventoryItemSubmit> lstInventoryItemSubmit);

}
