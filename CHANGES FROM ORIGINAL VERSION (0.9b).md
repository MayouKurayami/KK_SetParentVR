### 2.0.1  
- Switch to BepInEx framework instead of IPA
- Make configs visible and adjustable in the non-VR in-game plugin settings
- Add config option to customize or disable the autofinish feature
- Add config option to disable gaze control  
- Add config options to customize floating menu display behavior  
- Add config options to customize female limb's auto detachment threshold
- Add feature to independently control the female and male's position and rotation with the controller  
- Add feature to freely position the limbs of the female using the controller  
- Add controller and keyboard shortcut to release limbs from attachment  
- Add option to add colliders to male's feet so that they can stick to the ground  
- Overhaul of the male following system, such that the male's head would also align with the headset and not block it  
- Overhaul the floating menu buttons. Added button to change the SetParent Mode on the fly.
- Fix various animation issues caused by IK's in positions such as cowgirl  
- Fix character misplacement when switching position/category/H point  
- Other various bugfixes

### 2.0.4  
- Improve compatibility with KoikatuVRAssistPlugin  
- Add config option to enable/disable the grabbing of girl's limbs with controller  
- Add floating menu button to disable all limb fixing functionalities
- Limb fixing buttons now work even when SetParent is not enabled  
- Hide left floating menu position buttons in caress mode to prevent issues with kissing
- Fix left floating menu position buttons behavior when SetParent is not enabled
- Smooth the release of female default IK's  
- Limbs will now fix to new objects created in the scene (e.g., vault in vault doggy position)  
- Fix Japanese text not displaying properly in floating menu
- Change some text on position buttons  
- Fix issues caused by controller(s) being inactive at runtime  
- Adjust advanced settings default values

### 2.1.0  
- Add feature to sync male's hands to controllers  
- Add controller and keyboard shortcuts to fix/release male feet  
- Disable controller input for the parenting controller to prevent accidental input
- Fix issues with misplaced female limbs due to IK  
- Fix issue with male limbs pulling the head or body out of position  
- Fix controller 3D model visibility not updating when changing SetParentMode via the floating menu  
- Fix issues with modifiers not working for keyboard shortcuts  
- Fix issues with controller shortcuts sometimes not properly toggling SetParent  

### 2.1.8  
- Improve config defaults  
- Allow grabbing and holding the limbs of female even when SetParentVR is not enabled   
- Allow male hands synchronization when SetParentVR is not enabled  
- Make female limbs retain their positions when SetParentVR is disabled
- Hide parenting controller in AnimationOnly mode, and add config option to unhide it  
- Add config option to enable/disable input of parent controller  
- Add buttons to toggle synchronization of each male hand separately
- Improve behavior of buttons that toggle the forced fixing of male's feet
- Fix controller colliders not working in some cases  
- Fix SetParentVR not being reset when moving to different H points of the same position
- Floating menu re-arrangement and other cosmetic changes  
- Performance improvements and code refactoring
