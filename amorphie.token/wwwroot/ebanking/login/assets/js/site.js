// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var submitButton = document.getElementById("submit-login");
var loginForm = document.getElementById('login-form');
submitButton.addEventListener('click',async function(e){
    e.preventDefault();
    submitButton.setAttribute("disabled","disabled");
    loginForm.submit();
    // var username = document.getElementById("username").value;
    // var password = document.getElementById("password").value;
    // await postTransition("amorphie-token-start-login",
    // {
    //     username:username,
    //     password:password
    // });
});

