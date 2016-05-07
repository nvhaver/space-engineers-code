//Automatic solar panel code
//Solar panels will rotate at sun speed
//The only thing you need to do is build an array of solar panels on a rotor and put these all together in a group. (You can make more than one group of these)
//Make sure every group has a prefix in the name and this prefix must be the same for every group, for example -> "ABC Solarpanel group name". "ABC" is the prefix here.

private IMyTextPanel lcdOne; //If you don't use an LCD, delete this line
private List<SolarPowerGroup> solarPowerGroupList = new List<SolarPowerGroup>();
private List<String> groupNames = new List<String>();
private bool firstTime = true;
 
 
 //Constructor
public Program() {     
    lcdOne = GridTerminalSystem.GetBlockWithName("Wide LCD panel") as IMyTextPanel; //Insert name of textpanel if you want to display the output. If you don't use an LCD, delete this line
}
 
 
 
public void Save() {     
    // Called when the program needs to save its state. Use     
    // this method to save your state to the Storage field     
    // or some other means.      
}     
 
 
 //Main method
public void Main(string argument) {     
 
    //Take all groups on the grid    
    List<IMyBlockGroup> allBlockGroupsList = new List<IMyBlockGroup>();     
    GridTerminalSystem.GetBlockGroups(allBlockGroupsList);     
 
    //Make a selection with the argument
    List<IMyBlockGroup> selectedBlockGroupsList = new List<IMyBlockGroup>();     

    for(int i = 0; i < allBlockGroupsList.Count; i++) {     
        if(allBlockGroupsList[i].Name.ToLower().Contains(GetPrefixFromArgument(argument).ToLower())) {
            selectedBlockGroupsList.Add(allBlockGroupsList[i]);     
        }        
    }     
     
    //Loop through the selected groupslist;     
    for(int i = 0; i < selectedBlockGroupsList.Count; i++) {     
		string groupName = selectedBlockGroupsList[i].Name;
        IMyMotorStator motor = null;     
        List<IMySolarPanel> solarPanelList = new List<IMySolarPanel>();
		     
		for(int j = 0; j < selectedBlockGroupsList[i].Blocks.Count; j++) {     
		 
			IMySolarPanel solarPanel = selectedBlockGroupsList[i].Blocks[j] as IMySolarPanel;    
			if(solarPanel != null) {    
				solarPanelList.Add(solarPanel);     
			}    
			   
			IMyMotorStator solarPanelRotor = selectedBlockGroupsList[i].Blocks[j] as IMyMotorStator;    
			if(solarPanelRotor != null) {    
				motor = solarPanelRotor;    
			}    
		}
		
		//Create new objects. Check for new groups to add
		if(selectedBlockGroupsList.Count > solarPowerGroupList.Count) {
			if(solarPowerGroupList.Count != 0 && !firstTime) {
				if(!groupNames.Contains(selectedBlockGroupsList[i].Name.ToLower())) {
					CreateAndAddPowerGroup(groupName, motor, solarPanelList);
				}
			} else {
				CreateAndAddPowerGroup(groupName, motor, solarPanelList);
			}			
		}
		
    }
	
	firstTime = false;
	
	//set the rotors a start position
	for(int i = 0; i < solarPowerGroupList.Count; i++) {
		bool inPositionStart = solarPowerGroupList[i].InPositionStart;
		
		if(!inPositionStart) {
			SetToStart(solarPowerGroupList[i]);
		}
	}
	
	//calibrate the rotors
	for(int i = 0; i < solarPowerGroupList.Count; i++) {	
		bool calibrated = solarPowerGroupList[i].Calibrated;
		bool inPositionStart = solarPowerGroupList[i].InPositionStart;
		bool rollBack = solarPowerGroupList[i].RollBack;
		IMyMotorStator motor = solarPowerGroupList[i].Motor;
		List<IMySolarPanel> solarPanelList = solarPowerGroupList[i].SolarPanelList;
		float previousAngle = solarPowerGroupList[i].PreviousAngle;
		float previousPower = solarPowerGroupList[i].PreviousPower;
		
		if(!calibrated) {
			if(inPositionStart) {
				float currentPower = GetCurrentSolarOutputInkWatt(solarPanelList);
				float currentAngle  = GetMotorAngle(motor);
				
				if(currentPower != 0) {
					if(currentPower > previousPower && !rollBack) {
						RotateWithVelocity(motor, -1)	;
						solarPowerGroupList[i].PreviousAngle = currentAngle;
						solarPowerGroupList[i].PreviousPower = currentPower;
					} else {
						RotateWithVelocity(motor, 1);
						solarPowerGroupList[i].RollBack = true;
						if(GetMotorAngle(motor) == previousAngle) {
							motor.GetActionWithName("ResetVelocity").Apply(motor); 
							RotateOnSunSpeed(motor, argument);
							solarPowerGroupList[i].Calibrated = true;
						}
					}
				}
			}
		}
		
	}
        
    //lcdOne.WritePublicText(motor.Angle.ToString() + " / " + motor2.Angle.ToString(), false);    
    lcdOne.ShowPublicTextOnScreen();   
    lcdOne.UpdateVisual();     
}

