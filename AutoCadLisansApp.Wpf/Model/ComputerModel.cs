﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MaterialDesignDemo.Model
{
    public class ComputerModel:AutoCadLisansKontrol.DAL.Computer
    {
        
        public Visibility Visibility { get { return IsVisible == true ? Visibility.Visible : Visibility.Hidden; } }
    }
}