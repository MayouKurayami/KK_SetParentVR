**DISCLAIMER**: This project is an effort to decompile and improve an existing plugin that was not written by me.  
The original plugin can be found on http://uppervolta-3d.net/illu/upload.php.

# KK_SetParentVR
> Control the characters' position, posture, and animation speed with controllers in Koikatsu official VR  
> *When Illusion doesn't give you more positions, make your own in realtime*




## Prerequisites  
- Afterschool DLC  
- [Koikatsu Official VR Patch 0531](http://www.illusion.jp/preview/koikatu/download/vr.php)
- BepInEx 4 and above  
- (optional, recommended): [CrossFader](https://github.com/MayouKurayami/KK_CrossFader/releases) plugin
- (optional, strongly recommended): [KoikatuVRAssistPlugin](https://mega.nz/#!YQZyWRwQ!C2FX0Iwp-X7F5z55ytTlQGkjfqH6kQP-wcDPfNBvT0s) plugin


## Installation  
- Download the latest **KK_SetParentVR.dll** from [releases](https://github.com/MayouKurayami/KK_SetParentVR/releases).
- Place **KK_SetParentVR.dll** in BepInEx root folder.  
  - Remove the old IPA version plugin if you have it installed.  
  - This is a BepInEx 4 plugin, to run it with BepInEx 5 you'd need [BepInEx 4 Patcher](https://github.com/BepInEx/BepInEx.BepInEx4Upgrader) (included with HF Patch)  


- **(Optional)** To translate the text on the floating menu, make sure the [AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator/releases) is installed **(also included with HF Patch)**.  Then make sure **EnableUGUI** is set to **True** under [*TextFrameworks*] in *BepInEx/config/AutoTranslatorConfig.ini*  

- Installation of the [**CrossFader**](https://github.com/MayouKurayami/KK_CrossFader/releases) plugin is recommended to ensure smooth transitions between animations and poses.

- Also **strongly recommended** is the [**KoikatuVRAssistPlugin**](https://mega.nz/#!YQZyWRwQ!C2FX0Iwp-X7F5z55ytTlQGkjfqH6kQP-wcDPfNBvT0s) plugin found on https://vr-erogamer.com/archives/322, for easier movement and better access to actions in VR. With it installed, one can rotate and move the camera at all times by holding the trigger button. While the UI menu is visible, hold the grip button for 1 second to freeze it in place, and drag and move it by holding the grip button. Double click the grip button to return the menu to your controller.

## Usage  
- Hide/unhide the floating helper menu by long holding the **Menu (B or Y on the Rift) button** for more than **one second**.  

- Enable/disable SetParent plugin by either pressing the button on the floating menu or by pressing **Menu (B or Y on the Rift)** ***and*** **Trigger** at the same time, or by pressing the keyboard shortcut set in config. The girl will then be controlled by the other controller that wasn't pressing the button combo. Depending on the config, the girl's position and/or the speed of the piston will start being affected by the controller.

- Once SetParent is enabled, you can do the following:  
  - If the floating menu is currently hidden, you can temporarily bring it up by moving either controller close to your head. (Enabled by default in settings)
  - Hold **Grip** *and* **Trackpad (Thumbstick)** ***DOWN*** at the same time to adjust the position of the male character. Your best friend when ran into genitals animation glitch and clipping.   
  - Hold **Grip** *and* **Trackpad (Thumbstick)** ***UP*** at the same time to adjust the position of the female character temporarily using the non-parenting controller.  
  - Hold **Grip** *and* **Trigger** at the same time to move the camera while having **both** characters following the movement.
  - Grab the girl's hands or feet by touching them. Pressing **Trigger** will release the hand/foot while holding it at the current position.   
  - You can also hold the position of each limb by pressing the buttons on the floating menu  
  - If enabled in settings, girl's hands and feet as well as guy's feet will stick to objects on contact. **Girl's limbs will also stick to your shoulder on contact**.  
  - When stretched beyond a certain point, the limbs will automatically detach from objects. *Note: the limb will not automatically detach if it was held in place manually via the controller or the floating menu.*
  - **Double click Trigger** near a hand or foot will return it to the default position. **Double clicking Trigger while not being near any particular hand or foot** will return *ALL* limbs to their default positions. Extremely useful when limbs get stuck to the wrong places.  
  - **Hold Grip *then* double click Trigger** to fix or release male's feet at where they are.


- Pick a position or H category using the in-game menu, then use the in-game menu or the floating menu to insert. Once inserted, you can control the movement speed by moving the controller that the girl is parented to. Moving the controller past the configured range would cause the animation to switch from weak to strong motion. Maintain the movement range within the configured threshold for **1.5 seconds** will cause it to switch back from strong to weak motion.  

- The positions buttons in the left floating menu are **NOT** meant to be a replacement for changing positions via the built-in menu. They are designed to change only the girl's **posture** as they do not change the position and rotation of the characters.    

- If enabled in settings, after maintaining the girl's excitement gauge past 70 for set number of seconds, animation will enter orgasm automatically.  

- Controller input will be disabled for the parenting controller, to prevent unintentional or accidental input.


## Configurations
Configurations are located in *config.ini* in the BepInEx root folder, under section **MK.KK_SetParentVR**.  

***It is recommended to adjust the configs via the in-game plugin settings page (accessible outside of VR) instead of directly editing the config file.***
- **Autofinish Timer (Finishcount)** - When the girl's excitement gauge is above the cum threshold (70), she will cum after this set number of seconds. **Set this to 0 to disable the feature** **(Default: 0)**

- **Control Mode (SetParentMode)** - Whether to use the controller to control the girl's position, animation, or both. **(Default: PositionAndAnimation)**  

- **Distance to Display Hidden Menu (MenuUpProximity)** - When the floating menu is hidden, bring the controller close to the headset within this distance to temporarily display the menu. Unit in meters approximately. Set to 0 to disable this feature **(Default:0.25)**  

- **Enable Holding Girl's Limbs with Controllers (SetControllerCollider)** - If enabled, touching the girl's hands or feet with a controller will cause it to stick to the controller **(Default: True)**  

- **Gaze Control** - Enable/disable selecting any menu item by looking at it for more than 1 second **(Default: False)**  

- **Hide Floating Menu by Default (MenuHideDefault)** - Whether to hide floating menu by default. Bring it up by holding the Menu(A/B) button for more than 1 second, or bring the controller close to the headset when SetParent is active **(Default: True)**  

- **Make Girl's Hands and Feet Stick to Objects (SetFemaleCollider)** - If enabled, girl's hands and feet will automatically grab onto objects when in contact. **(Default: True)**  

- **Make Male's Feet Stick to Objects (SetMaleCollider)** - If enabled, male's feet will automatically grab onto objects when in contact. This is useful to prevent male's feet from moving in mid air or clipping into the ground. **(Default: True)**   

- **Synchronize Male's Head with Headset (SetParentMale)** - If enabled, the male body will rotate to align his head with the headset, so that it is never in front of you. Useful if you don't like getting NTR'ed by your own 3D model  **(Default: True)**  

- **Synchronize Male's Hands with Controllers (SyncMaleHands)** - If enabled, the male's hands and arms will be synchronized to the controllers. Useful for when male's hands are in the wrong place. This can be toggled on and off in the floating menu.  **(Default: True)**  

- **Which Controller Controls Animation(CalcController)** - Select which controller affects animation speed and switching between weak/strong motion. **(Default: SetParentController)**  

#### Keyboard Shortcuts  
- **Limb Release** - Press this key to release all limbs from attachment. **(Default: None)**  

- **Male Feet Fix/Release** - Press this key to fix or release both male's feet in place. This can also be toggled by holding **Grip** then double click **Trigger**. **(Default: None)**  

- **SetParent Toggle** - Press this key to enable/disable SetParent plugin using the left controller as parent. **(Default: None)**
<br/><br/>

### Advanced Settings  
(Under the hood stuff. Tread carefully)
- **Animation Max Threshold** - When movement amount of the controller reaches this value, animation speed will be at max. Unit in meters approximately. **(Default: 0.2)**  

- **Animation Start Threshold** - Movement amount of the controller above this threshold will cause piston animation to start. Unit in meters approximately **(Default: 0.04)**  

- **Controller Average Position Pool Size (MoveCoordinatePoolSize)** - Position of the controller will be calculated using the average position in this number of frames. This value is used to calculate movement range of the controller. **(Default: 8)**  

- **Controller Movement Pool Size (MoveDistancePoolSize)** - Movement amount of the controller will be calculated using the sum of distance moved in this number of frames. **(Default: 55)**

- **Distance to Detach Female Arms (StretchLimitArms)** - When stretched above this distance, the arms that are currently attached to objects will detach. Unit in meters approximately. This has no effect when the arms are attached manually via the floating menu button or by the controller  **(Default: 0.5)**  

- **Distance to Detach Female Legs (StretchLimitLegs)** - When stretched above this distance, the legs that are currently attached to objects will detach. Unit in meters approximately. This has no effect when the legs are attached manually via the floating menu button or by the controller. **(Default: 0.7)**  

- **Male Hands Display Mode (MaleHandsDisplay)** - When male hands are synchronized with the controllers, this controls whether additioanl hands will show on female body parts when you touch them. Set to auto to automatically hide them based on the proximity of your hands. **(Default: Auto)**

- **Male Yaw Rotation (MaleYaw)** - Enable/disable male body's yaw (left/right) rotation when male synchronization is enabled. **(Default: True)**

- **Part of Girl's Body to Parent with The Controller (ParentPart)** - Use this body part to act as the center/origin that will correspond to the movement and rotation of the controller **(Default: Torso)**

- **Smooth Tracking (TrackingMode)** - Enables smooth(ramped) following of the girl's body to the controller. Disable to use strict and immediate following. **(Default: True)**  

- **Strong Motion Speed Maximum Multiplier (StrongThresholdMultiplier)** - In strong motion, multiply the animation speed max threshold by this number to avoid reaching the maximum speed too easily due to the wide range of motion. **(Default: 1.3)**  

- **Strong Motion Threshold** - If the movement range of the controller calculated using *MoveCoordinatePoolSize* exceeds this threshold, animation will switch to strong motion. Unit in meters approximately. **(Default: 0.03)**  

- **Weak Motion Threshold** - If the movement range of the controller stays within this threshold for **1.5 seconds**, animation will switch to weak motion. Unit in meters approximately. **(Default: 0.01)**


## Notes and Limitations
- Unknown compatibility with Koikatsu Party (Steam release)  

- Currently does not work in the unofficial VR mod for main game.
- The plugin does not work well with positions that have significant animation differences between motions (e.g., cowgirl).  
Use the floating menu to switch to **Animation Only Mode** as a workaround.  

- The positions buttons in the left floating menu are **NOT** meant to be a replacement for changing positions via the built-in menu. They are designed to change only the girl's **posture** as they do not change the position and rotation of the characters.   

- Changing girl's position via the floating menu will not change the guy's position. This may cause the guy's hands to be in unnatural or undesired positions at times. Enable male hands to controllers synchronization **(SyncMaleHands)** to mitigate this.

- Likewise, the guy's feet will often clip into the ground if using a standing position while being close to the ground. Enabling the male feet collider option mitigates this somewhat, but not much. You can also position the male feet to a desired position then fix them there using the controller or keyboard shortcut.

- Limited functionality in 3P. Currently only the first girl's position will be controlled. The plugin cannot control the position and pose of the second girl.  

- With male following enabled, the shadow casted by the male body may be distracting. Use [KK_HAutosets](https://github.com/MayouKurayami/KK_HAutoSets) to disable it.


## Credits
All credit of the plugin up to version 0.9b goes to the unknown developer who made this plugin.

## Disclaimer
This software and its developer is not responsible for any damage to hardware or catastrophic loss of data due to overextended usage.