//method to create a new group object
private void CreateAndAddPowerGroup(string groupName, IMyMotorStator motor, List<IMySolarPanel> solarPanelList) {
		SolarPowerGroup solarPowerGroup = new SolarPowerGroup(groupName, motor, solarPanelList);
		//lcdOne.WritePublicText(groupName, false);   
		solarPowerGroupList.Add(solarPowerGroup);
		groupNames.Add(groupName.ToLower());
}

//method to get the prefix form the argument
private string GetPrefixFromArgument(string argument) {
	string prefixMark = "-";
	int indexOfPrefixMark = argument.IndexOf(prefixMark, 0);
	
	return argument.Substring(0, indexOfPrefixMark);
}

//method to get the day night cyclus from the argument
private long GetTimeFromArgument(string argument) {
	string prefixMark = "-";	
	int indexOfPrefixMark = argument.IndexOf(prefixMark, 0);	
		
	return long.Parse(argument.Substring(indexOfPrefixMark + 1));
}

//method to get the output of a solar panel array in a group
private float GetCurrentSolarOutputInkWatt(List<IMySolarPanel> solarPanels) {   
    float totalOutput = 0;   
    string startCO  = "Current Output: ";  
    string startSpace = " ";     
   
    for(int i = 0; i < solarPanels.Count; i++) {   
        string solarPanelDetailedInfo = solarPanels[i].DetailedInfo;   
        if(solarPanelDetailedInfo.Contains(startCO)) {   
            int startIndexCO = solarPanelDetailedInfo.IndexOf(startCO , 0) + startCO .Length;   
            string solarPanelCurrentOutputkW = solarPanelDetailedInfo.Substring(startIndexCO);   
  
            int startIndexSpace = solarPanelCurrentOutputkW.IndexOf(startSpace, 0);  
            string solarPanelCurrentOutput = solarPanelCurrentOutputkW.Substring(0, startIndexSpace);  
 
            totalOutput += float.Parse(solarPanelCurrentOutput); 
        }   
    }   
    return totalOutput;   
} 

//method to rotate a rotor to his start position
private void SetToStart(SolarPowerGroup solarPowerGroup) {
	
	IMyMotorStator motor = solarPowerGroup.Motor;
	bool inPositionStart = solarPowerGroup.InPositionStart;
	
	if(GetMotorAngle(motor) != 90) {
		if(GetMotorAngle(motor) <= 270) {
			RotateWithVelocity(motor, -1);
		} else {
			RotateWithVelocity(motor, 1);
		}
	} else {
		inPositionStart = true;
		motor.GetActionWithName("ResetVelocity").Apply(motor);
	}
}
 
 //method to rotate a rotor with a speed
private void RotateWithVelocity(IMyMotorStator motor, double velocity) { 
    	motor.SetValue("Velocity", velocity);		 
} 

private void RotateOnSunSpeed(IMyMotorStator motor, string argument) {
	int rotorAngle = 360;
	long time = GetTimeFromArgument(argument);
	
	double rotateSunSpeed = ((rotorAngle / time) / 60);
	
	RotateWithVelocity(motor, rotateSunSpeed);
}
 
 //method to get the rotated angle of a rotor
private float GetMotorAngle(IMyMotorStator motor) { 
	float angle = 0; 
	string startCA = "Current angle: "; 
	string motorDetailedInfo = motor.DetailedInfo; 
 
	if(motorDetailedInfo.Contains(startCA)) { 
		int startIndexCA = motorDetailedInfo.IndexOf(startCA, 0) + startCA.Length;
		string motorCurrentAngle = motorDetailedInfo.Substring(startIndexCA);

		int stopIndexMCA = motorCurrentAngle.Length - 1;		 
		string motorAngle = motorCurrentAngle.Substring(0, stopIndexMCA); 
	 
		angle = float.Parse(motorAngle); 
	}
    return angle; 
}

//solar group class
private class SolarPowerGroup {
	
	private string groupName;
	private IMyMotorStator motor;
	private List<IMySolarPanel> solarPanelList;
	private bool inPositionStart = false;
	private bool calibrated = false;
	private bool rollBack = false;
	private float previousAngle = 0f;
	private float previousPower = 0f;
	
	public SolarPowerGroup(string groupName, IMyMotorStator motor, List<IMySolarPanel> solarPanelList) {
		this.groupName = groupName;
		this.motor = motor;
		this.solarPanelList = solarPanelList;
		
		motor.GetActionWithName("ResetVelocity").Apply(motor);
	}
	
	public string GroupName { get; set; }
	public IMyMotorStator Motor { get; set; }
	public List<IMySolarPanel> SolarPanelList { get; set; }
	public bool InPositionStart { get; set; }
	public bool Calibrated { get; set; }
	public bool RollBack { get; set; }
	public float PreviousAngle { get; set; }
	public float PreviousPower { get; set; }
	
}