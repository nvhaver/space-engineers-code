private string LCDName = "LCDPanel"; // Insert the LCD Panel name here
private IMyTextPanel lcdPanel; 
private int counter; // Counter to keep track of the amount of calls 

/// Pre-execution setup method
public Program() { 
	InitLCD();
}       
 
/// Initialize the LCD screen variables 
private void InitLCD(){
	lcdPanel = null; 
	List<IMyTerminalBlock> results = new List<IMyTerminalBlock>();      
    GridTerminalSystem.SearchBlocksOfName(LCDName, results); 
	 
	if(results.Count != 0) { 
        lcdPanel = results[0] as IMyTextPanel; 
    }
}
 
/// Main method
public void Main(string argument) {
	writeDateTime();
} 
 
/// Append a line of text to the contents of the LCD
/// Clears the screen if the panel is full and appends newlines
/// The parameters maxLines indicates the amount of lines that fills the screen
private void writeMessageAppend(string message, int maxLines){
	bool clear = !(counter%maxLines == 0);
	
	lcdPanel.WritePublicText(message+"\n", clear);
	lcdPanel.ShowPublicTextOnScreen(); 
	lcdPanel.UpdateVisual();
	counter++;
} 

/// Write a line to the screen without appending
private void writeMessage(string message){
	lcdPanel.WritePublicText(message, false);
	lcdPanel.ShowPublicTextOnScreen(); 
	lcdPanel.UpdateVisual();
}

/// Print the current time
private void writeDateTime(){
	writeMessage(DateTime.Now.ToString());
}