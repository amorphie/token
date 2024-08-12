using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.token.core.Enums;

namespace amorphie.token.core.Constants
{
    public class ErrorMessages
    {
        public static readonly Dictionary<string, string> OtpNotValid = new Dictionary<string, string>()
        {
            {"tr-TR","Şifre doğrulama kodunu hatalı girdiniz, yeniden deneyin."},
            {"en-EN","You entered the password verification code incorrectly, try again."},
            {"en-US","You entered the password verification code incorrectly, try again."},
        };

        public static readonly Dictionary<string, string> OtpTimeout = new Dictionary<string, string>()
        {
            {"tr-TR","Şifre doğrulama kodunun süresi dolmuştur. Yeniden gönderiniz."},
            {"en-EN","Password verification code has been expired, send again."},
            {"en-US","Password verification code has been expired, send again."},
        };

        public static readonly Dictionary<string, string> General = new Dictionary<string, string>()
        {
            {"tr-TR","Teknik Bir Sorun Oluştu. Daha Sonra Tekrar Deneyin"},
            {"en-EN","A technical problem occurred, Please Try Again Later"},
            {"en-US","A technical problem occurred, Please Try Again Later"},
        };

        public static readonly Dictionary<string, string> LoginUserNotFound = new Dictionary<string, string>()
        {
            {"tr-TR","Yanlış şifre girdiniz, lütfen tekrar deneyin."},
            {"en-EN","You entered the wrong password, please try again."},
            {"en-US","You entered the wrong password, please try again."},
        };

        public static readonly Dictionary<string, string> LoginWrongPassword = new Dictionary<string, string>()
        {
            {"tr-TR","Yanlış şifre girdiniz, lütfen tekrar deneyin."},
            {"en-EN","You entered the wrong password, please try again."},
            {"en-US","You entered the wrong password, please try again."},
        };

        public static readonly Dictionary<string, string> LoginBlockedUser = new Dictionary<string, string>()
        {
            {"tr-TR","Şifreyi 5 kere yanlış girdiğiniz için giriş yapılamıyor. Şifremi Unuttum butonuna basarak yeni şifre alabilir, giriş yapabilirsiniz."},
            {"en-EN","You cannot log in because you entered the wrong password 5 times. You can get a new password and log in by pressing the Forgot My Password button."},
            {"en-US","You cannot log in because you entered the wrong password 5 times. You can get a new password and log in by pressing the Forgot My Password button."},
        };

        public static readonly Dictionary<string, string> LoginBlockUser = new Dictionary<string, string>()
        {
            {"tr-TR","Çok Fazla Hatalı Giriş Sonrası Kullanıcı Blokelendi"},
            {"en-EN","User is Blocked Now After Several Login Attemps"},
            {"en-US","User is Blocked Now After Several Login Attemps"},
        };

        public static readonly Dictionary<string, string> SamePassword = new Dictionary<string, string>()
        {
            {"tr-TR","Önceki 5 şifrenizi tekrar kullanamazsınız, lütfen başka bir şifre belirleyin."},
            {"en-EN","You cannot reuse your previous 5 passwords, please set another password."},
            {"en-US","You cannot reuse your previous 5 passwords, please set another password."},
        };

        public static readonly Dictionary<string, string> NotAuthorized = new Dictionary<string, string>()
        {
            {"tr-TR","Giriş yapma yetkiniz yok."},
            {"en-EN","You are not authorized to login."},
            {"en-US","You are not authorized to login."},
        };

        public static readonly Dictionary<LoginErrors, Dictionary<string, string>> LoginErrorMap = new()
        {
            { LoginErrors.UserNotFound, LoginUserNotFound },
            { LoginErrors.WrongPassword, LoginWrongPassword },
            { LoginErrors.WrongOtp, OtpNotValid },
            { LoginErrors.OtpTimeout, OtpTimeout },
            { LoginErrors.BlockedUser, LoginBlockedUser },
            { LoginErrors.BlockUser, LoginBlockUser },
            { LoginErrors.SamePassword, SamePassword },
            { LoginErrors.NotAuthorized, NotAuthorized },
            { LoginErrors.General, General }
        };

        public static readonly Dictionary<string, string> OldIdentityCard = new()
        {
            {"tr-TR","Yeni şifre alma işlemine devam edebilmemiz için yeni tip TC kimlik kartınız olmalıdır.Detaylı bilgi için 0850 222 29 10 numaralı Müşteri İletişim Merkezimiz ile iletişime geçebilirsiniz."},
            {"en-EN","To proceed with the process of obtaining a new password, you must have the new type of Turkish ID card. For detailed information, you can contact our Customer Communication Center at 0850 222 29 10."},
            {"en-US","To proceed with the process of obtaining a new password, you must have the new type of Turkish ID card. For detailed information, you can contact our Customer Communication Center at 0850 222 29 10."}
        };

        public static readonly Dictionary<string, string> HasNotNfcAndNewIdentityCard = new()
        {
            {"tr-TR","Hizmet saatleri dışında olduğumuz için şifre alma sürecinize devam edilemedi. 08.00 - 01.00 saatleri arasında görüntülü görüşme ile müşteri temsilcimize bağlanarak veya NFC özelliği bulunan bir cihaz ile yeni şifre alma sürecine devam edebilirsiniz."},
            {"en-EN","Your password retrieval process could not be completed as we are outside of service hours. You can continue the process by connecting with our customer representative via video call between 08:00 - 01:00, or by using a device with NFC capability."},
            {"en-US","Your password retrieval process could not be completed as we are outside of service hours. You can continue the process by connecting with our customer representative via video call between 08:00 - 01:00, or by using a device with NFC capability."}
        };

        public static readonly Dictionary<string, string> WrongCardInfo = new()
        {
            {"tr-TR","Girdiğiniz kart bilgisi yanlış. Yeni şifre almak için lütfen bilgileri kontrol ediniz."},
            {"en-EN","The card information you entered is incorrect. Please check the information to obtain a new password."},
            {"en-US","The card information you entered is incorrect. Please check the information to obtain a new password."}
        };
         public static readonly Dictionary<string, string> WrongQuestionAnswer = new()
        {
            {"tr-TR","Girdiğiniz kart bilgisi yanlış. Yeni şifre almak için lütfen bilgileri kontrol ediniz."},
            {"en-EN","The card information you entered is incorrect. Please check the information to obtain a new password."},
            {"en-US","The card information you entered is incorrect. Please check the information to obtain a new password."}
        };
    }
}