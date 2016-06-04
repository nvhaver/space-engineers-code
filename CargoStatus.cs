using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using VRage;

public class Testing
{
    private static readonly string prefixProduction = "ProductionMat";
    private static readonly string prefixRaw = "RawMat";
    private static readonly string prefixProductionRef = "ProductionRef";

    public class Item
    {
        private readonly string name;
        private readonly string abbreviation;
        private readonly string lightCode;
        private readonly VRage.MyFixedPoint maxCapacity;

        public Item(string name, string abbreviation, string lightCode, VRage.MyFixedPoint maxCapacity)
        {
            this.name = name;
            this.abbreviation = abbreviation;
            this.lightCode = lightCode;
            this.maxCapacity = maxCapacity;
        }

        public string Name { get { return name; } }
        public string Abbreviation { get { return abbreviation; } }
        public string LightCode { get { return lightCode; } }
        public VRage.MyFixedPoint MaxCapacity { get { return maxCapacity; } }
    }

    private Item[] ores = {
        new Item ("Iron", "Fe","Fe O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Silicon","Si", "Si O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Nickel","Ni","Ni O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Cobalt","Co","Co O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Magnesium","Mg","Mg O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Uranium","U","U O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Gold","Au","Au O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Silver","Ag","Ag O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Platinum","Pt","Pt O", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Stone","St","St O", VRage.MyFixedPoint.DeserializeStringSafe("1000000"))
    };

    private Item[] ingots = {
        new Item ("Iron", "Fe","Fe I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Silicon","Si", "Si I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Nickel","Ni","Ni I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Cobalt","Co","Co I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Magnesium","Mg","Mg I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Uranium","U","U I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Gold","Au","Au I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Silver","Ag","Ag I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Platinum","Pt","Pt I", VRage.MyFixedPoint.DeserializeStringSafe("1000000")),
        new Item ("Stone","St","St I", VRage.MyFixedPoint.DeserializeStringSafe("1000000"))
    };

    public void Main(string argument) {
        IMyGridTerminalSystem GridTerminalSystem = null;

        IMyTextPanel oreDisplay = GridTerminalSystem.GetBlockWithName("LCD Ore Status") as IMyTextPanel;
        List<IMyTerminalBlock> containers = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> refineries = new List<IMyTerminalBlock>();

        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(containers, (x => x.CustomName.ToLower().Contains(prefixRaw.ToLower())));
        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineries, (x => x.CustomName.ToLower().Contains(prefixProductionRef.ToLower())));

        List<IMyTerminalBlock> rawCargos = new List<IMyTerminalBlock>();
        rawCargos.AddList(containers);
        rawCargos.AddList(refineries);

        ItemStatus(oreDisplay, rawCargos, ores, GridTerminalSystem, "Ores Status:");

        IMyTextPanel ingotDisplay = GridTerminalSystem.GetBlockWithName("LCD Ingot Status") as IMyTextPanel;
        List<IMyTerminalBlock> ingotCargos = new List<IMyTerminalBlock>();

        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(ingotCargos, (x => x.CustomName.ToLower().Contains(prefixProduction.ToLower())));

        ItemStatus(ingotDisplay, ingotCargos, ingots, GridTerminalSystem, "Ingot Status:");
    }

    private void ItemStatus(IMyTextPanel itemDisplay, List<IMyTerminalBlock> itemCargos, Item[] items, IMyGridTerminalSystem GridTerminalSystem, string header)
    {
        clearDisplay(itemDisplay);
        writeToDisplay(itemDisplay, header + "\n======================================\n", true);

        Dictionary<string, VRage.MyFixedPoint> itemCount = new Dictionary<string, VRage.MyFixedPoint>();
        foreach (Item item in items)
        {
            itemCount[item.Name] = VRage.MyFixedPoint.DeserializeStringSafe("0");
        }   

        foreach (IMyTerminalBlock itemCargo in itemCargos)
        {
            IMyInventoryOwner inventoryOwner = itemCargo as IMyInventoryOwner;
            IMyInventory inventory = inventoryOwner.GetInventory(0);
            foreach (IMyInventoryItem item in inventory.GetItems())
            {
                itemCount[item.Content.SubtypeName] += item.Amount;
            }
        }

        foreach (Item item in items)
        {
            writeToDisplay(itemDisplay, formatOreState(item, itemCount[item.Name]) + "\n", true);

            List<IMyTerminalBlock> lights = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(lights, (x => x.CustomName.ToLower().EndsWith("[" + item.LightCode.ToLower() + "]")));

            switchLights(item, itemCount[item.Name], lights);
        }
    }

    private string formatOreState(Item item, VRage.MyFixedPoint count)
    {
        double percent = (double) count / (double) item.MaxCapacity;
        StringBuilder b = new StringBuilder();
        b.AppendFormat("{0}: {1:###,###,##0.0} kg / {2:###,###,##0.0} kg | {3:##0.00%}", item.Abbreviation, (double)count, (double)item.MaxCapacity, percent);
        b.AppendLine();
        int fill = percent >= 1 ? 100 : (int)(percent * 100);
        b.AppendFormat("[{0}{1}]", new String('|', fill), new String('\'', 100 - fill));

        return b.ToString();
    }

    private void switchLights(Item item, VRage.MyFixedPoint count, List<IMyTerminalBlock> lights)
    {
        Color color = Color.Blue;
        if (count >= item.MaxCapacity)
        {
            color = Color.Green;
        }
        else if (count > 0)
        {
            color = Color.Orange;
        }
        else
        {
            color = Color.Red;
        }

        foreach (IMyTerminalBlock block in lights) {
            IMyLightingBlock light = block as IMyLightingBlock;
            light.SetValue("Color", color);
        }
    }

    //Write string on an LCD 
    private void writeToDisplay(IMyTextPanel textPanel, string text, bool append = false)
    {
        textPanel.WritePublicText(text, append);
        textPanel.ShowPublicTextOnScreen();
        textPanel.UpdateVisual();
    }

    private void clearDisplay(IMyTextPanel textPanel) {
        textPanel.WritePublicText("");
    }
}
