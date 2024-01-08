using System.Dynamic;
using System.Text.Json;
using amorphie.core.Zeebe.dapr;
using amorphie.token.core.Models.InternetBanking;
using amorphie.token.data;
using amorphie.token.Services.InternetBanking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Modules.Login
{
    public static class SetNewPassword
    {
        public static async Task<IResult> setNewPassword(
        [FromBody] dynamic body,
        [FromServices] IbDatabaseContext ibContext,
        [FromServices] IInternetBankingUserService internetBankingUserService
        )
        {
            Console.WriteLine("Step - 1");
            var newPassword = body.GetProperty("TRXamorphiemobileloginsetnewpassword").GetProperty("Data").GetProperty("entityData").GetProperty("newPassword").ToString();
            Console.WriteLine("Step - 2");
            var ibUserSerialized = body.GetProperty("ibUserSerialized").ToString();
            IBUser ibUser = JsonSerializer.Deserialize<IBUser>(ibUserSerialized);
            Console.WriteLine("Step - 3");
            var oldPasswords = await ibContext.Password.Where(p => p.UserId == ibUser.Id).OrderByDescending(p => p.CreatedAt).Take(5).ToListAsync();

            PasswordHasher passwordHasher = new();
            IBPassword password = new IBPassword
            {
                AccessFailedCount = 0,
                CreatedByUserName = "Amorphie",
                UserId = ibUser.Id
            };
            
            Console.WriteLine("Step - 4");
            dynamic variables = new ExpandoObject();
            foreach (var pass in oldPasswords)
            {
                Console.WriteLine("Step - 6");
                Console.WriteLine("hashed pass: "+pass.HashedPassword);
                Console.WriteLine("new password: "+newPassword);
                Console.WriteLine("pass ID: "+pass.Id);
                Console.WriteLine("pass result: "+internetBankingUserService.VerifyPassword(pass.HashedPassword,newPassword,pass.Id.ToString()));
                if (passwordHasher.VerifyHashedPassword(pass.HashedPassword,newPassword,pass.Id.ToString()) != PasswordVerificationResult.Failed)
                {
                    Console.WriteLine("Step - 7");
                    variables.status = false;
                    variables.message = "New Password Can Not Be Same With Last 5 Passwords";
                    return Results.Ok(variables);
                }
            }

            password.HashedPassword = passwordHasher.HashPassword(newPassword, password.Id.ToString());
            Console.WriteLine("Step - 8");
            await ibContext.Password.AddAsync(password);
            await ibContext.SaveChangesAsync();


            variables.status = true;
            return Results.Ok(variables);
        }
    }
}