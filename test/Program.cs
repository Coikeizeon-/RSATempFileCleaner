using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        static string[] whiteList = new string[] {
            "c2319c42033a5ca7f44e731bfd3fa2b5",
        "d6d986f09a1ee04e24c949879fdb506c",
        "76944fb33636aeddb9590521c2e8815a",
        "bedbf0b4da5f8061b6444baedf4c00b1",
        "6de9cb26d2b98c01ec4e9e8b34824aa2",
        "7a436fe806e483969f48a894af2fe9a1",
        "f686aace6942fb7f7ceb231212eef4a4"
        };
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DeleteDirectory("C:\\ProgramData\\Microsoft\\Crypto\\RSA\\MachineKeys");

            sw.Stop();
            TimeSpan timespan = sw.Elapsed; //  获取当前实例测量得出的总时间
            double hours = timespan.TotalHours; // 总小时
            double minutes = timespan.TotalMinutes;  // 总分钟
            double seconds = timespan.TotalSeconds;  //  总秒数
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数

            Console.WriteLine("总小时:" + hours);
            Console.WriteLine("总分钟:" + minutes);
            Console.WriteLine("总秒数:" + seconds);
            Console.WriteLine("总毫秒数:" + milliseconds);

            Console.ReadKey();
        }

        /// <summary>
        /// 排除指定Key的文件（系统组件相关)
        /// //https://forums.iis.net/p/1224708/2099385.aspx?Re+C+ProgramData+Microsoft+Crypto+RSA+MachineKeys+is+filling+my+disk+space
        ///                 - Microsoft Internet Information Server -> c2319c42033a5ca7f44e731bfd3fa2b5...
        ///- NetFrameworkConfigurationKey          -> d6d986f09a1ee04e24c949879fdb506c...
        /// - iisWasKey                             -> 76944fb33636aeddb9590521c2e8815a...
        /// - WMSvc Certificate Key Container       -> bedbf0b4da5f8061b6444baedf4c00b1...
        /// - iisConfigurationKey                   -> 6de9cb26d2b98c01ec4e9e8b34824aa2...
        /// - MS IIS DCOM Server                    -> 7a436fe806e483969f48a894af2fe9a1...
        ///  - TSSecKeySet1                          -> f686aace6942fb7f7ceb231212eef4a4...
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void DeleteDirectory(string directoryPath)
        {
        
            int index = 0;
            List<string> files = Directory.GetFiles(directoryPath).ToList();
            //删除文件
            foreach (string fileName in files)
            {
                index++;
                Console.WriteLine("删除第 " + index + " 个文件");
                try
                {
                    foreach (string whiteListFileName in whiteList)
                    {
                        if (!fileName.Contains(whiteListFileName))
                        {
                            File.Delete(fileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine("文件数：" + files.Count);

           
        }

        /// <summary>
        /// 排除创建者是IIS的文件
        /// https://stackoverflow.com/questions/34527477/clean-my-machinekeys-folder-by-removing-multiple-rsa-files-without-touching-iis
        /// </summary>
        public static void deleteFileNotCreateByIIS()
        {
            var userNameToDeleteBy = "IIS_App_Pool_User_Goes_here!";

            var allFiles = (new DirectoryInfo(Directory.GetCurrentDirectory()).EnumerateFiles());

            var i = 0;

            foreach (var file in allFiles)
            {

                var fname = file.FullName;
                string user = System.IO.File
                    .GetAccessControl(fname)
                    .GetOwner(typeof(System.Security.Principal.NTAccount))
                    .ToString();

                if (user.Contains(userNameToDeleteBy))
                {
                    File.Delete(fname);
                    i++;
                }

                //output only every 1k files, as this is the slowest operation
                if (i % 1000 == 0) Console.WriteLine("Deleted: " + i);
            }
        }

        /// <summary>
        /// 通过证书来删除
        /// https://stackoverflow.com/questions/22618568/prevent-file-creation-when-x509certificate2-is-created?noredirect=1
        /// </summary>
        public static void deleteFileByCert()
        {
            Byte[] bytes = File.ReadAllBytes(@"D:\tmp\111111111111.p12");
            X509Certificate2 x509 = new X509Certificate2(bytes, "qwerty", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            var privateKey = x509.PrivateKey as RSACryptoServiceProvider;
            string uniqueKeyContainerName = privateKey.CspKeyContainerInfo.UniqueKeyContainerName;
            x509.Reset();

            File.Delete(string.Format(@"C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys\{0}", uniqueKeyContainerName));
        }
    }
}