/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Linq;
using System.Windows.Input;
using ESRI.ArcLogistics;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Commands;
using QBFC8Lib;

namespace QuickBooksPlugIns
{
    [CommandPlugIn(new string[1] { "ScheduleTaskWidgetCommands" })]
    public class ImportOrdersFromInvoicesCmd : ESRI.ArcLogistics.App.Commands.ICommand
    {
        App m_application = null;

        #region ICommand Members

        public void Execute(params object[] args)
        {
            m_application.Messenger.AddInfo("Executing import from QuickBooks.");

            int countOrdersAdded = 0;
            QBSessionManager session = null;
            try
            {
                // Start session with QuickBooks
                session = new QBSessionManager();
                
                session.OpenConnection("123", "ArcLogistics PlugIn: QuickBooksPlugIns.ImportOrdersFromInvoicesCmd");
                session.BeginSession("", ENOpenMode.omDontCare);

                IInvoiceRetList invoiceList = QueryInvoices(session);
                int invoiceCount = invoiceList.Count;

                for (int i = 0; i < invoiceCount; i++)
                {
                    IInvoiceRet invoiceRet = invoiceList.GetAt(i);

                    ESRI.ArcLogistics.DomainObjects.Order newOrder = MakeOrderFromInvoice(invoiceRet, session);

                    m_application.Project.Orders.Add(newOrder);

                    countOrdersAdded++;
                }
            }

            catch (Exception e)
            {
                m_application.Messenger.AddError("Error executing QuickBooksPlugIns.ImportOrdersFromInvoicesCmd: " + e.Message);
            }

            finally
            {
                // Close connection because we don't need it anymore
                if (session != null)
                {
                    session.EndSession();
                    session.CloseConnection();
                }

                m_application.Project.Save();

                m_application.Messenger.AddInfo(countOrdersAdded.ToString() + " orders added.");
            }
        }

