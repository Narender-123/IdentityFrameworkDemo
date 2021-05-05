﻿using IdentityFrameworkDemo.Models;
using System.Threading.Tasks;

namespace IdentityFrameworkDemo.Services
{
    public interface IEmailService
    {
        Task SendTestEmail(UserEmailOptionsModel userEmailOptionsModel);
    }
}