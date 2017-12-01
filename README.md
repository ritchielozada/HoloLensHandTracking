## HoloLens Hand Tracking

### Developer Tools Version

- Unity 2017.2.0f3
- Windows SDK 10.0.16299.0
- Windows 10 Version 1709 OS Build 16299

### HoloLens Tested Version

- Windows version	14393.1770.x86fre.rs1_release.170917-1700
- Windows version	14393.1884.x86fre.rs1_release.171101-1233

### Release Notes 11/30/2017
- Included GestureRecognizer to handle Tap/Hold Events
- GestureRecognizer Tap/Hold Events does not create a "Hand" InteractionSourceKind and Source ID is "0", the event cannot map to previous Hand Detection Events
- The "Tap" event does not have a "Tap Release", the workaround is to use "HoldStarted" and "HoldCompleted" to detect the release equivalent

### Release Notes 11/14/2017
- Updated Project for Unity 2017.2.0f3 / Windows 10.0.16299.0 SDK
- Removed HoloToolkit Dependency,  Project Self-Contained
