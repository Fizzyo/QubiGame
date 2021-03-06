# QubiGame
Qubi Game - Endless runner using your breadth to power up Qubi to give a super boost to compelete the levels

This project was developed at ICHealth Hack s as part of the Project Fizzyo (http://fizzyo.github.io) Project team Supervised by - Prof. Eleanor Main of UCL/GOSH

# Example
### Scenes/StartCalibrate.unity
A simple example of pre-calibrating your game for use by different people with different lung strengths.
In this example the user is invited to blow steadily for 2 seconds to start the game, during this time the max blow strength is recorded and used to scale pressure values in subsequent levels.

### Scenes/JetpackLevel.unity
Breathing into the device propels a character into the air using a jetpack with the height of the character is mapped to the breath strength. The fizzyo's  button or spacebar can be used to fire missiles.


## Test Harness
This example includes a test harness and test data that allows you to load and playback breath data saved from a fizzyo device.
To use this a singleton class is provided FizzyoDevice.Instance() that can be used at any point in your code if FizzyoDevice.cs is present in your project.

By default FizzyoDevice plays back pre-recorded data but can also be used to gather data directly from the device if the bool useRecordedData is set to false.
This can be done through the editor or programmatically in your code.
[image]
This allows you to program your game completely against pre-recoreded pressure values if desired and switched over to live values at a later stage.

# C#

```
FizzyoDevice.cs

/* (float) Return the current pressure value, either from the device or streamed from a log file.
*   range: -1.0f - 1.0f
*   comment: if useRecordedData is set pressure data is streamed from the specified data file instead of the device.
*/
FizzyoDevice.Instance().Pressure();


/* (bool) Return if the fizzyo device button is pressed */
FizzyoDevice.Instance().ButtonDown();

```


# Disclaimer 

GOSH/ICH/UCL Fizzyo Games are provided under a GNU AFFERO GENERAL PUBLIC LICENS and all terms of that licence apply (See Licence.txt). Use of the Game code is entirely at your own risk. GOSH/ICH/UCL accept any responsibility for loss or damage to any person, property or reputation as a result of using the software or code. No warranty is provided by any party, implied or otherwise. This software and code is not guaranteed safe to use in a clinical or other environment and you should make your own assessment on the suitability for such use. Installation of any software, indicates acceptance of this disclaimer.