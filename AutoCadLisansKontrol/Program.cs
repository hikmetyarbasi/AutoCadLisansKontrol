﻿using AutoCadLisansKontrol.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadLisansKontrol
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckAutoCadLicense obj = new CheckAutoCadLicense();
            obj.GetComputerInfoOnNetwork();
        }
    }
}