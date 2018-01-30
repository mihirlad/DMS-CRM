using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Configuration;
using System.ServiceModel.Description;

namespace ClassLibrary1
{
    public class OrganizationUtility
    {
        public static IOrganizationService GetCRMService()
        {
            try
            {
                OrganizationServiceProxy service = null;
                string ServerUrl = ConfigurationManager.AppSettings["CRMServerUrl"];
                string username = ConfigurationManager.AppSettings["username"];
                string password = ConfigurationManager.AppSettings["password"];
                ClientCredentials creds = new ClientCredentials();
                creds.UserName.UserName = username;
                creds.UserName.Password = password;

                service = new OrganizationServiceProxy(new Uri(ServerUrl), null, creds, null);
                service.ServiceConfiguration.CurrentServiceEndpoint.Behaviors.Add(new ProxyTypesBehavior());
                service.Timeout = new TimeSpan(0, 10, 0);

               

                return (IOrganizationService)service;
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}
