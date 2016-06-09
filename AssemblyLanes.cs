using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;

namespace SpaceEngineersLibrary
{
    class Program
    {
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

        private static readonly Item[] components = {
            new Item ("SteelPlate", "SP","SP_L", VRage.MyFixedPoint.DeserializeStringSafe("5000")),
            new Item ("Construction","CC", "CC_L", VRage.MyFixedPoint.DeserializeStringSafe("5000")),
            new Item ("LargeTube","LST", "LST_L", VRage.MyFixedPoint.DeserializeStringSafe("5000")),
            new Item ("InteriorPlate","IP", "IP_L", VRage.MyFixedPoint.DeserializeStringSafe("5000")),
            new Item ("Motor","M", "M_L", VRage.MyFixedPoint.DeserializeStringSafe("5000")),
            new Item ("SmallTube","SST","SST_L", VRage.MyFixedPoint.DeserializeStringSafe("5000"))
        };

        public Program()
        {
            IMyGridTerminalSystem GridTerminalSystem = null;
            display = GridTerminalSystem.GetBlockWithName("LCD Production Status") as IMyTextPanel;
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(componentContainers, (x => x.CustomName.Contains("Large Container all materials ")));

            productionLanes = new List<List<IMyTerminalBlock>>();

            foreach (Item component in components)
            {
                List<IMyTerminalBlock> currentLane = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyAssembler>(currentLane, (x => x.CustomName.Contains("ProdLane_" + component.Abbreviation)));
                productionLanes.Add(currentLane);
            }
        }

        private IMyTextPanel display;
        private List<IMyTerminalBlock> componentContainers;
        private List<List<IMyTerminalBlock>> productionLanes;

        public void Main(string argument)
        {
            clearDisplay(display);

            for (int i = 0; i<components.Length; i++)
            { // Per component
                var component = components[i];
                List<IMyInventoryItem> items = new List<IMyInventoryItem>();

                foreach (IMyTerminalBlock container in componentContainers)
                { // Find all existing components of this type
                    IMyInventory inventory = container.GetInventory(0);
                    items.AddList(inventory.GetItems().Where(x => x.Content.SubtypeName == component.Name).ToList());
                }

                var itemCount = VRage.MyFixedPoint.DeserializeStringSafe("0");
                foreach(IMyInventoryItem item in items)
                { // Count them
                    itemCount += item.Amount;
                }

                if (itemCount >= component.MaxCapacity)
                { // Check the count
                    writeToDisplay(display, component.Name + " capacity reached, shutting down lane...");
                    //disableLane(productionLanes[i]);
                }
                else
                {
                    writeToDisplay(display, component.Name + " deficiency, spinning up lane...");
                    //enableLane(productionLanes[i]);
                }
            }
        }

        // Disables all terminalblocks in the list
        public void disableBlocks(List<IMyTerminalBlock> blocks)
        {
            foreach(IMyTerminalBlock block in blocks)
            {
                if((block as IMyFunctionalBlock).Enabled)
                    block.GetActionWithName("OnOff_Off").Apply(block);
            }
        }

        // Enables all terminalblocks in the list
        public void enableBlocks(List<IMyTerminalBlock> blocks)
        {
            foreach (IMyTerminalBlock block in blocks)
            {
                if (!(block as IMyFunctionalBlock).Enabled)
                    block.GetActionWithName("OnOff_On").Apply(block);
            }
        }

        // Write string on an LCD 
        private void writeToDisplay(IMyTextPanel textPanel, string text, bool append = false)
        {
            textPanel.WritePublicText(text, append);
            textPanel.ShowPublicTextOnScreen();
            textPanel.UpdateVisual();
        }

        private void clearDisplay(IMyTextPanel textPanel)
        {
            textPanel.WritePublicText("");
        }
    }
}
