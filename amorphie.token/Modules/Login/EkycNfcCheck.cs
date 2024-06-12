﻿using System.Dynamic;
using amorphie.token.core;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token;

public static class EkycNfcCheck
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> Check(
      [FromBody] dynamic body,
      [FromServices] IEkycService ekycService
  )
    {

        var transitionName = body.GetProperty("LastTransition").ToString();
        // var transactionId = body.GetProperty("InstanceId").ToString();
        var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

        dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());
        dynamic targetObject = new System.Dynamic.ExpandoObject();
        targetObject.Data = dataChanged;

        var isSkip = dataChanged.entityData.IsSkip;
        dynamic variables = new Dictionary<string, dynamic>();

        variables.Add("IsSkip", isSkip);
        bool nfcStatus = false;
        if (!isSkip)
        {
            var nfcIsSuccess = dataChanged.entityData.IsSuccess;

            dataChanged.additionalData = new ExpandoObject();
            var callType = body.GetProperty("CallType").ToString();
            var instance = body.GetProperty("Instance").ToString();
            // var name = body.GetProperty("Name").ToString();
            // var surname = body.GetProperty("Surname").ToString();
            dataChanged.additionalData.isEkyc = true;// gitmek istediği data 
            dataChanged.additionalData.callType = callType;
            var ApplicantFullName = body.GetProperty("ApplicantFullName").ToString();
            dataChanged.additionalData.applicantFullName = ApplicantFullName;
            // dataChanged.additionalData.customerName = name; // bu kısımları doldur.
            // dataChanged.additionalData.customerSurname = surname;
            dataChanged.additionalData.instanceId = instance;


            bool identityNoCompatible = false;



            var sessionId = body.GetProperty("SessionId").ToString();
            if (nfcIsSuccess && !String.IsNullOrEmpty(sessionId))
            {

                var session = await ekycService.GetSessionInfoAsync(Guid.Parse(sessionId));
                if (session is not null && session.Data is not null && session.Data?.IDChip is not null)
                {
                    var identityNo = body.GetProperty("UserName").ToString();

                    identityNoCompatible = session.Data.IDChip.IdentityNo == identityNo;
                    if (identityNoCompatible && session.Data.IDChip.IsValid is true)
                    {
                        dataChanged.additionalData.isEkyc = true;// gitmek istediği data 
                        dataChanged.additionalData.NfcReadSuccess = true;
                        nfcStatus = true;
                    }

                }

            }

            var nfcCurrentFailedCount = Convert.ToInt32(body.GetProperty("CurrentNfcFailedCount").ToString());


            if (!nfcStatus)
            {

                //Max-Min try count 
                if (nfcCurrentFailedCount >= EkycConstants.NfcFailedTryCount)
                {
                    dataChanged.additionalData.pages = new List<EkycPageModel>
                {
                    EkycAdditionalDataContstants.StandartItem,
                    EkycAdditionalDataContstants.NfcFailedBiggerThanMinForRetry
                };

                }
                if (nfcCurrentFailedCount >= EkycConstants.NfcFailedMaxTryCount)
                {
                    //Min try additional data
                    dataChanged.additionalData.pages = new List<EkycPageModel>
                {
                    EkycAdditionalDataContstants.StandartItem,
                    EkycAdditionalDataContstants.NfcFailedMinForRetry
                };
                }



                variables.Add("FailedStepName", "nfc");
                nfcCurrentFailedCount++;
            }

            if (nfcIsSuccess && nfcStatus)
            {
                dataChanged.additionalData.pages = new List<EkycPageModel>{
                EkycAdditionalDataContstants.StandartItem
            };
            }
            dataChanged.additionalData.exitTransition = "amorphie-ekyc-exit";



            variables.Add("Init", true);

            variables.Add("CurrentNfcFailedCount", nfcCurrentFailedCount);
        }

        variables.Add("NfcStatus", nfcStatus);
        // variables.Add("NfcStatus", true);


        targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
        targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
        variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);

        return Results.Ok(variables);

        // return Task.FromResult(Results.Ok("data"));
    }
}
