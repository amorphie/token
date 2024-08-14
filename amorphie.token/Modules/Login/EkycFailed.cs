using System.Dynamic;
using amorphie.token.core;
using Microsoft.AspNetCore.Mvc;

namespace amorphie.token;

public static class EkycFailed
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public static async Task<IResult> ConnectionExit([FromBody] dynamic body,
    [FromServices] IEkycService ekycService)
  {

    var transitionName = body.GetProperty("LastTransition").ToString();
    var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");
    dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());


    dynamic targetObject = new System.Dynamic.ExpandoObject();
    targetObject.Data = dataChanged;


    var callType = body.GetProperty("CallType").ToString();
    var instance = body.GetProperty("Instance").ToString();
    // var name = body.GetProperty("Name").ToString();
    // var surname = body.GetProperty("Surname").ToString();
    // Add additional data object here !
    dataChanged.additionalData = new ExpandoObject();
    dataChanged.additionalData.isEkyc = true;// gitmek istediği data 
    dataChanged.additionalData.callType = callType;
    var ApplicantFullName = body.GetProperty("ApplicantFullName").ToString();
    dataChanged.additionalData.applicantFullName = ApplicantFullName;
    // dataChanged.additionalData.customerName = name; // bu kısımları doldur.
    // dataChanged.additionalData.customerSurname = surname;
    dataChanged.additionalData.instanceId = instance;
    dataChanged.additionalData.pages = new List<EkycPageModel>{
           EkycAdditionalDataContstants.StandartItem,
           EkycAdditionalDataContstants.ConnectionFailedItem
        };


    dataChanged.additionalData.exitTransition = "amorphie-ekyc-exit";
    dynamic variables = new Dictionary<string, dynamic>();
    // variables here !

    targetObject.Data = dataChanged;
    targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
    targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
    variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);

    return Results.Ok(variables);
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  public static async Task<IResult> GlobalFailed([FromBody] dynamic body,
    [FromServices] IEkycService ekycService)
  {

    Console.WriteLine(body.ToString());
    var transitionName = body.GetProperty("LastTransition").ToString();
    var failedStepName = body.GetProperty("FailedStepName").ToString();
    var callType = body.GetProperty("CallType").ToString();
    var dataBody = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");
    dynamic dataChanged = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(dataBody.ToString());


    dynamic targetObject = new System.Dynamic.ExpandoObject();
    targetObject.Data = dataChanged;
    // Add additional data object here !
    dataChanged.additionalData = new ExpandoObject();

    dataChanged.additionalData.exitTransition = "amorphie-ekyc-exit";
    dynamic variables = new Dictionary<string, dynamic>();
    // variables here !
    if (failedStepName == "ocr")
    {
      variables.Add("EkycResult", EkycResultConstants.FailedOcrMaxTryCount);
      variables.Add("EkycButton", "None");
      variables.Add("EkycCallType", callType);
      
    }


    targetObject.Data = dataChanged;
    targetObject.TriggeredBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
    targetObject.TriggeredByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());
    variables.Add($"TRX{transitionName.ToString().Replace("-", "")}", targetObject);

    return Results.Ok(variables);
  }


}
