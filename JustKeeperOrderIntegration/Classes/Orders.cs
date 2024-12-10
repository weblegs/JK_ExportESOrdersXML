using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JustKeeperOrderIntegration.Classes
{  
    public  class Orders
    {                
        public Customerdetails Customerdetails { get; set; }
        public Orderdetails Orderdetails { get; set; }
    }
    public class Customerdetails
    {
        public String AccountNo{get;set; }
        public String Password { get; set; }
        public String SystemID { get; set; }

    }
    public class Orderdetails
    {
        //List<Items> listitems = new List<Items>();
        public Orderheader Orderheader { get; set; }
        public Orderdelivery Orderdelivery { get; set; }
        [System.Xml.Serialization.XmlArrayItem(ElementName="listitem")]
        public List<Items> list_items{get;set; }
    }
    public class Orderheader
    {
        public String RSSCustnumber { get; set; }
        public Int32  Ordernumber { get; set; }
        public String   Orderdate { get; set; }
        public decimal  Totalvalue { get; set; }
        public decimal  Postagevalue { get; set; }
        public String Source { get; set; }
        public String Paymentmethod { get; set; }
        public String Customertitle { get; set; }
        public String Customerinitial { get; set; }
        public String Customerforename { get; set; }
        public String Customersurname { get; set; }
        public String Company { get; set; }
        public String Contact { get; set; }
        public String AddressLine1 { get; set; }
        public String AddressLine2 { get; set; }
        public String AddressLine3 { get; set; }
        public String AddressLine4 { get; set; }
        public String AddressLine5 { get; set; }
        public String Country { get; set; }
        public String Postcode { get; set; }
        public String Email { get; set; }
        public String Mobilenumber { get; set; }
        public String Homenumber { get; set; }
        public Int32  Worknumber { get; set; }
        public String DeliveryIns { get; set; }
        public int  Deliverysame { get; set; }
        public Int32 Currency { get; set; }  

    }
    public class Orderdelivery
    {
        public String Dcustomertitle { get; set; }
        public String Dcustomerinitial { get; set; }
        public String Dcustomerforename { get; set; }
        public String Dcustomersurname { get; set; }
        public String Dcompany { get; set; }
        public String Dcontact { get; set; }    
        public String DaddressLine1 { get; set; }
        public String DaddressLine2 { get; set; }
        public String DaddressLine3 { get; set; }
        public String DaddressLine4 { get; set; }
        public String DaddressLine5 { get; set; }
        public String Dcountry { get; set; }
        public String Dpostcode { get; set; }
        public String Demail { get; set; }
        public String Dmobilenumber { get; set; }
        public String Dhomenumber { get; set; }
        public Int32  Dworknumber { get; set; }
      
    }
    public class Items
    {
        public String Stylecode { get; set; }
        public String Stockcode { get; set; }
        public Int32 Quantity { get; set; }
        public Decimal Sell { get; set; }
        public String Vatcode { get; set; }
        public Decimal Vatrate { get; set; }
        public Decimal Discount { get; set; }
        public String Discountreason { get; set; }
        public Decimal LineTotal { get; set; }
        public String Deliverymethod { get; set; }
        public Boolean Personalised { get; set; }
        public String Persvalue { get; set; }
        public String Persname { get; set; }
        public String Persnumber { get; set; }
        public Boolean Perspatch { get; set; }
        public Boolean Perssquad { get; set; }

    }
}
