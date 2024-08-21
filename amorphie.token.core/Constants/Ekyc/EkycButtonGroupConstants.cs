namespace amorphie.token.core;

public static class EkycButtonGroupConstants
{
    public static readonly List<EkycButtonModel> EkycExitAndCancel = new List<EkycButtonModel>{
                        new EkycButtonModel{
                            type="primary",
                            itemNo=1,
                            text = "Girişe Dön",
                            action="Exit",
                            transition = "amorphie-ekyc-exit"
                        },
                        new EkycButtonModel
                        {
                            type = "secondary",
                            itemNo = 2,
                            text = "Vazgeç",
                            action = "Cancel"
                        }
    };

    public static readonly List<EkycButtonModel> RetryAndEkycExit = new List<EkycButtonModel>{
                  new EkycButtonModel
                  {
                     type="primary",
                     itemNo=1,
                     text = "Yeniden Dene",
                     action="Retry",
                  },
                  new EkycButtonModel
                  {
                     type="secondary",
                     itemNo=2,
                     text="Giriş Ekranına Dön",
                     action="Exit",
                     transition ="amorphie-ekyc-exit"

                  }
    };
        
    public static readonly List<EkycButtonModel> RetryAndSkip = new List<EkycButtonModel>{
                  new EkycButtonModel
                  {
                     type="primary",
                     itemNo=1,
                     text = "Yeniden Dene",
                     action="Retry",
                  },
                  new EkycButtonModel
                  {
                     type="secondary",
                     itemNo=2,
                     text="Görüntülü Görüşmeye Bağlan",
                     action="Skip"

                  }
    };
            
    public static readonly List<EkycButtonModel> Retry = new List<EkycButtonModel>{
                  new EkycButtonModel
                  {
                     type="primary",
                     itemNo=1,
                     text = "Yeniden Dene",
                     action="Retry",
                  }
    };

    public static readonly List<EkycButtonModel> FinishAndCancel = new List<EkycButtonModel>{
                new EkycButtonModel
                {
                    type="primary",
                    itemNo=1,
                    text="Onayla",
                    action="Exit",
                    transition="amorphie-ekyc-finish"
                },
                 new EkycButtonModel
                {
                    type="secondary",
                    itemNo=2,
                    text="Görüşmeye Devam Et",
                    action="Cancel"

                }
    };

}
