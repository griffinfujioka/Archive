﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography; 
using Windows.Storage.Streams; 

// This class helps to hash tokens for the Archive API
namespace Archive.Common
{
    public static class HashHelper
    {

        public static string Sha1(string str)
        {

            // put the string in a buffer, UTF-8 encoded...
            IBuffer input = CryptographicBuffer.ConvertStringToBinary(str,
                BinaryStringEncoding.Utf8);

            // hash it
            var hasher = HashAlgorithmProvider.OpenAlgorithm("SHA1");     
            IBuffer hashed = hasher.HashData(input);

            // format it...
            var textBase64 = CryptographicBuffer.EncodeToBase64String(hashed);

            // This is one implementation of the abstract class SHA1.

            return textBase64; 

        }




        //public static string MD5(string str)
        //{

        //    var data = Encoding.UTF8.GetBytes(str);

        //    MD5 sha = new MD5CryptoServiceProvider();

        //    // This is one implementation of the abstract class SHA1.

        //    return Convert.ToBase64String(sha.ComputeHash(data));

        //}

    }
}
