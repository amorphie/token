using System.Dynamic;
using amorphie.token.core;
using amorphie.token.data;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token;

public static class EkcyFaceCheck
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> Check(
         [FromBody] dynamic body,
         [FromServices] IEkycService ekycService
     )
    {

        var transitionName = body.GetProperty("LastTransition").ToString();
        int faceFailedTryCount = EkycConstants.FaceFailedTryCount;
        Int32.TryParse(body.GetProperty("FaceFailedTryCount")?.ToString(), out faceFailedTryCount);

        // var transactionId = body.GetProperty("InstanceId").ToString();
        var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

        dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());
        dynamic targetObject = new System.Dynamic.ExpandoObject();
        targetObject.Data = dataChanged;

        var faceIsSuccess = dataChanged.entityData.IsSuccess;
        dynamic variables = new Dictionary<string, dynamic>();
        dataChanged.additionalData = new ExpandoObject();
        var callType = body.GetProperty("CallType").ToString();
        var instance = body.GetProperty("Instance").ToString();
        dataChanged.additionalData.isEkyc = true;// gitmek istediği data 
        dataChanged.additionalData.callType = callType;
        var ApplicantFullName = body.GetProperty("ApplicantFullName").ToString();
        dataChanged.additionalData.applicantFullName = ApplicantFullName;

        dataChanged.additionalData.instanceId = instance;


        var isSkip = dataChanged.entityData.IsSkip;
        variables.Add("IsSkip", isSkip);
        bool faceStatus = false;
        if (!isSkip)
        {
            var sessionId = body.GetProperty("SessionId").ToString();
            if (faceIsSuccess && !String.IsNullOrEmpty(sessionId))
            {

                var session = await ekycService.GetSessionInfoAsync(Guid.Parse(sessionId));
                if (session.Data is not null && session.Data.Face is not null && session.Data.Face.ValidityLevel > 0)
                {
                    dataChanged.additionalData.isEkyc = true;// gitmek istediği data 
                    dataChanged.additionalData.FaceReadSuccess = true;
                    faceStatus = true;
                }

            }

            var faceCurrentFailedCount = Convert.ToInt32(body.GetProperty("CurrentFaceFailedCount").ToString());
            if (!faceStatus)
            {

                faceCurrentFailedCount++;
                //Max-Min try count 
                if (faceCurrentFailedCount < faceFailedTryCount)

                {
                    dataChanged.additionalData.pages = new List<EkycPageModel>
                    {
                        EkycAdditionalDataContstants.StandartItem,
                        EkycAdditionalDataContstants.FaceFailedMinForRetry
                    };
 
                }

                if (faceCurrentFailedCount >= faceFailedTryCount)

                {
                    //Min try additional data
                    dataChanged.additionalData.pages = new List<EkycPageModel>
                    {
                        EkycAdditionalDataContstants.StandartItem,
                        EkycAdditionalDataContstants.FaceFailedBiggerThanMinForRetry
                    };
                }


                faceCurrentFailedCount++;

            }

            if (faceStatus && faceIsSuccess)
            {
                dataChanged.additionalData.pages = new List<EkycPageModel>
                {
                    EkycAdditionalDataContstants.StandartItem,
                    EkycAdditionalDataContstants.FaceSuccessConfirm
                };
            }



            variables.Add("Init", true);

            // variables.Add("IsSelfServiceAvaliable", true); // bu client dan alınacak sanırım
            variables.Add("CurrentFaceFailedCount", faceCurrentFailedCount);
        }
        else
        {
            dataChanged.additionalData.pages = new List<EkycPageModel>
                {
                    EkycAdditionalDataContstants.StandartItem,
                    EkycAdditionalDataContstants.SkipFaceForVideoCall
                };
        }
        dataChanged.additionalData.exitTransition = "amorphie-ekyc-exit";
        variables.Add("FaceReadStatus", faceStatus);
        // variables.Add("FaceReadStatus", true);

        targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
        targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
        variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);

        return Results.Ok(variables);

    }
}
