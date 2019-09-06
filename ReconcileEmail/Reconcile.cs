
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using System.Security.Cryptography;
using Microsoft.Xrm.Sdk.Query;

namespace NRCAN
{
    public class ReconcileCase : CodeActivity
    {

        [RequiredArgument]
        [Input("Description")]
        [Default("CASE-")]

        public InArgument<string> SearchField { get; set; }

        
        protected override void Execute(CodeActivityContext executionContext)
        {

            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            // Obtain the execution context from the service provider.
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            // Use the context service to create an instance of IOrganizationService.  
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

            
            Guid recordId = context.PrimaryEntityId;
            Entity currentRecord = service.Retrieve("email", recordId, new ColumnSet(true));
            // Create a query expression getting all the case numbers into an array
            QueryExpression qe = new QueryExpression();
            qe.EntityName = "incident";
            qe.ColumnSet = new ColumnSet(true);
     

            List<Guid> CaseNumbers = new List<Guid>();

            EntityCollection cases = service.RetrieveMultiple(qe);


            
            foreach (Entity c in cases.Entities)
            {
                string num = c.GetAttributeValue<string>("ticketnumber");
                // Ensure that search string isn't null
                if (!String.IsNullOrEmpty(num))
                {
                    if (this.SearchField.Get<string>(executionContext).Contains(num))
                    {
                        //we found a match so set regarding to case.

                        // if more than one case number found in the string, the loop will return.
                        //if (CaseNumbers.Count() > 2)
                        //{
                        //    return;

                        //} else
                        //{
                            CaseNumbers.Add(c.Id);
                        //}
                        
                    }
                }

            }

            //if (CaseNumbers.Count() == 1)
            //{
            //one
                currentRecord["regardingobjectid"] = new EntityReference("incident", CaseNumbers.FirstOrDefault());
                service.Update(currentRecord);
            //}



        }





    }



}
