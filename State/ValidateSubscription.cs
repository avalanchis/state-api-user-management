using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LCU.State.API.NapkinIDE.UserManagement;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Fathym;
using LCU.Personas.Client.Enterprises;
using LCU.Personas.Client.Identity;
using LCU.StateAPI.Utilities;
using LCU.State.API.NapkinIDE.UserManagement.State;

namespace LCU.State.API.NapkinIDE.UserManagement.Management
{
    [Serializable]
    [DataContract]
    public class ValidateSubscriptionRequest
    {

        [DataMember]
        public virtual string SubscriptionID { get; set; }

    }

    public class ValidateSubscription
    {
        protected EnterpriseManagerClient engMgr;

        protected IdentityManagerClient idMgr;

        public ValidateSubscription(EnterpriseManagerClient engMgr, IdentityManagerClient idMgr)
        {
            this.engMgr = engMgr;

            this.idMgr = idMgr;
        }

        [FunctionName("ValidateSubscription")]
        public virtual async Task<Status> Run([HttpTrigger] HttpRequest req, ILogger log,
            [SignalR(HubName = UserManagementState.HUB_NAME)]IAsyncCollector<SignalRMessage> signalRMessages,
            [Blob("state-api/{headers.lcu-ent-api-key}/{headers.lcu-hub-name}/{headers.x-ms-client-principal-id}/{headers.lcu-state-key}", FileAccess.ReadWrite)] CloudBlockBlob stateBlob)
        {
            return await stateBlob.WithStateHarness<UserManagementState, ValidateSubscriptionRequest, UserManagementStateHarness>(req, signalRMessages, log,
                async (harness, reqData) =>
            {
                log.LogInformation($"Executing ValidateSubscription Action.");

                var stateDetails = StateUtils.LoadStateDetails(req);

                var status =  await harness.ValidateSubscription(engMgr, idMgr, stateDetails.EnterpriseAPIKey, stateDetails.Username, reqData.SubscriptionID);

                return status;
            });
        }
    }
}
