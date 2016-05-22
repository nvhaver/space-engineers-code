private IMyTextPanel textPanelOne;
private IMyTextPanel textPanelTwo;
private IMyTextPanel textPanelThree;

private List<IMyTerminalBlock> allCargoContainers = new List<IMyTerminalBlock>();
private List<IMyTerminalBlock> allRefineries = new List<IMyTerminalBlock>();

private List<IMyTerminalBlock> productionMatList = new List<IMyTerminalBlock>();
private List<IMyTerminalBlock> rawMatList = new List<IMyTerminalBlock>();

private string prefixProduction = "ProductionMat"; 
private string prefixRaw = "RawMat";
private string prefixProductionRef = "ProductionRef";

private Color colorRed = new Color(175, 0, 0);
private Color colorWhite = new Color(255, 255, 255);

private string[] oreNames = new string[] {"Iron", "Silicon", "Nickel", "Cobalt", "Magnesium", "Uranium", "Gold", "Silver", "Platinum", "Stone"};
private Dictionary<string, VRage.MyFixedPoint> oresAndValues = new Dictionary<string, VRage.MyFixedPoint>();

private string productionMaterialTextForLCD = "Production Materials\n---------------------------------------------\n";
private string rawMaterialTextForLCD = "Raw Materials\n---------------------------------------------\n";
private string cargoInventorySpace = "CargoInventory\n---------------------------------------------\n";

//Constructor
public Program() {
	textPanelOne = GridTerminalSystem.GetBlockWithName("TextPanelOne") as IMyTextPanel;
	textPanelTwo = GridTerminalSystem.GetBlockWithName("TextPanelTwo") as IMyTextPanel;
	textPanelThree = GridTerminalSystem.GetBlockWithName("TextPanelThree") as IMyTextPanel;
	
	GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(allCargoContainers);
	GridTerminalSystem.GetBlocksOfType<IMyRefinery>(allRefineries);
}

//Main method
public void Main(string argument) {	
		
	//Create lists from the storage blocks and get inventoryspace
	for(int i = 0; i < allCargoContainers.Count; i++) {		
		if(allCargoContainers[i].CustomName.ToLower().Contains(prefixProduction.ToLower())) {
			productionMatList.Add(allCargoContainers[i]);
			
		}
		
		if(allCargoContainers[i].CustomName.ToLower().Contains(prefixRaw.ToLower())) {
			rawMatList.Add(allCargoContainers[i]);
		}
	}
	
	for(int i = 0; i < allRefineries.Count; i++) {		
		if(allRefineries[i].CustomName.ToLower().Contains(prefixProductionRef.ToLower())) {
			rawMatList.Add(allRefineries[i]);
		}
	}
	
	//###DISPLAY ONE###
	
	//Set inventory space on LCD
	writeToDisplay(textPanelOne, createInventorySpaceString(productionMatList, cargoInventorySpace));
	
	//Clear all
	cargoInventorySpace = "CargoInventory\n---------------------------------------------\n";
	
	//###DISPLAY TWO###
	
	//Set text color
	setTextPanelTextColor(textPanelTwo, colorWhite);
	
	//Production material methods
	createMaterialDictionary(productionMatList, oresAndValues);
	writeToDisplay(textPanelTwo, createInventoryItemValueString(oreNames, oresAndValues, productionMaterialTextForLCD, textPanelTwo, colorRed));
	
	//Clear All
	productionMaterialTextForLCD = "Production Materials\n------------------------\n";
	productionMatList.Clear();
	oresAndValues.Clear();
	
	//###DISPLAY THREE###
		
	//Set text color
	setTextPanelTextColor(textPanelThree, colorWhite);
	
	//Raw material methods
	createMaterialDictionary(rawMatList, oresAndValues);
	writeToDisplay(textPanelThree, createInventoryItemValueString(oreNames, oresAndValues, rawMaterialTextForLCD, textPanelThree, colorRed));
	
	//Clear All
	rawMaterialTextForLCD = "Raw Materials\n------------------------\n";
	rawMatList.Clear();
	oresAndValues.Clear();
}

//Create string of an inventory space in percent to display on an LCD
private string createInventorySpaceString(List<IMyTerminalBlock> materialList, string textForLCD) {
	for(int i = 0; i < materialList.Count; i++) {
		IMyInventoryOwner inventoryOwner = materialList[i] as IMyInventoryOwner;
		IMyInventory inventory = inventoryOwner.GetInventory(0);
		textForLCD += materialList[i].CustomName + ": " + getInventoryInPercent(inventory.CurrentVolume, inventory.MaxVolume) + "%\n"; 
	}
	return textForLCD;
}

//Make percentage of two values
private double getInventoryInPercent(VRage.MyFixedPoint currentValue, VRage.MyFixedPoint maxValue) {
	return ((double) currentValue / (double) maxValue) * 100;
}
	
//Set LCD panel text color
private void setTextPanelTextColor(IMyTextPanel textPanel, Color color) {
	textPanel.SetValue("FontColor", color);
}

//Create dictionary for the production materials
//private void createMaterialDictionary(List<IMyTerminalBlock> materialList, IMyInventoryOwner inventoryOwner, IMyInventory inventory, List<IMyInventoryItem> inventoryItems, Dictionary<string, VRage.MyFixedPoint> oresAndValues){
private void createMaterialDictionary(List<IMyTerminalBlock> materialList, Dictionary<string, VRage.MyFixedPoint> oresAndValues) {
	for(int i = 0; i < materialList.Count; i++) {
		IMyInventoryOwner inventoryOwner = materialList[i] as IMyInventoryOwner;
		IMyInventory inventory = inventoryOwner.GetInventory(0);
		List<IMyInventoryItem> inventoryItems = inventory.GetItems();
		
		if(inventoryItems.Count != 0) {
			for (int j = 0; j < inventoryItems.Count; j++) {
				if(!oresAndValues.ContainsKey(inventoryItems[j].Content.SubtypeName)) {
					oresAndValues.Add(inventoryItems[j].Content.SubtypeName, inventoryItems[j].Amount);
				} else {
					oresAndValues[inventoryItems[j].Content.SubtypeName] += inventoryItems[j].Amount;
				}
			}
		}
	}	
}

//Create string of inventory item values to display on an LCD
private string createInventoryItemValueString(string[] oreNames, Dictionary<string, VRage.MyFixedPoint> oresAndValues, string textForLCD, IMyTextPanel textPanel, Color color) {
	for(int i = 0; i < oreNames.Length; i++) {
		if(oresAndValues.ContainsKey(oreNames[i])) {
			textForLCD += oreNames[i] + ": " + oresAndValues[oreNames[i]] + "\n"; 
		} else {
			textForLCD += oreNames[i] + ": Mine some of this :'( \n";
			setTextPanelTextColor(textPanel, color);
		}
	}
	return textForLCD;
}

//Write string on an LCD
private void writeToDisplay(IMyTextPanel textPanel, string text) {
	textPanel.WritePublicText(text, false);
	textPanel.ShowPublicTextOnScreen();
	textPanel.UpdateVisual();
}
