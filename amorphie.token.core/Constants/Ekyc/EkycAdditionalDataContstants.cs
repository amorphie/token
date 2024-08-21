namespace amorphie.token.core;

public static class EkycAdditionalDataContstants
{
    public static readonly EkycPageModel StandartItem = new EkycPageModel
    {
        type = "waiting",
        image = "wait",
        title = "Kontrolleriniz Devam Ediyor",
        navText = "Müşterimiz Ol",
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = new List<EkycButtonModel>()
    };

    public static readonly EkycPageModel ConnectionFailedItem = new EkycPageModel
    {
        type = "error",
        image = "generic",
        title = "Bağlantı Hatası",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "Teknik sebeplerden dolayı sayfa yüklenemedi. Yeniden dene diyerek işleme kaldığınız yerden devam edebilirsiniz." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.RetryAndEkycExit

    };

    public static readonly EkycPageModel OcrFailedItemForRetry = new EkycPageModel
    {
        type = "error",
        image = "error",
        title = "Kimlik Doğrulama Yapılamadı",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "Lütfen kimlik kartınızı hareket ettirmeden, yeterli ışık alan bir ortamda yeniden deneyin." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.Retry

    };


    public static readonly EkycPageModel OcrFailedItemForIdentityMatch = new EkycPageModel
    {
        type = "error",
        image = "error",
        title = "Kimlik Doğrulama Yapılamadı",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "Lütfen bilgilerini girdiğiniz kimliği okutun." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.Retry

    };

    public static readonly EkycPageModel OcrSuccessForNfcItem = new EkycPageModel
    {
        type = "nfc",
        image = "nfc",
        title = "NFC Kimlik Tarama",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "Kimliğinizi NFC ile doğrulatmak için lütfen kimlik kartınızı cihazınızın NFC alanına doğru yaklaştırın." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = new List<EkycButtonModel>()
    };


    public static readonly EkycPageModel NfcFailedMinForRetry = new EkycPageModel
    {

        type = "error",
        image = "error",
        title = "Kimlik Doğrulama Yapılamadı",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "NFC doğruluma yapmak için lütfen kimlik kartınızı cep telefonunuzun ön kamerasına yaklaştırın." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.Retry
    };

    public static readonly EkycPageModel NfcFailedBiggerThanMinForRetry = new EkycPageModel
    {

        type = "error",
        image = "error",
        title = "Kimlik Doğrulama Yapılamadı",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "Doğrulama yapılamadı. NFC doğrulama işlemini tekrar deneyebilir veya müşteri temsilcisinden destek almak için Görüntülü Görüşme adımına ilerleyebilirsiniz." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.RetryAndSkip

    };

    public static readonly EkycPageModel FaceSuccessConfirm = new EkycPageModel
    {
        type = "confirm",
        image = "confirm",
        title = "Görüntülü Görüşmeye Aktarılacaksınız",
        navText = "Müşterimiz Ol",
        subTexts = new List<string>
        {
            "Görüntülü görüşme kayıt altına alınacaktır.",
            "Görüntülü olarak gerçekleşecek görüşmenizin kayıt altına alınmasını onaylıyor musunuz?"
        },
        popUp = new EkycPopUpModel
        {

            title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
            subTexts = new List<string> { "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?" },
            buttons = new List<EkycButtonModel>{
                        new EkycButtonModel{
                            type="primary",
                            itemNo=1,
                            text = "Onayla",
                            action="Exit",
                            transition = "amorphie-ekyc-finish"
                        },
                        new EkycButtonModel{
                            type="secondary",
                            itemNo=2,
                            text="Görüntülü Görüşmeye Devam Et",
                            action="Cancel"
                        }
                    }
        },
        popUpVideoCall = new EkycPopUpModel
        {
            title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
            subTexts = new List<string>{
                "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?",
                "Yardıma ihtiyaç duymanız durumunda <b>0850 222 29 10</b> numaralı telefondan bizi 7/24 arayabilirsiniz."
            },
            buttons = EkycButtonGroupConstants.FinishAndCancel
        },
        buttons = new List<EkycButtonModel>
        {
            new EkycButtonModel
                {
                    type="primary",
                    itemNo=1,
                    text="Onayla ve Bağlan",
                    action="ConfirmConnect"

                },
                new EkycButtonModel
                {
                    type="secondary",
                    itemNo=2,
                    text="Giriş Ekranına Dön",
                    action="PopUpVideoCall"
                },
        }
    };

