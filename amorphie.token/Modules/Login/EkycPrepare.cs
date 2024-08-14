using amorphie.token.core;
using amorphie.token.core.Models.Profile;
using amorphie.token.Services.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Dynamic;
using System.Runtime.CompilerServices;
namespace amorphie.token;

public static class EkycPrepare
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public static async Task<IResult> Prepare(
        [FromBody] dynamic body,
        [FromServices] IEkycService ekycService,
        [FromServices] IProfileService profileService,
        IConfiguration configuration

    )
    {
        var transitionName = body.GetProperty("LastTransition").ToString();
        var transactionId = body.GetProperty("InstanceId").ToString();
        var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");

        dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());


        dynamic targetObject = new System.Dynamic.ExpandoObject();
        targetObject.Data = dataChanged;
        string citizenShipNumber = dataChanged.entityData.UserName;
        var callType = dataChanged.entityData.CallType;
        string wfId = dataChanged.entityData.WfId;
        string ApplicantFullName = dataChanged.entityData.ApplicantFullName;
        string constCallType = ekycService.GetCallType(callType);
        var instance = transactionId.ToString();
        var isSelfServiceAvaible = true;
        bool hasWfId = false;
        // WFID -- or zeen
        if (constCallType == EkycCallTypeConstants.Mevduat_ON ||
         callType == EkycCallTypeConstants.Mevduat_HEPSIBURADA || 
        callType == EkycCallTypeConstants.Mevduat_BRGN)
        {
           
            if(!wfId.IsNullOrEmpty())
            {
                hasWfId = true;
            }

            instance = wfId.ToString();
            isSelfServiceAvaible = false;

        }
       

        if (!hasWfId)
        {
            // Add prefix for creating session
            instance = $"{constCallType.ToLower()}_{instance}";
            await ekycService.CreateSession(instance, citizenShipNumber, callType);
        }



        //Register the enqura 



        dynamic variables = new Dictionary<string, dynamic>();


        // Set config variables :) 
        variables.Add("Init", true);
        variables.Add("CurrentOcrFailedCount", 0);
        variables.Add("CurrentNfcFailedCount", 0);
        variables.Add("CurrentFaceFailedCount", 0);
        variables.Add("CallType", constCallType);
        // variables.Add("Name", registerResult.Name);
        // variables.Add("Surname", registerResult.Surname);
        variables.Add("ApplicantFullName", ApplicantFullName);
        variables.Add("IsSelfServiceAvaible", isSelfServiceAvaible);
        variables.Add("Instance", instance);

        // Set failed count from body




        var ocrMinCount = Convert.ToInt32(configuration["EkycOcrFailMinTryCountDefault"]);
        var nfcMinCount = Convert.ToInt32(configuration["EkycNfcFailMinTryCountDefault"]);
        var faceMinCount = Convert.ToInt32(configuration["EkycFaceFailMinTryCountDefault"]);

        var ocrMaxCount = Convert.ToInt32(configuration["EkycOcrFailMaxTryCountDefault"]);
        var nfcMaxCount = Convert.ToInt32(configuration["EkycNfcFailMaxTryCountDefault"]);
        var faceMaxCount = Convert.ToInt32(configuration["EkycFaceFailMaxTryCountDefault"]);

        if (constCallType == EkycCallTypeConstants.Mevduat_ON || 
        constCallType == EkycCallTypeConstants.Mevduat_BRGN || 
        callType == EkycCallTypeConstants.Mevduat_HEPSIBURADA)
        {
            ocrMinCount = Convert.ToInt32(configuration["EkycOcrFailMinTryCountMevduat"]);
            nfcMinCount = Convert.ToInt32(configuration["EkycNfcFailMinTryCountMevduat"]);
            faceMinCount = Convert.ToInt32(configuration["EkycFaceFailMinTryCountMevduat"]);

            ocrMaxCount = Convert.ToInt32(configuration["EkycOcrFailMaxTryCountMevduat"]);
            nfcMaxCount = Convert.ToInt32(configuration["EkycNfcFailMaxTryCountMevduat"]);
            faceMaxCount = Convert.ToInt32(configuration["EkycFaceFailMaxTryCountMevduat"]);
        }


        variables.Add("OcrFailedTryCount", ocrMinCount);
        variables.Add("NfcFailedTryCount", nfcMinCount);
        variables.Add("FaceFailedTryCount", faceMinCount);

        variables.Add("OcrFailedMaxTryCount", ocrMaxCount);
        variables.Add("NfcFailedMaxTryCount", nfcMaxCount);
        variables.Add("FaceFailedMaxTryCount", faceMaxCount);


        variables.Add("UserName", citizenShipNumber);


        dataChanged.additionalData = new ExpandoObject();
        dataChanged.additionalData.isEkyc = true;// gitmek istediği data 
        dataChanged.additionalData.callType = constCallType;
        // dataChanged.additionalData.customerName = registerResult.Name; // bu kısımları doldur.
        // dataChanged.additionalData.customerSurname = registerResult.Surname;
        dataChanged.additionalData.instanceId = instance;
        dataChanged.additionalData.applicantFullName = ApplicantFullName;
        dataChanged.additionalData.isMediaServerActive = false;
        dataChanged.additionalData.ekycEnvironment = "preprod";

        dataChanged.additionalData.pages = new List<EkycPageModel>{
            new EkycPageModel
            {
                type="waiting",
                image="wait",
                title="Kimlik Okuma Adımları Yükleniyor",
                navText = "Müşterimiz Ol",
                popUp= new EkycPopUpModel{
                    image="alert",
                    title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
                    subTexts = new List<string>{"Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?"},
                    buttons = new List<EkycButtonModel>{
                        new EkycButtonModel{
                            type="primary",
                            itemNo=1,
                            text = "Onayla",
                            action="exit",
                            transition = "amorphie-ekyc-exit"
                        },
                        new EkycButtonModel{
                            type="secondary",
                            itemNo=2,
                            text="Görüntülü Görüşmeye Devam Et",
                            action="cancel"
                        }
                    }
                },
                buttons = new List<EkycButtonModel>()
            }
        };

        dataChanged.additionalData.exitTransition = "amorphie-ekyc-exit";



        targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
        targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
        variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);
        return Results.Ok(variables);

    }


}
