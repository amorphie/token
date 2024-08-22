var otpButton = document.getElementById("otp-submit");
var otpForm = document.getElementById('otp-form');
var alert = document.getElementById("alert-area");
otpButton.addEventListener('click',async function(e){
    e.preventDefault();
    
    alert.style.display = 'none';

    otpButton.setAttribute("disabled","disabled");
    
    const response = await fetch(otpForm.action,{method:"POST",body: new FormData(otpForm)});
    if(response.status === 401)
    {
        alert.style.display = 'block';
        alert.innerHTML = "Hatalı Otp";
    }

    if(response.status === 200)
    {
        alert.style.display = 'none';
        alert.innerHTML = "";
        var responseBody = await response.json();
        
        var redirectForm = document.getElementById("redirect-form");
        redirectForm.action = responseBody.redirectUri;
        redirectForm.submit();
        
    }


    otpButton.removeAttribute("disabled")
    //otpForm.submit();
});