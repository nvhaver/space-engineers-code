public Program() {

    // The constructor, called only once every session and
    // always before any other method is called. Use it to
    // initialize your script. 
    //     
    // The constructor is optional and can be removed if not
    // needed.

}

public void Save() {

    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.

}

public void Main(string argument) {

    // The main entry point of the script, invoked every time
    // one of the programmable block's Run actions are invoked.
    // 
    // The method itself is required, but the argument above
    // can be removed if not needed.
    
}

public struct Engine
{
    public int thrust; // Maximum Thrust (kN)
    public int mass; // Mass (kg)
    public int power; // Max Power Consumption (kW)
    
    public Engine(int t, int m, int p)
    {
        thrust = t;
        mass = m;
        power = p;
    }
}

private static Dictionary<string, Engine> ENGINES = new Dictionary<string, Engine>()
{
    {"SATS", new Engine(80, 539, 701)},
    {"SATL", new Engine(420, 4072, 2360)},
    {"LATS", new Engine(408, 4244, 2400)},
    {"LATL", new Engine(5400, 33834, 16360)}
};

/// Calulate how many engines are needed to provide a given amount of lift
private float CalculateNeeded(float lift) {
    // Lift [kg] = engine force [N] * effectivity [unitless] / acceleration due to gravity [m/s²]
    
    // Calculate for all engine types the amount needed (which efficiency? gravAccel?)
    foreach
    
    return 0f;
}

/// Calulate how many lift the given engines provide
private float LiftForEngines(int numEngines, string engineType, float efficiency = 0.9, float gravAccel = 9.81) {
    //if(ENGINE_THRUST.ContainsKey(engineType)}
        Engine engine = ENGINES[engineType];
        return (efficiency * engine.thrust) / gravAccel;
    /*} else {
        // Throw new exception or will it do that by itself (e.g. KeyNotFoundException)?
        return 0f;
    }*/
}

private boolean WillFly(int numEngines, string typeEngines, float efficiency = 0.9, float gravAccel = 9.81, float mass) {
    try {
        return LiftForEngines(numEngines, typeEngines, efficiency, gravAccel) > mass;
    }
    catch (KeyNotFoundException ex){
        // Throw again?
    }
}