        private ESRI.ArcLogistics.DomainObjects.Order MakeOrderFromInvoice(IInvoiceRet invoiceRet, QBSessionManager session)
        {
            ESRI.ArcLogistics.DomainObjects.Order resultOrder = null;

            ICustomerRet customerRet = QueryCustomer(session, invoiceRet.CustomerRef.FullName.GetValue());

            CapacitiesInfo capInfo = m_application.Project.CapacitiesInfo;
            OrderCustomPropertiesInfo propInfo = m_application.Project.OrderCustomPropertiesInfo;

            resultOrder = new ESRI.ArcLogistics.DomainObjects.Order(capInfo, propInfo);

            resultOrder.PlannedDate = m_application.CurrentDate;
            if (customerRet.ParentRef != null)
                resultOrder.Name = customerRet.ParentRef.FullName.GetValue();
            else
                resultOrder.Name = customerRet.FullName.GetValue();

            IAddress useAddress = null;
            if (customerRet.ShipAddress != null)
                useAddress = customerRet.ShipAddress;
            else if (customerRet.BillAddress != null)
                useAddress = customerRet.BillAddress;
            else
                m_application.Messenger.AddWarning("No address for: " + resultOrder.Name);

            if (useAddress != null)
            {
                if (useAddress.Addr2 != null)
                    resultOrder.Address.AddressLine = useAddress.Addr2.GetValue();
                else
                    resultOrder.Address.AddressLine = useAddress.Addr1.GetValue();

                resultOrder.Address.Locality3 = useAddress.City.GetValue();
                resultOrder.Address.StateProvince = useAddress.State.GetValue();
                resultOrder.Address.PostalCode1 = useAddress.PostalCode.GetValue();

                AddressCandidate candidate = m_application.Geocoder.Geocode(resultOrder.Address);

                resultOrder.GeoLocation = candidate.GeoLocation;
            }

            // Look in the order custom properties for matching invoice detail items (by item description).
            // Look in the order capacities for matching item type custom fields.

            OrderCustomPropertiesInfo orderPropertiesInfo = resultOrder.CustomPropertiesInfo;
            OrderCustomProperties orderProperties = resultOrder.CustomProperties;

            CapacitiesInfo orderCapacitiesInfo = resultOrder.CapacitiesInfo;
            Capacities orderCapacities = resultOrder.Capacities;

            // Retrieve invoice line list
            // Each line can be either InvoiceLineRet OR InvoiceLineGroupRet

            IORInvoiceLineRetList orInvoiceLineRetList = invoiceRet.ORInvoiceLineRetList;
            if (orInvoiceLineRetList != null && (orderProperties.Count > 0 || orderCapacities.Count > 0))
            {
                int lineCount = orInvoiceLineRetList.Count;
                for (int i = 0; i < lineCount; i++)
                {
                    IORInvoiceLineRet orInvoiceLineRet = orInvoiceLineRetList.GetAt(i);

                    // Check what to retrieve from the orInvoiceLineRet object
                    // based on the "ortype" property.  Skip summary lines.

                    if (orInvoiceLineRet.ortype != ENORInvoiceLineRet.orilrInvoiceLineRet)
                        continue;

                    if (orInvoiceLineRet.InvoiceLineRet.ItemRef.FullName != null)
                    {
                        string itemName = orInvoiceLineRet.InvoiceLineRet.ItemRef.FullName.GetValue();
                        double itemQuantity = 0;
                        if (orInvoiceLineRet.InvoiceLineRet.ItemRef != null)
                            itemQuantity = System.Convert.ToDouble(orInvoiceLineRet.InvoiceLineRet.Quantity.GetValue());

                        // look for matching custom order property

                        OrderCustomProperty orderPropertyInfoItem = null;
                        for (int j = 0; j < orderPropertiesInfo.Count; j++)
                        {
                            orderPropertyInfoItem = orderPropertiesInfo.ElementAt(j) as OrderCustomProperty;
                            if (orderPropertyInfoItem.Name == itemName)
                            {
                                if (orderPropertyInfoItem.Type == OrderCustomPropertyType.Numeric)
                                    orderProperties[j] = itemQuantity;
                                else
                                    orderProperties[j] = itemQuantity.ToString();

                                break;
                            }
                        }

                        // look for matching capacity

                        // need to lookup item record so we get the extra field(s)
                        // TODO: It might be a good idea to cache these locally to avoid
                        // excess QB queries.

                        IORItemRet orItemRet = QueryItem(session, itemName);
                        IDataExtRetList custItemFieldsRetList = null;

                        switch (orItemRet.ortype)
                        {
                            case ENORItemRet.orirItemServiceRet:
                                {
                                    // orir prefix comes from OR + Item + Ret
                                    IItemServiceRet ItemServiceRet = orItemRet.ItemServiceRet;
                                    custItemFieldsRetList = ItemServiceRet.DataExtRetList;
                                }
                                break;
                            case ENORItemRet.orirItemInventoryRet:
                                {
                                    IItemInventoryRet ItemInventoryRet = orItemRet.ItemInventoryRet;
                                    custItemFieldsRetList = ItemInventoryRet.DataExtRetList;
                                }
                                break;
                            case ENORItemRet.orirItemNonInventoryRet:
                                {
                                    IItemNonInventoryRet ItemNonInventoryRet = orItemRet.ItemNonInventoryRet;
                                    custItemFieldsRetList = ItemNonInventoryRet.DataExtRetList;
                                }
                                break;
                        }

                        int custItemFieldCount = 0;
                        if (custItemFieldsRetList != null)
                            custItemFieldCount = custItemFieldsRetList.Count;

                        for (int j = 0; j < custItemFieldCount; j++)
                        {
                            IDataExtRet custItemField = custItemFieldsRetList.GetAt(j);
                            string custItemFieldName = custItemField.DataExtName.GetValue();

                            CapacityInfo orderCapacityInfoItem = null;
                            for (int k = 0; k < orderCapacitiesInfo.Count; k++)
                            {
                                orderCapacityInfoItem = orderCapacitiesInfo.ElementAt(k);
                                if (orderCapacityInfoItem.Name == custItemFieldName)
                                {
                                    orderCapacities[k] += System.Convert.ToDouble(custItemField.DataExtValue.GetValue()) * itemQuantity;

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            resultOrder.CustomProperties = orderProperties;
            resultOrder.Capacities = orderCapacities;

            return resultOrder;
        }

        public void Initialize(ESRI.ArcLogistics.App.App app)
        {
            m_application = app;
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public System.Windows.Input.KeyGesture KeyGesture
        {
            get { return null; }
        }

        public string Name
        {
            get { return "QuickBooksPlugIns.ImportOrdersFromInvoicesCmd"; }
        }

        public string Title
        {
            get { return "Import from QuickBooks"; }
        }

        public string TooltipText
        {
            get { return "Import invoices for the selected date from QuickBooks"; }
        }

        #endregion

        ~ImportOrdersFromInvoicesCmd()
        {
            m_application = null;
        }

        #region Utility Members

        private IInvoiceRetList QueryInvoices(QBSessionManager session)
        {
            // Create Message Set request

            IMsgSetRequest requestMsgSet = getLatestMsgSetRequest(session);
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;

            // Create the query object needed to perform InvoiceQueryRq

            IInvoiceQuery pInvQuery = requestMsgSet.AppendInvoiceQueryRq();
            IInvoiceFilter pInvFilter = pInvQuery.ORInvoiceQuery.InvoiceFilter;

            // set the date range to the current schedule date selected in ArcLogistics

            pInvFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(m_application.CurrentDate);
            pInvFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(m_application.CurrentDate);

            pInvQuery.IncludeLineItems.SetValue(true);

            // Do the request
            IMsgSetResponse responseMsgSet = session.DoRequests(requestMsgSet);

            // Uncomment the following to see the request and response XML for debugging
            //string rq = requestMsgSet.ToXMLString();
            //string rs = responseMsgSet.ToXMLString();

            //m_application.Messenger.AddInfo("Resquest: " + rq);
            //m_application.Messenger.AddInfo("Response: " + rs);

            // Interpret the response

            IResponseList rsList = responseMsgSet.ResponseList;

            //  Retrieve the one response corresponding to our single request

            IResponse response = rsList.GetAt(0);

            if (response.StatusCode != 0)
            {
                string msg = "";
                if (response.StatusCode == 1)  //No record found
                    msg = "No invoices found for " + m_application.CurrentDate.ToShortDateString();
                else
                    msg = "Error getting invoices.  Status: " + response.StatusCode.ToString() + ", Message: " + response.StatusMessage;

                throw new Exception(msg);
            }

            return response.Detail as IInvoiceRetList;
        }

        private ICustomerRet QueryCustomer(QBSessionManager session, string customerFullName)
        {
            // query for the customer information

            IMsgSetRequest requestMsgSet = getLatestMsgSetRequest(session);
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;

            ICustomerQuery pCustomerQuery = requestMsgSet.AppendCustomerQueryRq();
            ICustomerListFilter pCustomerFilter = pCustomerQuery.ORCustomerListQuery.CustomerListFilter;
            pCustomerFilter.ORNameFilter.NameFilter.Name.SetValue(customerFullName);
            pCustomerFilter.ORNameFilter.NameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcContains);

            IMsgSetResponse responseMsgSet = session.DoRequests(requestMsgSet);

            // Uncomment the following to see the request and response XML for debugging
            //string rq = requestMsgSet.ToXMLString();
            //string rs = responseMsgSet.ToXMLString();

            //m_application.Messenger.AddInfo("Customer Resquest: " + rq);
            //m_application.Messenger.AddInfo("Customer Response: " + rs);

            // Interpret the response

            IResponseList rsList = responseMsgSet.ResponseList;

            //  Retrieve the one response corresponding to our single request

            IResponse response = rsList.GetAt(0);

            if (response.StatusCode != 0)
            {
                string msg = "";
                if (response.StatusCode == 1)  //No record found
                    msg = "Customer not found: " + customerFullName;
                else
                    msg = "Error getting customer.  Status: " + response.StatusCode.ToString() + ", Message: " + response.StatusMessage;

                throw new Exception(msg);
            }

            // We have one or more customers (expect one)

            ICustomerRetList customerList = response.Detail as ICustomerRetList;
            int customerCount = customerList.Count;

            if (customerCount > 1)
                m_application.Messenger.AddWarning("Multiple customers found: " + customerFullName);

            return customerList.GetAt(0);
        }

        private IORItemRet QueryItem(QBSessionManager session, string itemFullName)
        {
            // query for the customer information

            IMsgSetRequest requestMsgSet = getLatestMsgSetRequest(session);
            requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;

            IItemQuery pItemQuery = requestMsgSet.AppendItemQueryRq();
            pItemQuery.ORListQuery.ListFilter.ORNameFilter.NameFilter.Name.SetValue(itemFullName);
            pItemQuery.ORListQuery.ListFilter.ORNameFilter.NameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcContains);

            pItemQuery.OwnerIDList.Add("0");

            IMsgSetResponse responseMsgSet = session.DoRequests(requestMsgSet);

            // Uncomment the following to see the request and response XML for debugging
            //string rq = requestMsgSet.ToXMLString();
            //string rs = responseMsgSet.ToXMLString();

            //m_application.Messenger.AddInfo("Item Resquest: " + rq);
            //m_application.Messenger.AddInfo("Item Response: " + rs);

            // Interpret the response

            IResponseList rsList = responseMsgSet.ResponseList;

            //  Retrieve the one response corresponding to our single request

            IResponse response = rsList.GetAt(0);

            if (response.StatusCode != 0)
            {
                string msg = "";
                if (response.StatusCode == 1)  //No record found
                    msg = "Item not found: " + itemFullName;
                else
                    msg = "Error getting item.  Status: " + response.StatusCode.ToString() + ", Message: " + response.StatusMessage;

                throw new Exception(msg);
            }

            // We have one or more customers (expect one)

            IORItemRetList orItemRetList = response.Detail as IORItemRetList;

            int itemCount = orItemRetList.Count;

            if (itemCount > 1)
                m_application.Messenger.AddWarning("Multiple items found: " + itemFullName);

            return orItemRetList.GetAt(0);
        }


        #endregion Utility Members


        #region QuickBooks SDK Utility Members

        // This code was copied from the QuickBooks SDK

        // IY: CODE FOR HANDLING DIFFERENT VERSIONS
        private double QBFCLatestVersion(QBSessionManager SessionManager)
        {
            // IY: Use oldest version to ensure that we work with any QuickBooks (US)
            IMsgSetRequest msgset = SessionManager.CreateMsgSetRequest("US", 1, 0);
            msgset.AppendHostQueryRq();
            // MessageBox.Show(msgset.ToXMLString());

            // IY: Use SessionManager object to open a connection and begin a session 
            // with QuickBooks. At this time, you should add interop.QBFCxLib into 
            // your Project References
            //SessionManager.OpenConnection("", "IDN InvoiceAdd C# sample");
            //SessionManager.BeginSession("", ENOpenMode.omDontCare);

            IMsgSetResponse QueryResponse = SessionManager.DoRequests(msgset);
            // IY: The response list contains only one response,
            // which corresponds to our single HostQuery request
            IResponse response = QueryResponse.ResponseList.GetAt(0);
            // IY: Please refer to QBFC Developers Guide/pg for details on why 
            // "as" clause was used to link this derrived class to its base class
            IHostRet HostResponse = response.Detail as IHostRet;
            IBSTRList supportedVersions = HostResponse.SupportedQBXMLVersionList as IBSTRList;

            int i;
            double vers;
            double LastVers = 0;
            string svers = null;

            for (i = 0; i <= supportedVersions.Count - 1; i++)
            {
                svers = supportedVersions.GetAt(i);
                vers = Convert.ToDouble(svers);
                if (vers > LastVers)
                {
                    LastVers = vers;
                    //svers = supportedVersions.GetAt(i);
                }
            }

            // IY: Close the session and connection with QuickBooks
            //SessionManager.EndSession();
            //SessionManager.CloseConnection();
            return LastVers;
        }


        public IMsgSetRequest getLatestMsgSetRequest(QBSessionManager sessionManager)
        {
            // IY: Find and adapt to supported version of QuickBooks
            double supportedVersion = QBFCLatestVersion(sessionManager);
            // MessageBox.Show("supportedVersion = " + supportedVersion.ToString());

            short qbXMLMajorVer = 0;
            short qbXMLMinorVer = 0;
            if (supportedVersion >= 6.0)
            {
                qbXMLMajorVer = 6;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 5.0)
            {
                qbXMLMajorVer = 5;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 4.0)
            {
                qbXMLMajorVer = 4;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 3.0)
            {
                qbXMLMajorVer = 3;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 2.0)
            {
                qbXMLMajorVer = 2;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 1.1)
            {
                qbXMLMajorVer = 1;
                qbXMLMinorVer = 1;
            }
            else
            {
                qbXMLMajorVer = 1;
                qbXMLMinorVer = 0;
                // MessageBox.Show("It seems that you are running QuickBooks 2002 Release 1. We strongly recommend that you use QuickBooks' online update feature to obtain the latest fixes and enhancements");
            }
            // MessageBox.Show("qbXMLMajorVer = " + qbXMLMajorVer);
            // MessageBox.Show("qbXMLMinorVer = " + qbXMLMinorVer);

            // IY: Create the message set request object
            IMsgSetRequest requestMsgSet = sessionManager.CreateMsgSetRequest("US", qbXMLMajorVer, qbXMLMinorVer);
            return requestMsgSet;
        }

        #endregion

    }
}
