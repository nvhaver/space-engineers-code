//Automatic solar panel code
//Solar panels will rotate at sun speed
//The only thing you need to do is build an array of solar panels on a rotor and put these all together in a group.
//Make sure the group has a prefix in the name, for example -> "ABC Solarpanel group name". "ABC" is the prefix here.

private IMyTextPanel lcdOne; //If you don't use an LCD, delete this line

private IMyMotorStator motor = null;
private List<IMySolarPanel> solarPanelList = new List<IMySolarPanel>();

private bool firstTime = true;
private bool inPositionStart = false;
private bool calibrated = false;
private bool rollBack = false;

private float previousAngle = 0f;
private float previousPower = 0f;
 
 
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
	
	if(firstTime) {
		//Take all groups on the grid    
		List<IMyBlockGroup> allBlockGroupsList = new List<IMyBlockGroup>();     
		GridTerminalSystem.GetBlockGroups(allBlockGroupsList);     
	 
		//Get the group with the argument
		IMyBlockGroup selectedBlockGroup = null;

		for(int i = 0; i < allBlockGroupsList.Count; i++) {     
			if(allBlockGroupsList[i].Name.ToLower().Contains(GetPrefixFromArgument(argument).ToLower())) {
				selectedBlockGroup = allBlockGroupsList[i];
			}        
		}     
		
		//Loop through the selected groupslist;      
		for(int i = 0; i < selectedBlockGroup.Blocks.Count; i++) {     
		 
			IMySolarPanel solarPanel = selectedBlockGroup.Blocks[i] as IMySolarPanel;    
			if(solarPanel != null) {    
				solarPanelList.Add(solarPanel);     
			}    
			   
			IMyMotorStator solarPanelRotor = selectedBlockGroup.Blocks[i] as IMyMotorStator;    
			if(solarPanelRotor != null) {    
				motor = solarPanelRotor;    
			}    
		}
		
		motor.GetActionWithName("ResetVelocity").Apply(motor);    
		firstTime = false;
	}
	
	//set the rotor at the start position		
	if(!inPositionStart) {
		SetToStart(motor);
	}
	
	//calibrate the rotor
	if(!calibrated) {   
		if(inPositionStart) {
			float currentPower = GetCurrentSolarOutputInkWatt(solarPanelList);
			float currentAngle  = GetMotorAngle(motor);
			if(currentPower != 0) {
				if(currentPower > previousPower && !rollBack) {
					RotateWithVelocity(motor, -1);
					previousAngle = currentAngle;
					previousPower = currentPower;
				} else {
					RotateWithVelocity(motor, 1);
					rollBack = true;
					if(currentAngle == previousAngle) {
						motor.GetActionWithName("ResetVelocity").Apply(motor); 
						RotateOnSunSpeed(motor, argument);
						calibrated = true;
					}
				}
			}
		}
	}
	
    lcdOne.ShowPublicTextOnScreen();   
    lcdOne.UpdateVisual();     
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
private void SetToStart(IMyMotorStator motor) {	
	if(GetMotorAngle(motor) != 90) {
		/*if(GetMotorAngle(motor) <= 270) {
			RotateWithVelocity(motor, -1);
		} else {
			RotateWithVelocity(motor, 1);
		}*/
		RotateWithVelocity(motor, 1);
	} else {
		inPositionStart = true;
		motor.GetActionWithName("ResetVelocity").Apply(motor);
	}
}
 
 //method to rotate a rotor with a speed
private void RotateWithVelocity(IMyMotorStator motor, Single velocity) { 
    	motor.SetValue("Velocity", velocity);		 
} 

private void RotateOnSunSpeed(IMyMotorStator motor, string argument) {
	float rotorAngle = 360f;
	float time = GetTimeFromArgument(argument);
	
	//float rotateSunSpeed = (rotorAngle / time) / 60;
	float rotateSunSpeed = (rotorAngle / time); 
	
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