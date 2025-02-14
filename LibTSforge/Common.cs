namespace LibTSforge
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.ServiceProcess;
    using System.Text;
    using LibTSforge.Crypto;
    using LibTSforge.PhysicalStore;
    using LibTSforge.SPP;
    using LibTSforge.TokenStore;

    public enum PSVersion
    {
        Vista,
        Win7,
        Win8Early,
        Win8,
        WinBlue,
        WinModern
    }

    public static class Constants
    {
        public static readonly byte[] UniversalHWIDBlock =
        {
            0x26, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1c, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x0c, 0x01, 0x00
        };

        public static readonly byte[] KMSv4Response =
        {
            0x00, 0x00, 0x04, 0x00, 0x62, 0x00, 0x00, 0x00, 0x30, 0x00, 0x35, 0x00, 0x34, 0x00, 0x32, 0x00,
            0x36, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x30, 0x00, 0x32, 0x00, 0x30, 0x00, 0x36, 0x00, 0x2D, 0x00,
            0x31, 0x00, 0x36, 0x00, 0x31, 0x00, 0x2D, 0x00, 0x36, 0x00, 0x35, 0x00, 0x35, 0x00, 0x35, 0x00,
            0x30, 0x00, 0x36, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x31, 0x00, 0x30, 0x00,
            0x33, 0x00, 0x33, 0x00, 0x2D, 0x00, 0x39, 0x00, 0x32, 0x00, 0x30, 0x00, 0x30, 0x00, 0x2E, 0x00,
            0x30, 0x00, 0x30, 0x00, 0x30, 0x00, 0x30, 0x00, 0x2D, 0x00, 0x30, 0x00, 0x36, 0x00, 0x35, 0x00,
            0x32, 0x00, 0x30, 0x00, 0x31, 0x00, 0x33, 0x00, 0x00, 0x00, 0xDE, 0x19, 0x02, 0xCF, 0x1F, 0x35,
            0x97, 0x4E, 0x8A, 0x8F, 0xB8, 0x07, 0xB1, 0x92, 0xB5, 0xB5, 0x97, 0x42, 0xEC, 0x3A, 0x76, 0x84,
            0xD5, 0x01, 0x32, 0x00, 0x00, 0x00, 0x78, 0x00, 0x00, 0x00, 0x60, 0x27, 0x00, 0x00, 0xC4, 0x1E,
            0xAA, 0x8B, 0xDD, 0x0C, 0xAB, 0x55, 0x6A, 0xCE, 0xAF, 0xAC, 0x7F, 0x5F, 0xBD, 0xE9
        };

        public static readonly byte[] KMSv5Response =
        {
            0x00, 0x00, 0x05, 0x00, 0xBE, 0x96, 0xF9, 0x04, 0x54, 0x17, 0x3F, 0xAF, 0xE3, 0x08, 0x50, 0xEB,
            0x22, 0xBA, 0x53, 0xBF, 0xF2, 0x6A, 0x7B, 0xC9, 0x05, 0x1D, 0xB5, 0x19, 0xDF, 0x98, 0xE2, 0x71,
            0x4D, 0x00, 0x61, 0xE9, 0x9D, 0x03, 0xFB, 0x31, 0xF9, 0x1F, 0x2E, 0x60, 0x59, 0xC7, 0x73, 0xC8,
            0xE8, 0xB6, 0xE1, 0x2B, 0x39, 0xC6, 0x35, 0x0E, 0x68, 0x7A, 0xAA, 0x4F, 0x28, 0x23, 0x12, 0x18,
            0xE3, 0xAA, 0x84, 0x81, 0x6E, 0x82, 0xF0, 0x3F, 0xD9, 0x69, 0xA9, 0xDF, 0xBA, 0x5F, 0xCA, 0x32,
            0x54, 0xB2, 0x52, 0x3B, 0x3E, 0xD1, 0x5C, 0x65, 0xBC, 0x3E, 0x59, 0x0D, 0x15, 0x9F, 0x37, 0xEC,
            0x30, 0x9C, 0xCC, 0x1B, 0x39, 0x0D, 0x21, 0x32, 0x29, 0xA2, 0xDD, 0xC7, 0xC1, 0x69, 0xF2, 0x72,
            0x3F, 0x00, 0x98, 0x1E, 0xF8, 0x9A, 0x79, 0x44, 0x5D, 0x25, 0x80, 0x7B, 0xF5, 0xE1, 0x7C, 0x68,
            0x25, 0xAA, 0x0D, 0x67, 0x98, 0xE5, 0x59, 0x9B, 0x04, 0xC1, 0x23, 0x33, 0x48, 0xFB, 0x28, 0xD0,
            0x76, 0xDF, 0x01, 0x56, 0xE7, 0xEC, 0xBF, 0x1A, 0xA2, 0x22, 0x28, 0xCA, 0xB1, 0xB4, 0x4C, 0x30,
            0x14, 0x6F, 0xD2, 0x2E, 0x01, 0x2A, 0x04, 0xE3, 0xBD, 0xA7, 0x41, 0x2F, 0xC9, 0xEF, 0x53, 0xC0,
            0x70, 0x48, 0xF1, 0xB2, 0xB6, 0xEA, 0xE7, 0x0F, 0x7A, 0x15, 0xD1, 0xA6, 0xFE, 0x23, 0xC8, 0xF3,
            0xE1, 0x02, 0x9E, 0xA0, 0x4E, 0xBD, 0xF5, 0xEA, 0x53, 0x74, 0x8E, 0x74, 0xA1, 0xA1, 0xBD, 0xBE,
            0x66, 0xC4, 0x73, 0x8F, 0x24, 0xA7, 0x2A, 0x2F, 0xE3, 0xD9, 0xF4, 0x28, 0xD9, 0xF8, 0xA3, 0x93,
            0x03, 0x9E, 0x29, 0xAB
        };

        public static readonly byte[] KMSv6Response =
        {
            0x00, 0x00, 0x06, 0x00, 0x54, 0xD3, 0x40, 0x08, 0xF3, 0xCD, 0x03, 0xEF, 0xC8, 0x15, 0x87, 0x9E,
            0xCA, 0x2E, 0x85, 0xFB, 0xE6, 0xF6, 0x73, 0x66, 0xFB, 0xDA, 0xBB, 0x7B, 0xB1, 0xBC, 0xD6, 0xF9,
            0x5C, 0x41, 0xA0, 0xFE, 0xE1, 0x74, 0xC4, 0xBB, 0x91, 0xE5, 0xDE, 0x6D, 0x3A, 0x11, 0xD5, 0xFC,
            0x68, 0xC0, 0x7B, 0x82, 0xB2, 0x24, 0xD1, 0x85, 0xBA, 0x45, 0xBF, 0xF1, 0x26, 0xFA, 0xA5, 0xC6,
            0x61, 0x70, 0x69, 0x69, 0x6E, 0x0F, 0x0B, 0x60, 0xB7, 0x3D, 0xE8, 0xF1, 0x47, 0x0B, 0x65, 0xFD,
            0xA7, 0x30, 0x1E, 0xF6, 0xA4, 0xD0, 0x79, 0xC4, 0x58, 0x8D, 0x81, 0xFD, 0xA7, 0xE7, 0x53, 0xF1,
            0x67, 0x78, 0xF0, 0x0F, 0x60, 0x8F, 0xC8, 0x16, 0x35, 0x22, 0x94, 0x48, 0xCB, 0x0F, 0x8E, 0xB2,
            0x1D, 0xF7, 0x3E, 0x28, 0x42, 0x55, 0x6B, 0x07, 0xE3, 0xE8, 0x51, 0xD5, 0xFA, 0x22, 0x0C, 0x86,
            0x65, 0x0D, 0x3F, 0xDD, 0x8D, 0x9B, 0x1B, 0xC9, 0xD3, 0xB8, 0x3A, 0xEC, 0xF1, 0x11, 0x19, 0x25,
            0xF7, 0x84, 0x4A, 0x4C, 0x0A, 0xB5, 0x31, 0x94, 0x37, 0x76, 0xCE, 0xE7, 0xAB, 0xA9, 0x69, 0xDF,
            0xA4, 0xC9, 0x22, 0x6C, 0x23, 0xFF, 0x6B, 0xFC, 0xDA, 0x78, 0xD8, 0xC4, 0x8F, 0x74, 0xBB, 0x26,
            0x05, 0x00, 0x98, 0x9B, 0xE5, 0xE2, 0xAD, 0x0D, 0x57, 0x95, 0x80, 0x66, 0x8E, 0x43, 0x74, 0x87,
            0x93, 0x1F, 0xF4, 0xB2, 0x2C, 0x20, 0x5F, 0xD8, 0x9C, 0x4C, 0x56, 0xB3, 0x57, 0x44, 0x62, 0x68,
            0x8D, 0xAA, 0x40, 0x11, 0x9D, 0x84, 0x62, 0x0E, 0x43, 0x8A, 0x1D, 0xF0, 0x1C, 0x49, 0xD8, 0x56,
            0xEF, 0x4C, 0xD3, 0x64, 0xBA, 0x0D, 0xEF, 0x87, 0xB5, 0x2C, 0x88, 0xF3, 0x18, 0xFF, 0x3A, 0x8C,
            0xF5, 0xA6, 0x78, 0x5C, 0x62, 0xE3, 0x9E, 0x4C, 0xB6, 0x31, 0x2D, 0x06, 0x80, 0x92, 0xBC, 0x2E,
            0x92, 0xA6, 0x56, 0x96
        };

        // 2^31 - 1 minutes
        public static ulong TimerMax = (ulong)TimeSpan.FromMinutes(2147483647).Ticks;

        public static readonly string ZeroCID = new string('0', 48);
    }

    public static class BinaryReaderExt
    {
        public static void Align(this BinaryReader reader, int to)
        {
            int pos = (int)reader.BaseStream.Position;
            reader.BaseStream.Seek(-pos & (to - 1), SeekOrigin.Current);
        }

        public static string ReadNullTerminatedString(this BinaryReader reader, int maxLen)
        {
            return Encoding.Unicode.GetString(reader.ReadBytes(maxLen)).Split(new char[] { '\0' }, 2)[0];
        }
    }

    public static class BinaryWriterExt
    {
        public static void Align(this BinaryWriter writer, int to)
        {
            int pos = (int)writer.BaseStream.Position;
            writer.WritePadding(-pos & (to - 1));
        }

        public static void WritePadding(this BinaryWriter writer, int len)
        {
            writer.Write(Enumerable.Repeat((byte)0, len).ToArray());
        }

        public static void WriteFixedString(this BinaryWriter writer, string str, int bLen)
        {
            writer.Write(Encoding.ASCII.GetBytes(str));
            writer.WritePadding(bLen - str.Length);
        }

        public static void WriteFixedString16(this BinaryWriter writer, string str, int bLen)
        {
            byte[] bstr = Utils.EncodeString(str);
            writer.Write(bstr);
            writer.WritePadding(bLen - bstr.Length);
        }

        public static byte[] GetBytes(this BinaryWriter writer)
        {
            return ((MemoryStream)writer.BaseStream).ToArray();
        }
    }

    public static class ByteArrayExt
    {
        public static byte[] CastToArray<T>(this T data) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] result = new byte[size];
            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(data, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }
            return result;
        }

        public static T CastToStruct<T>(this byte[] data) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
    }

    public static class FileStreamExt
    {
        public static byte[] ReadAllBytes(this FileStream fs)
        {
            BinaryReader br = new BinaryReader(fs);
            return br.ReadBytes((int)fs.Length);
        }

        public static void WriteAllBytes(this FileStream fs, byte[] data)
        {
            fs.Seek(0, SeekOrigin.Begin);
            fs.SetLength(data.Length);
            fs.Write(data, 0, data.Length);
        }
    }

    public static class Utils
    {
        public static string DecodeString(byte[] data)
        {
            return Encoding.Unicode.GetString(data).Trim('\0');
        }

        public static byte[] EncodeString(string str)
        {
            return Encoding.Unicode.GetBytes(str + '\0');
        }

        [DllImport("kernel32.dll")]
        public static extern uint GetSystemDefaultLCID();

        public static uint CRC32(byte[] data)
        {
            const uint polynomial = 0x04C11DB7;
            uint crc = 0xffffffff;

            foreach (byte b in data)
            {
                crc ^= (uint)b << 24;
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x80000000) != 0)
                    {
                        crc = (crc << 1) ^ polynomial;
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return ~crc;
        }

        public static void KillSPP()
        {
            ServiceController sc;

            try
            {
                sc = new ServiceController("sppsvc");

                if (sc.Status == ServiceControllerStatus.Stopped)
                    return;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Unable to access sppsvc: " + ex.Message);
            }

            Logger.WriteLine("Stopping sppsvc...");

            bool stopped = false;

            for (int i = 0; stopped == false && i < 60; i++)
            {
                try
                {
                    if (sc.Status != ServiceControllerStatus.StopPending)
                        sc.Stop();

                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(500));
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    continue;
                }
                catch (InvalidOperationException)
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                stopped = true;
            }

            if (!stopped)
                throw new System.TimeoutException("Failed to stop sppsvc");

            Logger.WriteLine("sppsvc stopped successfully.");
        }

        public static string GetPSPath(PSVersion version)
        {
            switch (version)
            {
                case PSVersion.Win7:
                    return Directory.GetFiles(
                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "7B296FB0-376B-497e-B012-9C450E1B7327-*.C7483456-A289-439d-8115-601632D005A0")
                    .FirstOrDefault() ?? "";
                case PSVersion.Win8Early:
                case PSVersion.WinBlue:
                case PSVersion.Win8:
                case PSVersion.WinModern:
                    return Path.Combine(
                        Environment.ExpandEnvironmentVariables(
                            (string)Registry.GetValue(
                                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform",
                                "TokenStore",
                                string.Empty
                                )
                            ),
                            "data.dat"
                        );
                default:
                    return "";
            }
        }

        public static string GetTokensPath(PSVersion version)
        {
            switch (version)
            {
                case PSVersion.Win7:
                    return Path.Combine(
                        Environment.ExpandEnvironmentVariables("%WINDIR%"),
                        @"ServiceProfiles\NetworkService\AppData\Roaming\Microsoft\SoftwareProtectionPlatform\tokens.dat"
                    );
                case PSVersion.Win8Early:
                case PSVersion.WinBlue:
                case PSVersion.Win8:
                case PSVersion.WinModern:
                    return Path.Combine(
                        Environment.ExpandEnvironmentVariables(
                            (string)Registry.GetValue(
                                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform",
                                "TokenStore",
                                string.Empty
                                )
                            ),
                            "tokens.dat"
                        );
                default:
                    return "";
            }
        }

        public static IPhysicalStore GetStore(PSVersion version, bool production)
        {
            string psPath;

            try
            {
                psPath = GetPSPath(version);
            }
            catch
            {
                throw new FileNotFoundException("Failed to get path of physical store.");
            }

            if (string.IsNullOrEmpty(psPath) || !File.Exists(psPath))
            {
                throw new FileNotFoundException(string.Format("Physical store not found at expected path {0}.", psPath));
            }

            if (version == PSVersion.Vista)
            {
                throw new NotSupportedException("Physical store editing is not supported for Windows Vista.");
            }

            return version == PSVersion.Win7 ? new PhysicalStoreWin7(psPath, production) : (IPhysicalStore)new PhysicalStoreModern(psPath, production, version);
        }

        public static ITokenStore GetTokenStore(PSVersion version)
        {
            string tokPath;

            try
            {
                tokPath = GetTokensPath(version);
            }
            catch
            {
                throw new FileNotFoundException("Failed to get path of physical store.");
            }

            if (string.IsNullOrEmpty(tokPath) || !File.Exists(tokPath))
            {
                throw new FileNotFoundException(string.Format("Token store not found at expected path {0}.", tokPath));
            }

            return new TokenStoreModern(tokPath);
        }

        public static string GetArchitecture()
        {
            string arch = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine).ToUpperInvariant();
            return arch == "AMD64" ? "X64" : arch;
        }

        public static PSVersion DetectVersion()
        {
            int build = Environment.OSVersion.Version.Build;

            if (build >= 9600) return PSVersion.WinModern;
            if (build >= 6000 && build <= 6003) return PSVersion.Vista;
            if (build >= 7600 && build <= 7602) return PSVersion.Win7;
            if (build == 9200) return PSVersion.Win8;

            throw new NotSupportedException("Unable to auto-detect version info, please specify one manually using the /ver argument.");
        }

        public static bool DetectCurrentKey()
        {
            SLApi.RefreshLicenseStatus();

            using (RegistryKey wpaKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\WPA"))
            {
                foreach (string subKey in wpaKey.GetSubKeyNames())
                {
                    if (subKey.StartsWith("8DEC0AF1") && subKey.EndsWith("-1"))
                    {
                        return subKey.Contains("P");
                    }
                }
            }

            throw new FileNotFoundException("Failed to autodetect key type, specify physical store key with /prod or /test arguments.");
        }

        public static void DumpStore(PSVersion version, bool production, string filePath, string encrFilePath)
        {
            if (encrFilePath == null)
            {
                encrFilePath = GetPSPath(version);
            }

            if (string.IsNullOrEmpty(encrFilePath) || !File.Exists(encrFilePath))
            {
                throw new FileNotFoundException("Store does not exist at expected path '" + encrFilePath + "'.");
            }

            KillSPP();

            using (FileStream fs = File.Open(encrFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                byte[] encrData = fs.ReadAllBytes();
                File.WriteAllBytes(filePath, PhysStoreCrypto.DecryptPhysicalStore(encrData, production));
            }

            Logger.WriteLine("Store dumped successfully to '" + filePath + "'.");
        }

        public static void LoadStore(PSVersion version, bool production, string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("Store file '" + filePath + "' does not exist.");
            }

            KillSPP();

            using (IPhysicalStore store = GetStore(version, production))
            {
                store.WriteRaw(File.ReadAllBytes(filePath));
            }

            Logger.WriteLine("Loaded store file succesfully.");
        }
    }

    public static class Logger
    {
        public static bool HideOutput = false;

        public static void WriteLine(string line)
        {
            if (!HideOutput) Console.WriteLine(line);
        }
    }
}