    public static readonly EkycPageModel SkipFaceForVideoCall = new EkycPageModel
    {
        type = "confirm",
        image = "confirm",
        title = "Görüntülü Görüşmeye Aktarılacaksınız",
        navText = "Müşterimiz Ol",
        subTexts = new List<string>
        {
            "Görüntülü görüşme kayıt altına alınacaktır.",
            "Görüntülü olarak gerçekleşecek görüşmenizin kayıt altına alınmasını onaylıyor musunuz?"
        },
        popUp = new EkycPopUpModel
        {

            title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
            subTexts = new List<string> { "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?" },
            buttons = new List<EkycButtonModel>{
                        new EkycButtonModel{
                            type="primary",
                            itemNo=1,
                            text = "Onayla",
                            action="Exit",
                            transition = "amorphie-ekyc-finish"
                        },
                        new EkycButtonModel{
                            type="secondary",
                            itemNo=2,
                            text="Görüntülü Görüşmeye Devam Et",
                            action="Cancel"
                        }
                    }
        },
        popUpVideoCall = new EkycPopUpModel
        {
            title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
            subTexts = new List<string>{
                "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?",
                "Yardıma ihtiyaç duymanız durumunda <b>0850 222 29 10</b> numaralı telefondan bizi 7/24 arayabilirsiniz."
            },
            buttons = EkycButtonGroupConstants.FinishAndCancel
        },
        buttons = new List<EkycButtonModel>
        {
            new EkycButtonModel
                {
                    type="primary",
                    itemNo=1,
                    text="Onayla ve Bağlan",
                    action="ConfirmConnect"

                },
                new EkycButtonModel
                {
                    type="secondary",
                    itemNo=2,
                    text="Giriş Ekranına Dön",
                    action="PopUpVideoCall"
                },
        }
    };


    public static readonly EkycPageModel FaceFailedMinForRetry = new EkycPageModel
    {
        type = "error",
        image = "error",
        title = "Kimlik Doğrulama Yapılamadı",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "Lütfen yüz tanıma işlemini yeniden deneyin." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.Retry

    };

    public static readonly EkycPageModel FaceFailedBiggerThanMinForRetry = new EkycPageModel
    {
        type = "error",
        image = "error",
        title = "Kimlik Doğrulama Yapılamadı",
        navText = "Müşterimiz Ol",
        subTexts = new List<string> { "Yüz tanıma işlemini tekrar deneyebilir veya müşteri temsilcisinden destek almak için Görüntülü Görüşme adımına ilerleyebilirsiniz" },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.RetryAndSkip
    };

    public static readonly EkycPageModel VideoCallReadySuccessWait = new EkycPageModel
    {
        type = "videoCallWait",
        navText = "Müşterimiz Ol",
        image = "confirm",
        title = "Görüntülü Görüşme İçin Sıradasınız",
        isInVideoCall = true,
        subTexts = new List<string> { "İletişim Merkezi’ne en kısa sürede otomatik olarak bağlanacaksınız. " },
        popUp = new EkycPopUpModel
        {
            title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
            subTexts = new List<string> { "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?" },
            buttons = new List<EkycButtonModel>{
                  new EkycButtonModel{
                            type="primary",
                            itemNo=1,
                            text = "Onayla",
                            action="Exit",
                            transition = "amorphie-ekyc-exit"
                        },
                        new EkycButtonModel{
                            type="secondary",
                            itemNo=2,
                            text="Görüntülü Görüşmeye Devam Et",
                            action="Cancel"
                        }
            },

        },
        popUpVideoCall = new EkycPopUpModel
        {
            title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
            subTexts = new List<string>{
                "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?",
                "Yardıma ihtiyaç duymanız durumunda <b>0850 222 29 10</b> numaralı telefondan bizi 7/24 arayabilirsiniz."
            },
            buttons = EkycButtonGroupConstants.FinishAndCancel
        },

        buttons = new List<EkycButtonModel>
        {
             new EkycButtonModel
             {
                type="primary",
                itemNo=1,
                text="Giriş Ekranına Dön",
                action="PopUpVideoCall"
             }
        }
    };


    public static readonly EkycPageModel VideoCallReadyFail = new EkycPageModel
    {
        type = "error",
        navText = "Müşterimiz Ol",
        image = "generic",
        title = "Bağlantı Hatası",
        isInVideoCall = true,
        subTexts = new List<string> { "Teknik sebeplerden dolayı sayfa yüklenemedi. Yeniden dene diyerek işleme kaldığınız yerden devam edebilirsiniz." },
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = EkycButtonGroupConstants.RetryAndEkycExit
    };

