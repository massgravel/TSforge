namespace LibTSforge.Modifiers
{
    using System;
    using System.IO;
    using LibTSforge.PhysicalStore;
    using LibTSforge.SPP;

    public static class KMSHostCharge
    {
        public static void Charge(PSVersion version, Guid actId, bool production)
        {
            if (actId == Guid.Empty)
            {
                actId = SLApi.GetDefaultActivationID(SLApi.WINDOWS_APP_ID, true);

                if (actId == Guid.Empty)
                {
                    throw new NotSupportedException("No applicable activation IDs found.");
                }
            }

            if (SLApi.GetPKeyChannel(SLApi.GetInstalledPkeyID(actId)) != "Volume:CSVLK")
            {
                throw new NotSupportedException("Non-Volume:CSVLK product key installed.");
            }

            Guid appId = SLApi.GetAppId(actId);
            int totalClients = 50;
            int currClients = 25;
            byte[] hwidBlock = Constants.UniversalHWIDBlock;
            string key = string.Format("SPPSVC\\{0}", appId);
            long ldapTimestamp = DateTime.Now.ToFileTime();

            BinaryWriter writer = new BinaryWriter(new MemoryStream());

            for (int i = 0; i < currClients; i++)
            {
                writer.Write(ldapTimestamp - (10 * (i + 1)));
                writer.Write(Guid.NewGuid().ToByteArray());
            }

            byte[] cmidGuids = writer.GetBytes();

            writer = new BinaryWriter(new MemoryStream());

            writer.Write(new byte[40]);

            writer.Seek(4, SeekOrigin.Begin);
            writer.Write((byte)currClients);

            writer.Seek(24, SeekOrigin.Begin);
            writer.Write((byte)currClients);
            byte[] reqCounts = writer.GetBytes();

            Utils.KillSPP();

            Logger.WriteLine("Writing TrustedStore data...");

            using (IPhysicalStore store = Utils.GetStore(version, production))
            {
                VariableBag kmsCountData = new VariableBag();
                kmsCountData.Blocks.AddRange(new CRCBlock[]
                {
                    new CRCBlock
                    {
                        DataType = CRCBlockType.BINARY,
                        KeyAsStr = "SppBindingLicenseData",
                        Value = hwidBlock
                    },
                    new CRCBlock
                    {
                        DataType = CRCBlockType.UINT,
                        Key = new byte[] { },
                        ValueAsInt = (uint)totalClients
                    },
                    new CRCBlock
                    {
                        DataType = CRCBlockType.UINT,
                        Key = new byte[] { },
                        ValueAsInt = 1051200000
                    },
                    new CRCBlock
                    {
                        DataType = CRCBlockType.UINT,
                        Key = new byte[] { },
                        ValueAsInt = (uint)currClients
                    },
                    new CRCBlock
                    {
                        DataType = CRCBlockType.BINARY,
                        Key = new byte[] { },
                        Value = cmidGuids
                    },
                    new CRCBlock
                    {
                        DataType = CRCBlockType.BINARY,
                        Key = new byte[] { },
                        Value = reqCounts
                    }
                });

                byte[] kmsChargeData = kmsCountData.Serialize();
                string countVal = string.Format("msft:spp/kms/host/2.0/store/counters/{0}", appId);

                store.DeleteBlock(key, countVal);
                store.AddBlock(new PSBlock
                {
                    Type = BlockType.NAMED,
                    Flags = (version == PSVersion.WinModern) ? (uint)0x400 : 0,
                    KeyAsStr = key,
                    ValueAsStr = countVal,
                    Data = kmsChargeData
                });

                Logger.WriteLine(string.Format("Set charge count to {0} successfully.", currClients));
            }
        }
    }
}
