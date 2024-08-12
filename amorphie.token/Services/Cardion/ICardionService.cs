using Refit;

namespace amorphie.token.Services.Cardion;

public interface ICardionService
{
    [Get("/card/minimallist/{customerId}")]
    Task<CardionResponse<List<CardionCardValidatePinResult>>> GetCardListAsync(string customerId);

    [Post("/configuration/{customerId}/validatepin")]
    Task<CardionResponse<CardionCardValidatePinResult>> CardValidatePinAsync(string customerId, [Body] CardionCardValidatePinRequest input);
}

public class CardionResponse<T> where T: class, new()
{
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public T Result { get; set; }
}

public class CardionCardListResult
{
    public string CardId { get; set; }
    public string EmbossName { get; set; }
    public string CardType { get; set; }
    public string StatCode { get; set; }
}

public class CardionCardValidatePinResult
{
    public string CardIdLastFourDigits { get; set; }
}

public class CardionCardValidatePinRequest
{
    public string Pin { get; set; }
}
