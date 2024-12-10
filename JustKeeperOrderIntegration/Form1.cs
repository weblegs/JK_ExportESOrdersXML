using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using WeblegsClasses.ChannelAdvisor;
using WeblegsClasses.api.channeladvisor.OrderService;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using JustKeeperOrderIntegration.Classes;
using System.IO;
using System.Net;
using System.Globalization;
using System.Text;
using WeblegsClasses;
using System.Linq;
using System.Text.RegularExpressions;

namespace JustKeeperOrderIntegration
{
    public partial class Form1 : Form
    {

        Dictionary<string, string> LstXmlDoc = new Dictionary<string, string>();
        List<string> channelordernumber = new List<string>();
        string Request = "", ChannelResponse = "";
        int ChannelOrderNumberToInsert = 0, JustKeeperorderNumberToInsert = 0;
        SqlConnection con = new SqlConnection();
        OrdersManager OMJUSTKEEP;
        List<string> dd = new List<string>();
        DataSet DsCountry;
        string[] EUCountries = { "Azerbaijan", "Armenia", "Andorra", "Albania", "Austria", "Belgium", "Belarus", "Bosnia & Herzegovina", "Bulgaria", "Croatia", "Cyprus", "Czech Republic", "Denmark", "Estonia", "Finland", "France", "Georgia", "Germany", "Greece", "Hungary", "Iceland", "Ireland", "Italy", "Kosovo", "Latvia", "Liechtenstein", "Lithuania", "Luxembourg", "Macedonia", "Malta", "Moldova", "Monaco", "Montenegro", "Netherlands", "The Netherlands", "Norway", "Poland", "Portugal", "Russia", "Romania", "San Marino", "Slovakia", "Serbia", "Spain", "Slovenia", "Spain", "Switzerland", "Turkey", "Sweden", "Ukraine", "Vatican City State" };
        public Form1()
        {
            OMJUSTKEEP = new OrdersManager("25750c27-fcc3-4f29-bb41-c02d1c974df6"); //255198da-5829-4a1f-9576-26dfe77b14e1
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            timer1.Interval = 1000 * 60 * 12;
            timer1.Start();

            con.ConnectionString = ConfigurationManager.AppSettings["Con_Stg"];
           // con.ConnectionString = @"server=WIN-UFARGF5B64Q\SQLCHANNELBO;Database=JustKeeper;integrated security=true"; // Live



            /*****************module 1 for get orders from channel**************************/
            try
            {
                //channelordernumber.Add("404-2100702-7248");
                //OMJUSTKEEP.SetOrdersExportStatus(channelordernumber.ToArray(), true);
                OrderResponseItem[] orderresponse = GetOrders();



                if (orderresponse.Length > 0)
                {
                    /*****************module 1.1 make xml of orders*************/
                    try
                    {
                        /*********************code to get all country name acc to country code*******************/

                        try
                        {
                            DsCountry = Operations.ReadCSVFile(Application.StartupPath, "countrycodes.csv");
                        }
                        catch (Exception exp)
                        {

                        }
                        Dictionary<string, string> requestxml = GetRequestOrderXml(orderresponse);

                        /************************************************end******************************************/
                        if (requestxml.Count > 0)
                        {
                            /*****************module 1.1.1 make httpwebrequest of xml*************/
                            try
                            {

                                GetWebRequest(requestxml);

                                if (channelordernumber.Count > 0)
                                {
                                    /*****************module 1.1.1.1 make set status of order that submit on justkeep*************/
                                    try
                                    {
                                        OMJUSTKEEP.SetOrdersExportStatus(channelordernumber.ToArray(), true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Operations.SendMail("nephrodoc15@gmail.com", "justkeeper order integration (SPAIN) application in module 1.1.1.1", ex.Source + "</br>" + ex.Message);
                                    }
                                }

                                /************************end*********************************************/
                            }

                            catch (Exception ex)
                            {
                                Operations.SendMail("nephrodoc15@gmail.com", "justkeeper order integration (SPAIN) application in module 1.1.1", ex.Source + "</br>" + ex.Message);
                            }
                            /*********************************end********************************/
                        }
                    }
                    catch (Exception ex)
                    {
                        Operations.SendMail("nephrodoc15@gmail.com", "justkeeper order integration (SPAIN) application in module 1.1", ex.Source + "</br>" + ex.Message);
                    }
                    /*******************end*******************************************/
                }

            }
            catch (Exception exp)
            {
                Operations.SendMail("nephrodoc15@gmail.com", "justkeeper order integration (SPAIN) application in module 1", exp.Source + "</br>" + exp.Message);

                /**************************end****************************************/
            }

          
        }

        private OrderResponseItem[] GetOrders()
        {

            Orders order = new Orders();
            OrderResponseItem[] orderresponseitem;
            OrderCriteria OC = new OrderCriteria(); //Define the OrderCriteria 
            //int[] abc = { 20597443 };
            //OC.OrderIDList =abc;
            OC.DetailLevel = DetailLevelType.Complete;
            OC.ExportState = ExportStateType.NotExported;
            OC.PaymentStatusFilter = PaymentStatusCode.Cleared;
            OC.OrderStateFilter = OrderStateCode.Active;
            OC.ShippingStatusFilter = ShippingStatusCode.Unshipped;
            OC.OrderCreationFilterBeginTimeGMT = Convert.ToDateTime("01/04/2015");
            OC.PageSize = 20;
            OC.PageNumberFilter = 1;

            orderresponseitem = OMJUSTKEEP.GetOrdersList(OC);
            return orderresponseitem;


        }

        private Dictionary<string, string> GetRequestOrderXml(OrderResponseItem[] orderresponse)
        {
            string countrycode = ""; string Region = "";
            string xml = "";
            SiteToken token;
            decimal shippingcost = 0;
            string DeliveryMethod = "";
            string Carrier = "", Class = "";
            decimal discount = 0;
            string paymentmethod = "";
            XmlDocument Requestxml = new XmlDocument();
            Orders order = new Orders();
            Customerdetails customerDetails = new Customerdetails();
            Orderdetails orderdetail = new Orderdetails();
            Orderheader OrderHeader = new Orderheader();
            Orderdelivery OrderDelivery = new Orderdelivery();
            string shippcountries = "";
            int i = 0;
            foreach (OrderResponseDetailHigh Response in orderresponse)
            {
                /**********************GeneratingOrderXmlForRequest***************/
                try
                {
                    if (Response.OrderState != OrderStateCode.Cancelled)
                    {
                        shippingcost = 0;
                        i = i + 1;
                        countrycode = Response.ShippingInfo.CountryCode;
                        if (countrycode.ToLower() == "gb")
                            countrycode = "UK";
                        else if (countrycode.ToLower() == "aq")
                            countrycode = "AB";
                        else if (countrycode.ToLower() == "mp")
                            countrycode = "XU";
                        else if (countrycode.ToLower() == "im")
                            countrycode = "IMN";

                        shippcountries = GetCountry(Response.ShippingInfo.CountryCode);
                        /***************paymentmethod*******************/
                        paymentmethod = Response.PaymentInfo.PaymentType;
                        /*****************end*************************/

                        Region = Response.ShippingInfo.RegionDescription;


                        #region CustomerDetail

                        customerDetails.AccountNo = "JK001";
                        customerDetails.Password = "Justkeepers2";
                        customerDetails.SystemID = "JK001SYSID";

                        #endregion

                        #region OrderHeader

                        ChannelOrderNumberToInsert = Response.OrderID;
                        //MessageBox.Show(Response.OrderID.ToString());

                        OrderHeader.RSSCustnumber = "";
                        OrderHeader.Postcode = Response.ShippingInfo.PostalCode;
                        OrderHeader.Orderdate = Convert.ToDateTime(Response.OrderTimeGMT).ToString("yyyy/MM/dd").Replace("-", "/");

                        OrderHeader.Ordernumber = Response.OrderID;
                        OrderHeader.AddressLine1 = Regex.Replace(Response.ShippingInfo.AddressLine1, @"\s+", " "); ;
                        OrderHeader.AddressLine2 = Regex.Replace(Response.ShippingInfo.AddressLine2, @"\s+", " "); ;
                        OrderHeader.AddressLine3 = "";
                        OrderHeader.AddressLine4 = Response.ShippingInfo.City;
                        OrderHeader.AddressLine5 = Region;
                        OrderHeader.Company = Response.ShippingInfo.CompanyName;
                        OrderHeader.Contact = Response.ShippingInfo.PhoneNumberDay;
                        OrderHeader.Country = countrycode;//string.IsNullOrEmpty(Response.BillingInfo.CountryCode) ? Response.ShippingInfo.CountryCode : Response.BillingInfo.CountryCode;
                        OrderHeader.Customerforename = Response.ShippingInfo.FirstName;//Response.BillingInfo.FirstName;
                        OrderHeader.Customerinitial = "";
                        OrderHeader.Customersurname = Response.ShippingInfo.LastName;//Response.BillingInfo.LastName;
                        OrderHeader.Customertitle = "";//string.IsNullOrEmpty(Response.BillingInfo.Suffix) ? Response.ShippingInfo.Suffix : Response.BillingInfo.Suffix;//Response.BillingInfo.Title;
                        OrderHeader.DeliveryIns = Response.ShippingInfo.ShippingInstructions;
                        OrderHeader.Deliverysame = 0;
                        OrderHeader.Email = Response.BuyerEmailAddress;
                        //"Channel Advisor";//

                        #region  PhoneNumbers

                        if (Response.ShippingInfo.PhoneNumberEvening.Length == 0)
                        {
                            if (Response.ShippingInfo.PhoneNumberDay.Length == 0)
                            {
                                if (Response.BillingInfo.PhoneNumberDay.Length == 0)
                                {
                                    OrderDelivery.Dhomenumber = Response.BillingInfo.PhoneNumberEvening;
                                    OrderDelivery.Dmobilenumber = Response.BillingInfo.PhoneNumberEvening;
                                    OrderHeader.Mobilenumber = Response.BillingInfo.PhoneNumberEvening;
                                    OrderHeader.Homenumber = Response.BillingInfo.PhoneNumberEvening;
                                }
                                else
                                {
                                    OrderDelivery.Dhomenumber = Response.BillingInfo.PhoneNumberDay;
                                    OrderDelivery.Dmobilenumber = Response.BillingInfo.PhoneNumberDay;
                                    OrderHeader.Mobilenumber = Response.BillingInfo.PhoneNumberDay;
                                    OrderHeader.Homenumber = Response.BillingInfo.PhoneNumberDay;
                                }
                            }
                            else
                            {
                                OrderDelivery.Dhomenumber = Response.ShippingInfo.PhoneNumberDay;
                                OrderDelivery.Dmobilenumber = Response.ShippingInfo.PhoneNumberDay;
                                OrderHeader.Mobilenumber = Response.ShippingInfo.PhoneNumberDay;
                                OrderHeader.Homenumber = Response.ShippingInfo.PhoneNumberDay;
                            }
                        }

                        else
                        {
                            OrderDelivery.Dhomenumber = Response.ShippingInfo.PhoneNumberEvening;
                            OrderDelivery.Dmobilenumber = Response.ShippingInfo.PhoneNumberEvening;
                            OrderHeader.Mobilenumber = Response.ShippingInfo.PhoneNumberEvening;
                            OrderHeader.Homenumber = Response.ShippingInfo.PhoneNumberEvening;
                        }
                        #endregion

                        OrderHeader.Totalvalue = Response.TotalOrderAmount;
                        OrderHeader.Worknumber = 0;

                        #endregion OrderHeader

                        #region OrderDelivery

                        OrderDelivery.DaddressLine1 = Regex.Replace(Response.ShippingInfo.AddressLine1, @"\s+", " "); ;
                        OrderDelivery.DaddressLine2 = Regex.Replace(Response.ShippingInfo.AddressLine2, @"\s+", " "); ;
                        OrderDelivery.DaddressLine3 = "";
                        OrderDelivery.DaddressLine4 = Response.ShippingInfo.City;
                        OrderDelivery.DaddressLine5 = Region;
                        OrderDelivery.Dcompany = Response.ShippingInfo.CompanyName;
                        OrderDelivery.Dcontact = Response.ShippingInfo.PhoneNumberDay;
                        OrderDelivery.Dcountry = countrycode;//string.IsNullOrEmpty(Response.ShippingInfo.CountryCode) ? Response.BillingInfo.CountryCode : Response.ShippingInfo.CountryCode;
                        OrderDelivery.Dcustomerforename = Response.ShippingInfo.FirstName;
                        OrderDelivery.Dcustomerinitial = "";// Response.ShippingInfo.Suffix;
                        OrderDelivery.Dcustomersurname = Response.ShippingInfo.LastName;
                        OrderDelivery.Dcustomertitle = ""; //Response.ShippingInfo.Title;
                        OrderDelivery.Demail = Response.BuyerEmailAddress;
                        OrderDelivery.Dpostcode = Response.ShippingInfo.PostalCode;
                        OrderDelivery.Dworknumber = 0;
                        OrderHeader.Currency = 1; //2


                        #endregion
                        #region invoice
                        OrderLineItemInvoice[] lineiteminvoice = Response.ShoppingCart.LineItemInvoiceList;
                        foreach (OrderLineItemInvoice invoice in lineiteminvoice)
                        {
                            if (invoice.LineItemType == LineItemTypeCode.Shipping)
                            {
                                shippingcost = shippingcost + invoice.UnitPrice;
                            }


                        }
                        #endregion
                        #region Items

                        List<Items> listitems = new List<Items>();
                        int I = 0;
                        foreach (OrderLineItemItemResponse item in Response.ShoppingCart.LineItemSKUList)
                        {
                            // shippingcost = shippingcost + Convert.ToDecimal(item.ShippingCost);
                            Items Item = new Items();
                            if (I == 0)
                            {
                                Carrier = Response.ShippingInfo.ShipmentList[0].ShippingCarrier;
                                Class = Response.ShippingInfo.ShipmentList[0].ShippingClass;

                                if (Carrier == "Amazon Merchants@" && Class == "Expedited")
                                {
                                    DeliveryMethod = "DP19";
                                }
                                else if (Carrier == "eBay" && Class == "Expedited")
                                    DeliveryMethod = "EURO";
                                else if (Carrier == "Amazon Merchants@" && Class == "Standard")
                                    DeliveryMethod = "EURO";

                                else if (Carrier + " " + Class == "Royal Mail 1st Class Standard (1-2 working days)")
                                {
                                    DeliveryMethod = "STD";
                                }
                                else if (Carrier + " " + Class == "Royal Mail Special Delivery (TM) Next Day (1 working day)")
                                {
                                    DeliveryMethod = "NEXT";
                                }
                                else if (Carrier + " " + Class == "Royal Mail 2nd Class Standard (3-5 working days)")
                                {
                                    DeliveryMethod = "2ND";
                                }
                                else if (Carrier + " " + Class == "Saturday Guaranteed")
                                {
                                    DeliveryMethod = "SAT";
                                }
                                else if (Carrier + " " + Class == "Royal Mail Airsure")
                                {
                                    if (EUCountries.Contains(shippcountries))
                                    {

                                        DeliveryMethod = "EURO";
                                    }
                                    else
                                    {
                                        DeliveryMethod = "RW";
                                    }
                                }
                                else if (Carrier + " " + Class == "Rest Of World Standard")
                                {
                                    DeliveryMethod = "RW";
                                }
                                else if (Carrier + " " + Class == "Royal Mail International Signed-for")
                                {
                                    if (EUCountries.Contains(shippcountries))
                                    {
                                        DeliveryMethod = "RM3";
                                    }
                                    else
                                    {
                                        DeliveryMethod = "RW";
                                    }
                                }
                                else if (Carrier + " " + Class == "Standard Letters 1st & 2nd")
                                {

                                    if (EUCountries.Contains(shippcountries))
                                    {
                                        DeliveryMethod = "RM4";
                                    }
                                    else
                                    {
                                        DeliveryMethod = "RW";
                                    }
                                }
                                else if (Carrier + " " + Class == "UPS Express Saver")
                                {
                                    DeliveryMethod = "UPS";
                                }
                                else if (Carrier + " " + Class == "Standard International Shipping 4-10 business days")
                                {
                                    DeliveryMethod = "EURO";
                                }
                                else
                                {
                                    DeliveryMethod = "STD";
                                }


                                /**********************FOR SOURCE CODE******************/
                                token = item.ItemSaleSource;
                                if (token.ToString().ToLower().Contains("ebay"))
                                {
                                    paymentmethod = "Ebay" + paymentmethod;
                                    OrderHeader.Source = "CA-Ebay";
                                }
                                else if (token.ToString().ToLower().Contains("amazon"))
                                {
                                    paymentmethod = "Amazon";
                                    OrderHeader.Source = "CA-Amazon";
                                }
                                else if (token.ToString().ToLower().Contains("direct"))
                                {
                                    paymentmethod = "EbayPaypal";
                                    OrderHeader.Source = "CA-Ebay";
                                }
                                else if (paymentmethod.ToString().ToLower().Contains("cdiscount"))
                                {
                                    paymentmethod = "cdiscount";
                                    OrderHeader.Source = "CA-Cdiscount";
                                    DeliveryMethod = "EURO";
                                }
                                else
                                {
                                    paymentmethod = "Amazon";
                                    OrderHeader.Source = "CA-Amazon";
                                }
                            }
                            I = I + 1;
                            Item.Deliverymethod = DeliveryMethod;
                            /*************************end************************************/

                            /******************discount***********************/

                            OrderLineItemItemPromo[] lineitempromo = item.ItemPromoList;
                            if (lineitempromo != null)
                            {
                                foreach (OrderLineItemItemPromo promo in lineitempromo)
                                {
                                    discount = discount + promo.UnitPrice;
                                    shippingcost = shippingcost + promo.ShippingPrice;
                                }
                                discount = -discount;
                            }
                            /************************end*********************/
                            Item.Discount = discount;
                            Item.Discountreason = "";
                            Item.LineTotal = item.UnitPrice * item.Quantity - discount;
                            Item.Persname = "";// Response.ShippingInfo.FirstName + ' ' + Response.ShippingInfo.LastName;
                            Item.Persnumber = ""; //Response.ShippingInfo.PhoneNumberDay;
                            Item.Personalised = false;
                            Item.Perspatch = false;
                            Item.Perssquad = false;
                            Item.Persvalue = "";
                            Item.Quantity = item.Quantity;
                            Item.Sell = item.UnitPrice;//;
                            Item.Stylecode = "";
                            Item.Vatcode = "T1";
                            Item.Vatrate = 20;
                            Item.Stockcode = item.SKU;
                            listitems.Add(Item);
                            //I = I + 1;
                            //"4433";                         
                        }

                        OrderHeader.Paymentmethod = paymentmethod;
                        OrderHeader.Postagevalue = shippingcost;
                        order.Customerdetails = customerDetails;
                        orderdetail.list_items = listitems;
                        orderdetail.Orderdelivery = OrderDelivery;
                        orderdetail.Orderheader = OrderHeader;
                        order.Orderdetails = orderdetail;

                        #endregion Items

                        #region GetReqLstXmlDoc

                        XmlSerializer xmlSerializer = new XmlSerializer(order.GetType());
                        try
                        {
                            using (MemoryStream xmlStream = new MemoryStream())
                            {
                                xmlSerializer.Serialize(xmlStream, order);
                                xmlStream.Position = 0;
                                Requestxml.Load(xmlStream);
                                xml = Requestxml.InnerXml.Replace("<list_items>", "").Replace("</list_items>", "").Replace("<listitem>", "<Items>").Replace("</listitem>", "</Items>");

                                LstXmlDoc.Add(Response.ClientOrderIdentifier, xml);
                            }
                        }
                        catch
                        {
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return LstXmlDoc;
        }

        private string GetCountry(string countryCode)
        {
            try
            {
                /*********************linq query to get country name *********************************/
                string Country = (from Codes in DsCountry.Tables[0].AsEnumerable()
                                  where Codes.Field<string>("Code") == countryCode
                                  select Codes.Field<string>("Country")).First();
                return Country.Replace("&", "&amp;");
                /*********************************end************************************************/
            }
            catch
            {
                return countryCode;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
            Application.ExitThread();
            Application.Exit();
        }

        private void GetWebRequest(Dictionary<string, string> XmlDoc)
        {
            Dictionary<string, HttpWebRequest> RequestToSend = new Dictionary<string, HttpWebRequest>();
            foreach (var request in XmlDoc)
            {
                try
                {
                    //HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://justkeep.retailsportssystems.com/ws06.php");
                    // HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.just-keepers.com/ws06-test.php");
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.just-keepers.com/ws06.php");
                    string RequestXml = request.Value;
                    req.Headers.Add("SOAPAction", "\"http://tempuri.org/Register\"");
                    req.ContentType = "text/xml; charset=\"utf-8\"";
                    req.Accept = "text/xml";
                    req.Method = "POST";
                    using (Stream stm = req.GetRequestStream())
                    {
                        using (StreamWriter stmw = new StreamWriter(stm))
                        {
                            stmw.Write(RequestXml);
                        }
                    }
                    RequestToSend.Add(request.Key, req);

                    /**********************functio to place order on system********************/

                    GetResponse(RequestToSend);

                    /*******************end******************************/

                    RequestToSend.Clear();
                }
                catch
                {
                }
            }

        }

        private void GetResponse(Dictionary<string, HttpWebRequest> XmlWebRequest)
        {
            DataTable dtResponse = new DataTable();
            try
            {
                string JustKeepOrderNum = "";

                foreach (var Xml_WebRequest in XmlWebRequest)
                {
                    /**********************code to place order on system********************/

                    HttpWebRequest Req = Xml_WebRequest.Value;
                    WebResponse response1 = Req.GetResponse();
                    XmlDocument Doc = new XmlDocument();
                    using (Stream responseStream = response1.GetResponseStream())
                    {
                        Doc.Load(responseStream);
                        ChannelResponse = Doc.InnerXml;
                        XmlNodeList justkeeperordernumber = Doc.GetElementsByTagName("OurOrderNo"); //
                        JustKeepOrderNum = justkeeperordernumber[0].InnerText;

                        if (JustKeepOrderNum != "")
                        {
                            JustKeeperorderNumberToInsert = Convert.ToInt32(JustKeepOrderNum);

                            /********************GetRequestXml************************/
                            Request = LstXmlDoc[Xml_WebRequest.Key.ToString()];

                            /********************code to insert data into database******************/

                            /*********************code to clientorderidentifier into list***********************/

                            channelordernumber.Add(Xml_WebRequest.Key);

                            /*********************************end**********************************************/

                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = con;
                            if (con.State == ConnectionState.Closed)
                            {
                                con.Open();
                            }
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "SP_JKInsertdata";
                            //cmd.CommandText = "insert into Orders(ChannelOrderNumber,JustKeeperOrderNumber,Request,Response) values" +
                            //    "(" + Xml_WebRequest.Key + "," + JustKeeperorderNumberToInsert + ",'" + Request + "','" + ChannelResponse + "')";
                            cmd.Parameters.AddWithValue("@ChannelOrderNumber", Xml_WebRequest.Key);
                            cmd.Parameters.AddWithValue("@JustKeeperOrderNumber", JustKeeperorderNumberToInsert);
                            cmd.Parameters.AddWithValue("@Request", Request);
                            cmd.Parameters.AddWithValue("@Response", ChannelResponse);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            con.Close();




                            /****************************end*****************************/



                        }
                        else
                        {
                            Operations.SendMail("nephrodoc15@gmail.com", "justkeeper order integration (SPAIN) application in module 1.1.1.1", ChannelResponse);
                        }
                        /*******************************end***********************************/
                    }
                }
            }
            catch (Exception ex)
            {
                Operations.SendMail("nephrodoc15@gmail.com", "justkeeper order integration (SPAIN) application in module 1.1.1.1", ex.Message);
            }

        }
    }
}
