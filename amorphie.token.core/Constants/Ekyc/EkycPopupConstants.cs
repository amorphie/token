namespace amorphie.token.core;

public static class EkycPopupConstants
{

    public static readonly EkycPopUpModel CancelEkyc = new EkycPopUpModel
    {
        image = "alert",
        title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
        subTexts = new List<string> { "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?" },
        buttons = EkycButtonGroupConstants.EkycExitAndCancel
    };


    public static readonly EkycPopUpModel VideoCallWithExitTransitionAndNumber = new EkycPopUpModel
    {
        title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
        subTexts = new List<string>{
                "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?",
                "Yardıma ihtiyaç duymanız durumunda <b>0850 222 29 10</b> numaralı telefondan bizi 7/24 arayabilirsiniz."
            },
        buttons = new List<EkycButtonModel>{
                new EkycButtonModel
                {
                    type="primary",
                    itemNo=1,
                    text="Onayla",
                    action="Exit",
                    transition="amorphie-ekyc-exit"
                },
                 new EkycButtonModel
                {
                    type="secondary",
                    itemNo=2,
                    text="Görüşmeye Devam Et",
                    action="Cancel"

                }
            }
    };
    
    public static readonly EkycPopUpModel VideoCallWithExitTransition = new EkycPopUpModel
    {
        title = "Görüntülü Görüşmeyi Sonlandırmak İstediğinize Emin Misiniz?",
        subTexts = new List<string>{
                "Görüntülü görüşme işleminiz sonlandırılacaktır, onaylıyor musunuz?"
            },
        buttons = new List<EkycButtonModel>{
                new EkycButtonModel
                {
                    type="primary",
                    itemNo=1,
                    text="Onayla",
                    action="Exit",
                    transition="amorphie-ekyc-exit"
                },
                 new EkycButtonModel
                {
                    type="secondary",
                    itemNo=2,
                    text="Görüşmeye Devam Et",
                    action="Cancel"

                }
            }
    };

}
