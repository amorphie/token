using System.Dynamic;
using amorphie.token.core;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token;

public static class EkycSelfServiceCheck
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


        
        dynamic variables = new Dictionary<string, dynamic>();
        dataChanged.additionalData = new ExpandoObject();



       
        
        // var ocrStatus =Convert.ToBoolean(body.GetProperty("OcrStatus").ToString());
        // var nfcStatus =Convert.ToBoolean(body.GetProperty("NfcStatus").ToString());
        // var faceStatus =Convert.ToBoolean(body.GetProperty("FaceReadStatus").ToString());

        // if(ocrStatus && nfcStatus && faceStatus){
        //      selfServiceCheck = true;
        // }
           
        dataChanged.additionalData.exitTransition = "amorphie-ekyc-exit";
      

        variables.Add("Init", true);
        variables.Add("EkycResult", EkycResultConstants.SelfServiceCompleted);
        variables.Add("EkycButton","None"); // self service ile işlem sonlandığında üst flow için
        
        
        return Results.Ok(variables);

        // return Task.FromResult(Results.Ok("data"));
    }
}
