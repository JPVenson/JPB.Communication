﻿using JPB.Communication.ComBase.Serializer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPB.Communication.PCLIntigration.Contracts.Security
{
    public interface ISecureMessageProvider
    {
        /// <summary>
        ///     Is used to convert the Complete Message Object into a byte[] that will be transferted to the remote computer
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        byte[] Encrypt(byte[] networkMessage);


        /// <summary>
        ///     Converts the output from the TCP network adapter into a valid TCP message
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        byte[] Decrypt(byte[] networkMessage);
    }
}
