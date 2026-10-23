namespace DataCenter_MoreModules;

public class BulkBoxBase
{
    public class BulkConfig
    {
        public int OriginalID;
        public string BulkName;
        public int BulkPrice;
        public string PrefabKeyword;
        public int Quantity;
    }

    public static List<BulkConfig> BoxConfigs = new List<BulkConfig>()
    {
        new BulkConfig { OriginalID = 0, BulkPrice = 1600, BulkName = "32x SFP+ Module RJ45 10Gbps", PrefabKeyword = "rj45", Quantity = 32 },
        new BulkConfig { OriginalID = 1, BulkPrice = 2240, BulkName = "32x SFP+ Module Fiber 10Gbps", PrefabKeyword = "fibre_10gb", Quantity = 32 },
        new BulkConfig { OriginalID = 2, BulkPrice = 5760, BulkName = "32x SFP28 Module Fiber 25Gbps", PrefabKeyword = "25gb", Quantity = 32 },
        new BulkConfig { OriginalID = 3, BulkPrice = 9600, BulkName = "32x QSFP+ Module Fiber 40Gbps", PrefabKeyword = "40gb", Quantity = 32 },
        new BulkConfig { OriginalID = 0, BulkPrice = 3200, BulkName = "64x SFP+ Module RJ45 10Gbps", PrefabKeyword = "rj45", Quantity = 64 },
        new BulkConfig { OriginalID = 1, BulkPrice = 4480, BulkName = "64x SFP+ Module Fiber 10Gbps", PrefabKeyword = "fibre_10gb", Quantity = 64  },
        new BulkConfig { OriginalID = 2, BulkPrice = 11520, BulkName = "64x SFP28 Module Fiber 25Gbps", PrefabKeyword = "25gb", Quantity = 64  },
        new BulkConfig { OriginalID = 3, BulkPrice = 19200, BulkName = "64x QSFP+ Module Fiber 40Gbps", PrefabKeyword = "40gb", Quantity = 64  }
    };
}