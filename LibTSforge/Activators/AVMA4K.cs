namespace LibTSforge.Activators
{
    using System;
    using LibTSforge.PhysicalStore;
    using LibTSforge.SPP;

    public static class AVMA4k
    {
        public static void Activate(PSVersion version, bool production, Guid actId)
        {
            if (version != PSVersion.WinModern)
            {
                throw new NotSupportedException("AVMA licenses are not available for this product.");
            }

            Guid appId;
            if (actId == Guid.Empty)
            {
                appId = SLApi.WINDOWS_APP_ID;
                actId = SLApi.GetDefaultActivationID(appId, false);

                if (actId == Guid.Empty)
                {
                    throw new NotSupportedException("No applicable activation IDs found.");
                }
            }
            else
            {
                appId = SLApi.GetAppId(actId);
            }

            if (SLApi.GetPKeyChannel(SLApi.GetInstalledPkeyID(actId)) != "VT:IA")
            {
                throw new NotSupportedException("Non-VT:IA product key installed.");
            }

            Utils.KillSPP();

            Logger.WriteLine("Writing TrustedStore data...");

            using (IPhysicalStore store = Utils.GetStore(version, production))
            {
                string key = string.Format("SPPSVC\\{0}\\{1}", appId, actId);

                ulong unknown = 0;
                ulong time1;
                ulong crcBindTime = (ulong)DateTime.UtcNow.ToFileTime();
                ulong timerTime;

                ulong expiry = Constants.TimerMax;

                long creationTime = BitConverter.ToInt64(store.GetBlock("__##USERSEP##\\$$_RESERVED_$$\\NAMESPACE__", "__##USERSEP-RESERVED##__$$GLOBAL-CREATION-TIME$$").Data, 0);
                long tickCount = BitConverter.ToInt64(store.GetBlock("__##USERSEP##\\$$_RESERVED_$$\\NAMESPACE__", "__##USERSEP-RESERVED##__$$GLOBAL-TICKCOUNT-UPTIME$$").Data, 0);
                long deltaTime = BitConverter.ToInt64(store.GetBlock(key, "__##USERSEP-RESERVED##__$$UP-TIME-DELTA$$").Data, 0);

                time1 = (ulong)(creationTime + tickCount + deltaTime);
                timerTime = crcBindTime / 10000;
                expiry /= 10000;

                VariableBag avmaBinding = new VariableBag();

                avmaBinding.Blocks.AddRange(new CRCBlock[]
                {
                    new CRCBlock
                    {
                        DataType = CRCBlockType.BINARY,
                        Key = new byte[] { },
                        Value = BitConverter.GetBytes(crcBindTime),
                    },
                    new CRCBlock
                    {
                        DataType = CRCBlockType.STRING,
                        Key = new byte[] { },
                        ValueAsStr = "AVMA4K",
                    },
                    new CRCBlock
                    {
                        DataType = CRCBlockType.STRING,
                        Key = new byte[] { },
                        ValueAsStr = "00491-50000-00001-AA666",
                    }
                });

                byte[] avmaBindingData = avmaBinding.Serialize();

                Timer avmaTimer = new Timer
                {
                    Unknown = unknown,
                    Time1 = time1,
                    Time2 = timerTime,
                    Expiry = expiry
                };

                string storeVal = string.Format("msft:spp/ia/bind/1.0/store/{0}/{1}", appId, actId);
                string timerVal = string.Format("msft:spp/ia/bind/1.0/timer/{0}/{1}", appId, actId);

                store.DeleteBlock(key, storeVal);
                store.DeleteBlock(key, timerVal);

                store.AddBlocks(new PSBlock[]
                {
                    new PSBlock
                    {
                        Type = BlockType.NAMED,
                        Flags = 0x400,
                        KeyAsStr = key,
                        ValueAsStr = storeVal,
                        Data = avmaBindingData,
                    },
                    new PSBlock
                    {
                        Type = BlockType.TIMER,
                        Flags = 0x4,
                        KeyAsStr = key,
                        ValueAsStr = timerVal,
                        Data = avmaTimer.CastToArray()
                    }
                });
            }

            SLApi.RefreshLicenseStatus();
            SLApi.FireStateChangedEvent(appId);
            Logger.WriteLine("Activated using AVMA4k successfully.");
        }
    }
}
