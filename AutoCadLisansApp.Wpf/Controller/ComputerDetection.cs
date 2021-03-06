﻿
using AutoCadLisansKontrol.DAL;
using LicenseController.autocad.masterkey.ws;
using MaterialDesignDemo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace AutoCadLisansKontrol.Controller
{
    public class ComputerDetection
    {
        public static int FirmId { get; set; }
        static object thisLock = new object();
        private static string localip;
        public static ComputerDTO ExecuteLocal()
        {
            var localcomp = new ComputerDTO
            {
                Ip = GetLocalIp(),
                Name = GetLocalMachineName(),
                Type = "Local",
                //PyshicalAddress="",
                IsRootMachine = false,
                IsComputer = true,
                InsertDate = DateTime.Now
            };
            return localcomp;
        }
        public static List<ComputerModel> Execute()
        {
            var net = GetComputerFromArpTable();
            var arp = GetComputerFromNetView();

            arp.AddRange(net);
            return arp;
            //return arp.ConvertAll(x => new ComputerModel { FirmId = x.FirmId, Id = x.Id, InsertDate = x.InsertDate, Ip = x.Ip, IsComputer = x.IsComputer, IsRootMachine = x.IsRootMachine, IsVisible = x.IsVisible, Name = x.Name, PyshicalAddress = x.PyshicalAddress, Type = x.Type, IsProgress = true });
        }
        private static List<ComputerModel> GetComputerFromNetView()
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = System.IO.Directory.GetCurrentDirectory() + @"\BatFile\netview.bat";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();

            List<string> lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Where(x => x != "" && x.StartsWith("\\")).Select(x => x.Replace("\\", "").TrimEnd()).ToList();


            List<string> multiple = lines.Where(x => x.Contains(" ")).ToList();

            foreach (var item in multiple)
            {
                List<string> child = item.Split(' ').ToList().Where(x => x != "").ToList();
                lines.AddRange(child);
            }
            lines = lines.Where(x => !multiple.Contains(x)).ToList();

            List<ComputerModel> networkmachine = new List<ComputerModel>();

            for (int i = 0; i < lines.Count; i++)
            {
                networkmachine.Add(new ComputerModel
                {
                    // Ip= GetIpAddressFromName(lines[i]),
                    Name = lines[i],
                    Type = "NetView",
                    //PyshicalAddress="",
                    IsRootMachine = false,
                    IsComputer = true,
                    IsProgress=true,
                    InsertDate=DateTime.Now,
                    FirmId= FirmId
                });
            }
            p.WaitForExit();

            return networkmachine.ToList();
        }

        private static List<ComputerModel> GetComputerFromArpTable()
        {
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = System.IO.Directory.GetCurrentDirectory() + @"\BatFile\arptable.bat";
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();

            List<string> lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Where(x => x != "").ToList();

            var index = lines.FindIndex(x => x.Contains("Internet Address"));
            lines.RemoveAt(index);
            index = lines.FindIndex(x => x.Contains("Interface:"));
            localip = lines[index].Split(' ')[1];
            lines.RemoveAt(index);
            index = lines.FindIndex(x => x.Contains("arp -a"));
            lines.RemoveAt(index);

            List<ComputerModel> networkmachine = new List<ComputerModel>();
            for (int i = 1; i < lines.Count; i++)
            {
                var compline = lines[i].Split(' ').Where(x => x != "").ToList();


                networkmachine.Add(new ComputerModel
                {
                    Ip = compline[0],
                    //Name = compline[1],
                    PyshicalAddress = compline[1],
                    Type = "ArpTable",
                    IsRootMachine = false,
                    IsComputer = true,
                    IsProgress=true,
                    FirmId = FirmId
                });

            }


            p.WaitForExit();

            return networkmachine.ToList();
        }

        private static string GetMacAddress(string ipAddress)
        {
            var filelocation = System.IO.Directory.GetCurrentDirectory() + @"\BatFile\getmacaddress.bat";
            string macAddress = string.Empty;
            //ADDED THIS
            System.IO.StreamWriter file = new System.IO.StreamWriter(filelocation);
            file.WriteLine("nbtstat -A " + ipAddress);
            file.Close();
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = filelocation;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            //var subnet = GetSubnetMask(28);

            //string[] lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            //    var line = lines.Where(x => x.Contains(ipAddress)).First();
            //    if (line != null)
            //    {
            //        var substrings = line.Split(' ').Where(x => x != "").ToList();
            //        return substrings[1];
            //    }
            //    else
            //    {
            //        return "not found";
            //    }
            return "";
        }
        private static string GetIpAddressFromName(string name)
        {
            try
            {
                IPAddress[] ips;
                ips = Dns.GetHostAddresses(name);

                if (ips.Length > 0)
                    return ips[0].ToString();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        private static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }
        private static IPAddress GetNetworkAddress()
        {

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
                if (f.OperationalStatus == OperationalStatus.Up)
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        return d.Address;
                    }
            return null;
        }

        private static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }
        private static string GetSubnetMask(byte subnet)
        {
            long mask = (0xffffffffL << (32 - subnet)) & 0xffffffffL;
            mask = IPAddress.HostToNetworkOrder((int)mask);
            return new IPAddress((UInt32)mask).ToString();
        }
        public static ComputerModel GetAdditionalInfo(ComputerModel comp)
        {

            lock (thisLock)
            {
                if (comp.Type == "ArpTable")
                {
                    comp.Name = GetMachineNameFromIPAddress(comp.Ip);
                    comp.IsVisible = PingHost(comp.Ip);
                }
                else if (comp.Type == "NetView")
                {
                    comp.Ip = GetIpAddressFromName(comp.Name);
                    //comp.PyshicalAddress = GetMacAddress(comp.Ip);
                    comp.IsVisible = PingHost(comp.Ip);
                }
            }
            return comp;
        }
        private static string GetMachineNameFromIPAddress(string ipAdress)
        {
            string machineName = string.Empty;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(ipAdress);

                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                // Machine not found...
            }
            return machineName;
        }
        private static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            return pingable;
        }
        private static string GetLocalMachineName()
        {
            return Environment.MachineName;
        }
        private static string GetLocalIp()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
        }
        public static ObservableCollection<ComputerModel> FilterComputer(ObservableCollection<ComputerModel> comps)
        {

            var tempcomp = new List<ComputerModel>(comps);
            var filter1 = GetNetworkAddress();
            var filter2 = GetSubnetMask(28);
            var filter3 = GetBroadcastAddress(System.Net.IPAddress.Parse(localip), System.Net.IPAddress.Parse(filter2));
            var localname = ExecuteLocal();

            var index = tempcomp.FindIndex(x => x.Ip.Contains(filter1.ToString()));
            if (index != -1) tempcomp.RemoveAt(index);

            index = tempcomp.FindIndex(x => x.Ip.Contains(filter2.ToString()));
            if (index != -1) tempcomp.RemoveAt(index);

            index = tempcomp.FindIndex(x => x.Ip.Contains(filter3.ToString()));
            if (index != -1) tempcomp.RemoveAt(index);

            index = tempcomp.FindIndex(x => x.Name == localname.Name.ToString());
            if (index != -1) tempcomp.RemoveAt(index);

            var list = tempcomp.Where(x => x.Ip.StartsWith("255.".ToString())).ToList();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    tempcomp.Remove(item);
                }
            }

            list = tempcomp.Where(x => x.Ip.StartsWith("240.".ToString())).ToList();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    tempcomp.Remove(item);
                }
            }

            list = tempcomp.Where(x => x.Ip.StartsWith("224.".ToString())).ToList();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    tempcomp.Remove(item);
                }
            }

            list = tempcomp.Where(x => x.Ip.StartsWith("239.".ToString())).ToList();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    tempcomp.Remove(item);
                }
            }

            return new ObservableCollection<ComputerModel>(tempcomp);
        }
    }
}
