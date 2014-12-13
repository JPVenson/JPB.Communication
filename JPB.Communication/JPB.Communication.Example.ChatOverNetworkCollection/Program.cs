/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.Shared;

namespace JPB.Communication.Example.ChatOverNetworkCollection
{
    public class Program
    {
        /// <summary>
        /// This example will show the usage of the NetworkValueBag
        /// WIP
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //create an Instance of the NetworkValueBag

            var networkValueCollection = NetworkValueBag<string>.CreateNetworkValueCollection(1337, 
                
                //To have multible NetworkBags on the Same system and Port we need a Valid Uniq identifyer
                "AnyValidStringLikeAGuid: 46801E06-AB14-4910-BA95-7E13F58F4186");

            Console.WriteLine("Connect to a server or be the first?");


        }
    }
}
