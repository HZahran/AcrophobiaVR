using System.IO;
using System.Text;

public class Tests {

    private float head; // Testing Head Movement Artifact
    private float press; // Testing Finger Press Artifact
    private float sound; // Testing change in sound
    private float fall; // Testing sudden fall
    private float visual; // Testing change in visuals & sudden darkness
    private float maxFear; // Keeping track of max lvl of fear

    // Tests Time
    public float headTime = 5; 
    public float pressTime = 2; 
    public float soundTime = 5; 
    public float fallTime = 5; 
    public float visualTime = 5;

    private string[] CSV_COLS = new string[] {"Head Movement", "Pressing", "Sound", "Falling","Visuals","Max Fear"};

    public float HeadTest
    {
        get { return head; }
        set { head = value; }
    }

    public float PressTest
    {
        get { return press; }
        set { press = value; }
    }
    public float SoundTest
    {
        get { return sound; }
        set { sound = value; }
    }
    public float FallingTest
    {
        get { return fall; }
        set { fall = value; }
    }
    public float VisualTest
    {
        get { return visual; }
        set { visual = value; }
    }
    public float MaxFear
    {
        get { return maxFear; }
        set { maxFear = value; }
    }

    public void SaveData()
    {
        string filePath = @"/Tests.csv";
        string delimiter = ",";

        // Insert new row
        string[] output = new string []{ HeadTest.ToString(),
                                         PressTest.ToString(),
                                         SoundTest.ToString(),
                                         FallingTest.ToString(),
                                         VisualTest.ToString(),                                         HeadTest.ToString(),
                                         MaxFear.ToString()};

        StringBuilder sb = new StringBuilder();

        if (!File.Exists(filePath))
            sb.AppendLine(string.Join(delimiter, CSV_COLS)); // Add Col Names

        sb.AppendLine(string.Join(delimiter, output)); // Add New Recording
        File.AppendAllText(filePath, sb.ToString()); // Save To File
    }
}
