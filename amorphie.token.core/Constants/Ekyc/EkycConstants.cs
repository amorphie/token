namespace amorphie.token.core;

public static class EkycConstants
{
    // public static readonly string AddressConfirmationDocument = "address-confirmation-document";

    // public static readonly string IDFrontDocument = "id-front-document";

    // public static readonly string AnotherDocument = "another";

    // public static readonly string IDFrontHologramDocument = "id-front-hologram";

    public static readonly string Session = "Session";

    // public static string GetRequestReason(string key)
    // {
    //     Dictionary<string, string> dictionary = new Dictionary<string, string>
    //     {
    //         { "depositApply", "Mevduat başvurusu" },
    //         { "loanApply", "Kredi başvurusu" },
    //         { "updateIdentityCard", "Kimlik güncelleme" },
    //         { "removeSimBloke", "Sim bloke kaldırma" },
    //         { "phoneUpdate", "Cep tel güncelleme" },
    //         { "passwordRenew", "Yeni şifre talebi" },
    //         { "pyhsicalPropertyDateUpdate", "Fiziki varlık tarihi güncelleme" },
    //         { "induvidialApply", "Bireysel başvuru" },
    //         { "ecommerceHEPSIBURADAApply", "Hepsiburada başvurusu" }
    //     };
    //     if (!dictionary.Keys.Contains(key))
    //     {
    //         return null;
    //     }

    //     return dictionary[key];
    // }

    //TODO: they'll move to Vault 
    public static int OcrFailedTryCount = 1;
    public static int OcrFailedMaxTryCount = 10;
    public static int NfcFailedTryCount = 4;
    public static int NfcFailedMaxTryCount = 10;
    public static int FaceFailedTryCount = 3;
    public static int FaceFailedMaxTryCount = 10;
}

