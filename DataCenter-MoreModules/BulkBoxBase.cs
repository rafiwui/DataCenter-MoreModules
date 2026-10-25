namespace DataCenter_MoreModules;

public class BulkBoxBase
{
    public class BulkConfig
    {
        public int OriginalID;
        public string BulkName;
        public int Quantity;

        // The shop price is for a vanilla box of 5 modules, a bulk box costs the same per module
        public const int ModulesPerShopBox = 5;

        public int PriceFor(int shopBoxPrice)
        {
            return shopBoxPrice * Quantity / ModulesPerShopBox;
        }
    }

    public static List<BulkConfig> BoxConfigs = new()
    {
        new BulkConfig { OriginalID = 0, BulkName = "32x SFP+ Module RJ45 10Gbps", Quantity = 32 },
        new BulkConfig { OriginalID = 1, BulkName = "32x SFP+ Module Fiber 10Gbps", Quantity = 32 },
        new BulkConfig { OriginalID = 2, BulkName = "32x SFP28 Module Fiber 25Gbps", Quantity = 32 },
        new BulkConfig { OriginalID = 3, BulkName = "32x QSFP+ Module Fiber 40Gbps", Quantity = 32 },
        new BulkConfig { OriginalID = 0, BulkName = "64x SFP+ Module RJ45 10Gbps", Quantity = 64 },
        new BulkConfig { OriginalID = 1, BulkName = "64x SFP+ Module Fiber 10Gbps", Quantity = 64  },
        new BulkConfig { OriginalID = 2, BulkName = "64x SFP28 Module Fiber 25Gbps", Quantity = 64  },
        new BulkConfig { OriginalID = 3, BulkName = "64x QSFP+ Module Fiber 40Gbps", Quantity = 64  }
    };
}
