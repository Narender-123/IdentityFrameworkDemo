﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityFrameworkDemo.Models;
using IdentityFrameworkDemo.Repository;
using Microsoft.AspNetCore.Authorization;

namespace IdentityFrameworkDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;

        //Here We Inject the AccountRepository so that we can make use all the features of that object 
        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [Route("signup")]
        public IActionResult SignUp()
        {
            return View();
        }

        [Route("signup")]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel) 
        {
            //If ModelState is Valid That means to Store in the Database
            if (ModelState.IsValid)
            {
                //Write Your Code
                //Below Code Will Invoke the Method which define the Logic to Add UserDetails
                var result = await _accountRepository.CreateAsyncUser(signUpModel);

                //Now we have to check for the Success
                if (!result.Succeeded) 
                {
                    foreach (var errorMessage in result.Errors) 
                    {
                        ModelState.AddModelError("",errorMessage.Description);
                    }
                    return View(signUpModel);
                }

                ModelState.Clear();
                return RedirectToAction("ConfirmEmail", new { email = signUpModel.Email});
            }
            return View(signUpModel);
        }

        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        //Here we define the Logic of SignIn Afeter the page is post back to the Server
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(SignInModel signInModel, string returnUrl)
        {
            //First We Check the Model States
            if (ModelState.IsValid) 
            {
                //Invoking Login Logic
                var result = await _accountRepository.PasswordSignInAsync(signInModel);

                //After we are getting the result we check the Success
                if (result.Succeeded) 
                {
                    if (!String.IsNullOrEmpty(returnUrl)) 
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("Index","Home");
                }
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("", "Plz Confirm the Email First and then Login");
                }
                else if (result.IsLockedOut) 
                {
                    ModelState.AddModelError("","Plz Attempt After Some Time ");
                }
                else
                {
                    //else we are geneate the Model state error
                    ModelState.AddModelError("", "Plz Enter the write Username and password");
                }
                return View(signInModel);
            }
            //else we return the Same Modle From Here
            return View(signInModel);
        }

        [Route("logout")]
        public async Task<IActionResult> Logout() 
        {
            await _accountRepository.SignOutAsync();
            return RedirectToAction("Index","Home");
        }

        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountRepository.ChangePasswordAsync(model);
                if (result.Succeeded)
                {
                    ViewBag.IsSuccees = true; 
                    ModelState.Clear();
                    return View();
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }
                return View(model);
            } 
            return View(model);
        }

        //This Controller Action Method Will Handle the GetRequest of the Link in the Email on the Server
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string uid, string token, string email)
        {
            EmailConfirmModel model = new EmailConfirmModel()
            {
                Email = email
            };
            if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token)) 
            {
                token = token.Replace(' ', '+');
               var result = await _accountRepository.ConfirmEmailAsync(uid, token);
                if (result.Succeeded) 
                {
                    //Flag for the Success
                    //ViewBag.IsSuccess = true;
                    model.EmailVerified = true;
                }
            }
            return View(model);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(EmailConfirmModel model)
        {
            if (ModelState.IsValid) {
                ApplicationUser user = await _accountRepository.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        model.IsConfirmed = true;
                        return View(model);
                    }
                    await _accountRepository.GenerateEmailConfirmationTokenAsync(user);
                    model.EmailSent = true;
                    ModelState.Clear();     
                }
                else 
                {
                    ModelState.AddModelError("","Something Went Wrong");
                }   
            }
            return View(model);
        }

        [AllowAnonymous ,HttpGet("forgot-password")]
        public IActionResult ForgotPassword() 
        {
            return View();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid) 
            {
                var user = await _accountRepository.GetUserByEmailAsync(model.Email);
                if (user!=null)
                {
                    await _accountRepository.GenerateForgotPasswordTokenAsync(user);
                }
                
                ModelState.Clear();
                model.EmailSent = true;
            }
            return View(model);
        }

        [AllowAnonymous, HttpGet("reset-password")]
        public IActionResult ResetPassword(string uid, string token)
        {
            ResetPasswordModel model = new ResetPasswordModel
            {
                UserId = uid,
                Token = token
            };
            return View(model);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                model.Token = model.Token.Replace(' ','+');
                var result = await _accountRepository.ResetPasswordAsync(model);
                if (result.Succeeded)
                {
                    ModelState.Clear();
                    model.IsSuccess = true;
                    return View(model);
                }
                foreach (var error in result.Errors) 
                {
                    ModelState.AddModelError("",error.Description);
                }                
            }
            return View(model);
        }

    }
}