    public static readonly EkycPageModel VideoCallReadySuccessFaceRetryFail = new EkycPageModel
    {
        type = "faceRetryFailed",
        navText = "Müşterimiz Ol",
        image = "error",
        title = "Bağlantı Hatası",
        isInVideoCall = true,
        subTexts = new List<string> { "Lütfen yüz tanıma işlemini yeniden deneyin." },
        popUpVideoCall = EkycPopupConstants.VideoCallWithExitTransitionAndNumber,
        buttons = new List<EkycButtonModel>{
            new EkycButtonModel{
                    type= "primary",
                    itemNo = 1,
                    text = "Görüntülü Görüşme",
                    action = "ReturnVideoCall"
            },
            new EkycButtonModel{
                    type= "secondary",
                    itemNo = 2,
                    text = "Giriş Ekranına Dön",
                    action = "Exit",
                    transition="amorphie-ekyc-exit"
            }
        }

    };


    public static readonly EkycPageModel VideoCallReadySuccessNfcRetryFail = new EkycPageModel
    {
        type = "nfcRetryFailed",
        navText = "Müşterimiz Ol",
        image = "error",
        title = "Bağlantı Hatası",
        isInVideoCall = true,
        subTexts = new List<string> { "NFC doğruluma yapmak için lütfen kimlik kartınızı cep telefonunuzun ön kamerasına yaklaştırın." },
        popUpVideoCall = EkycPopupConstants.VideoCallWithExitTransitionAndNumber,
        buttons = new List<EkycButtonModel>{
           new EkycButtonModel{
                    type= "primary",
                    itemNo = 1,
                    text = "Görüntülü Görüşme",
                    action = "ReturnVideoCall"
            },
            new EkycButtonModel{
                    type= "secondary",
                    itemNo = 2,
                    text = "Giriş Ekranına Dön",
                    action = "Exit",
                    transition="amorphie-ekyc-exit"
            }
        }

    };

    public static readonly EkycPageModel VideoCallReadySuccessOcrRetryFail = new EkycPageModel
    {
        type = "ocrRetryFailed",
        navText = "Müşterimiz Ol",
        image = "error",
        title = "Bağlantı Hatası",
        isInVideoCall = true,
        subTexts = new List<string> { "Lütfen kimlik kartınızı hareket ettirmeden, yeterli ışık alan bir ortamda yeniden deneyin." },
        popUpVideoCall = EkycPopupConstants.VideoCallWithExitTransitionAndNumber,
        buttons = new List<EkycButtonModel>{
            new EkycButtonModel{
                    type= "primary",
                    itemNo = 1,
                    text = "Görüntülü Görüşme",
                    action = "ReturnVideoCall"
            },
            new EkycButtonModel{
                    type= "secondary",
                    itemNo = 2,
                    text = "Giriş Ekranına Dön",
                    action = "Exit",
                    transition="amorphie-ekyc-exit"
            }
        }

    };


    public static readonly EkycPageModel NfcActivePassiveAuth = new EkycPageModel
    {
        type = "auth",
        navText = "Müşterimiz Ol",
        image = "error",
        title = "NFC Okuma Hatası",
        subTexts = new List<string> { "Nfc Aktif Pasif Otantikasyon Hatası (Güncellenecek)" },
        popUpVideoCall = EkycPopupConstants.VideoCallWithExitTransition,
        buttons = new List<EkycButtonModel>{
            new EkycButtonModel{
                    type= "primary",
                    itemNo = 1,
                    text = "Giriş Ekranına Dön",
                    action = "Exit",
                    transition = "amorphie-ekyc-exit"
            }
        }

    };


    public static readonly EkycPageModel NfcType = new EkycPageModel
    {
        type = "nfc",
        navText = "Müşterimiz Ol",
        image = "error",
        title = "NFC Okuma Hatası",
        subTexts = new List<string> { "Nfc Aktif Pasif Otantikasyon Hatası (Güncellenecek)" },
        popUpVideoCall = EkycPopupConstants.VideoCallWithExitTransition,
        buttons = new List<EkycButtonModel>{
            new EkycButtonModel{
                    type= "primary",
                    itemNo = 1,
                    text = "Giriş Ekranına Dön",
                    action = "Exit",
                    transition = "amorphie-ekyc-exit"
            }
        }

    };

    public static readonly EkycPageModel EkycPrepare = new EkycPageModel
    {
        type = "waiting",
        image = "wait",
        title = "Kontrolleriniz Devam Ediyor",
        navText = "Müşterimiz Ol",
        popUp = EkycPopupConstants.CancelEkyc,
        buttons = new List<EkycButtonModel>()
    };
}
