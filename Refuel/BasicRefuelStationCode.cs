//A basic script for a refuel station.
//When a sensor detects a ship or car, a piston extends and two indicator lights are turned on
//Sensor active -> Lights green - Sensor inactive -> Lights red

private IMyPistonBase pistonBase;
private IMySensorBlock sensor;
private IMyReflectorLight reflectorLightOne;
private IMyReflectorLight reflectorLightTwo;
private Color color;
private bool pistonIn = true;


//Constructor
public Program() {
    pistonBase = GridTerminalSystem.GetBlockWithName("Piston") as IMyPistonBase; //Insert piston name here
    sensor = GridTerminalSystem.GetBlockWithName("Sensor") as IMySensorBlock; //Insert sensor name here
    reflectorLightOne = GridTerminalSystem.GetBlockWithName("Spotlight") as IMyReflectorLight; //Insert spotlight name here
	reflectorLightTwo = GridTerminalSystem.GetBlockWithName("Spotlight") as IMyReflectorLight; //Insert spotlight name here	
}


//Main method
public void Main(string argument) {

    if(sensor.IsActive) {
		if(pistonIn) {
			color = new Color(0, 255, 0);
			reflectorLight.SetValue("Color", color);
		
			pistonBase.GetActionWithName("Reverse").Apply(pistonBase);
			pistonIn = !pistonIn;
		}
    } else {
		if(!pistonIn) {
			color = new Color(255, 0, 0);
			reflectorLight.SetValue("Color", color);
		
			pistonBase.GetActionWithName("Reverse").Apply(pistonBase);
			pistonIn = !pistonIn;
		}
	}
}

