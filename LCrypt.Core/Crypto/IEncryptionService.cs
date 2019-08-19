﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.Core.Crypto
{
    public interface IEncryptionService
        : IDisposable
    {
        Task EncryptAsync();

        Task DecryptAsync();
    }
}
