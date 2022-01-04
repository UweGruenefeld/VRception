# VRception Toolkit
**Rapid Prototyping of Cross-Reality Systems in Virtual Reality**

![The VRception Toolkit allows users to transition on the Reality-Virtuality Continuum.](/Figures/RealityVirtualityContinuum.jpg?raw=true "The VRception Toolkit allows users to transition on the Reality-Virtuality Continuum.")

**Figure 1: The VRception Toolkit allows users to transition on the Reality-Virtuality Continuum [[1]](#references), simulating different manifestations of the Continuum, such as Augmented Reality (AR) or Augmented Virtuality (AV), inside of Virtual Reality. The figures (a-d) demonstrate the alpha-blending function to transition between concrete manifestations, however, other transition functions are possible as well.**

ABSTRACT Cross-Reality Systems empower users to transition along the Reality-Virtuality Continuum or collaborate with others experiencing different manifestations of it [[2]](#references). However, prototyping these systems is challenging, as it requires sophisticated technical skills, time, and often expensive hardware. We present VRception, a concept and toolkit for quick and easy prototyping of Cross-Reality Systems. By simulating all levels of the Reality-Virtuality Continuum entirely in Virtual Reality, our concept overcomes the asynchronicity of realities, eliminating technical obstacles. Our VRception Toolkit leverages this concept to allow rapid prototyping of Cross-Reality Systems and easy remixing of elements from all continuum levels. We replicated six cross-reality papers using our Toolkit and presented them to their authors. Interviews with them revealed that our Toolkit sufficiently replicates their core functionalities and allows quick iterations. Additionally, remote participants used our Toolkit in pairs to collaboratively implement prototypes in about eight minutes that they would have otherwise expected to take days.

Links: [Paper (probably not available yet)](https://doi.org/10.1145/3491102.3501821)


## Table of Contents
1. [Overview of the VRception Toolkit](#overview-of-the-vrception-toolkit)
2. [Install the APK on the Oculus Quest](#install-the-apk-on-the-oculus-quest)
3. [Overview of the User Interface](#overview-of-the-user-interface)
5. [Documentation of the Unity Project](#documentation-of-the-unity-project)
6. [Third-Party Content](#third-party-content)

_*Note: Below, we often use the term player to refer to a user of the VRception Toolkit._

## Overview of the VRception Toolkit
The VRception Toolkit is a multi-user toolkit for quickly and easily prototyping Cross-Reality Systems in Virtual Reality. The concept behind the Toolkit is to simulate different realities in Virtual Reality (VR). Thereby, we enable users to rapidly prototype experiences across different realities. Simulated realities can be physical realities but also digital realities, such as AR, AV, or VR. By bringing different realities into VR, users can easily switch between them and remix their elements. With this, we also overcome the limitations of the physical world and reduce the effort necessary to prototype novel Cross-Reality Systems. As follows, we introduce the different prototyping environments the Toolkit offers and their respective workflows, and provide an overview of the implementation of the VRception Toolkit.

![Workflows and Environments of the VRception Toolkit.](/Figures/WorkflowsAndEnvironments.jpg?raw=true "Workflows and Environments of the VRception Toolkit.")

**Figure 2: Workflows and Environments of the VRception Toolkit. The Unity3D option is designed to maximize expert developers' ability to customized the Toolkit. The WYSIWYG mode allows developers that are not experts in Unity to experiment with Cross-Reality Systems; thereby, lowering the barrier to entry.**

We implemented the VRception Toolkit in [Unity3D (2020.1.8f1)](https://unity.com) using the [Oculus SDK](http://developer.oculus.com) providing users with the following major features:

* **Reality and Virtuality** To present different realities, we make use of multiple scenes, each holding one world that can be designed independently. Our implementation currently supports two realities, e.g., reality and virtuality (set by default) or virtuality-1 and virtuality-2. With our implementation, we can load any existing Unity3D scene as part of one of the two realities, allowing the reuse of existing projects. Additionally, we can have a shared scene containing shared objects that are visible in both realities, such as the player's avatar.
* **Interaction** Users have full control over the realities with their two controllers. Here, the left controller is mainly used to provide a virtual menu, which can be opened with a button on that controller. The menu contains a horizontal slider that allows users to transition between the two realities. Additionally, it contains  a set of predefined objects. The users can drag these objects into the scene, attach them to one another, or manipulate them to create more complex systems, objects, or structures. To directly manipulate objects, users can select them with their right controller and translate, rotate, scale, duplicate, or delete them.
* **Gradual Transition Between Realities** In our toolkit, a horizontal slider –- the Reality-Virtuality Slider –- allows users to transition between the two realities, with reality on the left side and virtuality on the right. The slider is a representation of the Reality-Virtuality Continuum [[1]](#references). Positioning the slider knob at one of the ends will render only one of the two realities. Between the extreme positions, transparency is applied to gradually blend all objects from all realities, depending on the position (cf. Figure 1). Each user has a slider to independently switch between realities and different glasses on their avatars show their current reality. Objects from shared scenes are always visible and unaffected by the slider. We implemented this with two stacked cameras (one for each reality) and transparency-compatible shaders attached to all objects.
Additionally, our toolkit supports individual blending or remixing of realities via a feature that we refer to as Experiences. Here, every experience can implement a highly customizable rendering of the different realities beyond well-known manifestations such as AR, AV, and VR. Such an Experience could, for example, render from one reality only the objects closer to the observer while rendering everything of the other reality unconditionally. In sum, users can transition between realities by switching through Experiences or adjusting the Reality-Virtuality Slider.
* **Predefined Objects** To empower users to quickly prototype cross-reality systems, we created an initial set of objects. While the objects in the virtual menu can be changed and extended easily, we decided for certain predefined objects as the default set of objects that ship with our prototyping tool (example below). To create objects inside the VR environment, users simply drag them from the menu into the currently selected reality, which is set by the Reality-Virtuality Slider. If the slider knob is more towards reality, objects spawn in reality, and vice versa. As an example, we implemented a display that allows one to bridge realities. While it exists in one reality, it shows the other. To realize the virtual displays, we use an additional camera that renders onto a texture attached to the display. To control the displays, users can adapt the position and direction of the camera independent of the display position. Both objects representing camera position and direction can also be attached to other objects.
* **Multiplayer** To enable multiple users to collaborate within the VRception Toolkit, we implemented network synchronization that keeps all clients in a consistent state. We used the [Photon Engine](https://photonengine.com) which allows up to 20 concurrent users (in the free version) without the need to host a dedicated server. Additionally, we implemented an in-game voice chat with 3D spatialized audio to allow collaborators to talk to one another using the Photon Voice feature.
* **Avatars** To represent collaborators in our VRception Toolit, we adapted a rigged character from the [Unity3D Asset Store](https://assetstore.unity.com/packages/3d/characters/humanoids/humans/liam-lowpoly-character-100007) (since the model is not free, it is remove from the source files, but can be added again after purchase). Moreover, we used inverse kinematic (IK) to map the controllers and headset to fitting poses of the avatar character. Specifically, we used the [FinalIK package](http://root-motion.com) (this model is also not free and thus, remove from the source files, but again, can be addeed after purchase). Last, we adjusted the shirt color and hairstyle to give each collaborator a unique look.
* **Real-world Scan** To increase the realism of the reality within our VRception Toolkit, we decided to include a 3D scan taken from a [private living room (Chalet in France)](https://skfb.ly/6ZynL). The advantage of such a real-world scan is that the scanning technology required for it has recently become available to more people (e.g., with the LIDAR sensors integrated in selected Apple products). Furthermore, compared to modeling with higher levels of realism, scanning can be done quickly and does not require any advanced skills, allowing developers to bring their own room into the VRception Toolkit.

## Showcases 
We re-implemented the six selected research prototypes; see Figure 3 for an overview of the reimplemented systems. Our goal was to see how easily we would be able to replicate them in the VRception Toolkit. We used the prototyping workflow A (Unity3D + WYSIWYG; see Figure 2). In our paper, you can find additional details on the reimplemented prototypes and read about expert interviews with the original paper authors about these reimplemented prototypes.

![Re-implementation of six selected cross-reality systems proposed in previous work.](/Figures/Showcases.jpg?raw=true "Re-implementation of six selected cross-reality systems proposed in previous work.")

**Figure 3: Re-implementation of six selected Cross-Reality Systems proposed in previous work. For each system, we present the re-implementation in the VRception Toolkit left and the original system right (right pictures taken from the original papers; cf. [citations](#references)).**

## Install the APK on the Oculus Quest
During our development, we tested the VRception Toolkit on the Oculus Quest 1 and 2. While more platforms exist (and the VRception Toolkit could be ported to these platforms), we thought that the standalone capabilites in combination with the wide distribution of the Quest headsets are good reasons to initially develop for them. In the Build folder, we offer two different APKs. Since we used some paid assets (mainly for 3D models and inverse kinematics; cf. [Third Party](#third-party-content)), we provide an APK with these paid assets and one without. The latter is compiled based on the code and assets available in this Github repository. Please be aware that for both APKs a multiplayer room is configured, meaning if others use the same APK at the same time, you will likely spawn in one multiplayer room with them (to configure a different room the APK needs to be recompiled). In the following, it is described how to deploy the app (avialable APKs) onto the Oculus Quest 1/2 via ADB or alternatively via Sidequest.

### Requirements
* Oculus App on the computer
* USB cable to connect the VR device to the computer
* Oculus Quest 1/2 headset

### Enable Developer Mode
1. Turn on the Oculus VR headset
2. Open the Oculus app (on your phone) and go to 'Settings'
3. Connect to your device and go to 'More Settings'
4. Enable the 'Developer Mode'

### How to Install an APK via ADB
1. Download and install ADB (Android Debug Bridge), for more info and instruction: https://developer.android.com/studio/releases/platform-tools and https://developer.android.com/studio/command-line/adb
2. Download one of the two APK files from the 'Build' folder
3. Open the CMD/Terminal and navigate to the <platform-tools> folder
4. Connect your VR headset with the USB cable and allow permission in Oculus, when asked
5. Check that the device is connected/listed with the command `adb devices`
6. Install the .apk file with `adb install <apk-path>`
  
### How to Install an APK via SideQuest
1. Download and install the SideQuest software from the [offical website](https://sidequestvr.com/setup-howto)
2. Download one of the two APK files from the 'Build' folder
3. Connect your VR headset with the USB cable and allow permission in Oculus, when asked
4. Start the SideQuest software and select 'Install APK file from folder on computer' represented as an icon in the top menu bar
5. Select the APK you want to install and SideQuest should automatically install it


## Overview of the User Interface
The user interface of the VRception toolkit is primarly controller-based. The only exception are some keyboard shortcuts that are available when the Toolkit is running in the Unity Player (see details below). In essence, the user interface supports four different modes, whereas each offers other functionality to the player. For each mode except the default mode, an overlay shows the currently active mode to the player.

* **Simulation Mode** is the default mode. The simulation is controlled via a virtual menu attached to the left controller. In this menu, players can transition on the Reality-Virtuality Continuum using a simple slider and add predefined objects (e.g., displays, projectors) from the menu to Virtuality or Reality to prototype Cross-Reality Systems.
* **Calibration Mode** allows players to map their physical reality to the virtual representation of reality in the VRception Toolkit. Please be aware that this requires a 3D scan or a remodeled virtual representation of the physical reality.
* **Configuration Mode** offers additional functionality to customize predefined objects. For example, displays have a camera component attached that can be customized in this mode.
* **Experience Mode** offers an alternative to the Reality-Virtuality slider, enabling players to experience custom remixes of the different spaces (e.g., specific manifestations of the Reality-Virtuality Continuum). Different than the slider, one experience describes one concrete manifestation. Players can switch between different experiences but the switch is not continous as it is for the crossfader. 

In the following, we show the tooltips of each mode (which are also available in-game via the 'Y'-button on the left controller). These mappings can be easily customized in the [Mapping.cs](/Assets/VRception/Scripts/Mapping.cs) script.

![Controller Bindings for Simulation Mode.](/Figures/MappingSimulationMode.jpg?raw=true "Controller Bindings for Simulation Mode.")

**Figure 4: Controller Bindings for Simulation Mode.**
  
![Controller Bindings for Calibration Mode.](/Figures/MappingCalibrationMode.jpg?raw=true "Controller Bindings for Calibration Mode.")

**Figure 5: Controller Bindings for Calibration Mode.**
  
![Controller Bindings for Configuration Mode.](/Figures/MappingConfigurationMode.jpg?raw=true "Controller Bindings for Configuration Mode.")

**Figure 6: Controller Bindings for Configuration Mode.**
  
![Controller Bindings for Experience Mode.](/Figures/MappingExperienceMode.jpg?raw=true "Controller Bindings for Experience Mode.")

**Figure 7: Controller Bindings for Experience Mode.**

When you start the VRception Toolkit in the Unity Editor, you can use some basic keybindings to control the Toolkit. For example, when you press the spacebar, the virtual menu will open and you can control the transition on the Reality-Virtuality Continuum with the keys 'A' and 'S.' Furthermore, you can switch between the different modes with the number keys (key '1' for Calibration Mode, key '2' for Configuration Mode, and key '0' for Experience Mode). For all implemented keybindings have a look ath the [Mapping.cs](/Assets/VRception/Scripts/Mapping.cs) script.
  

## Documentation of the Unity Project
We dedicated some time to provide an easy-to-use Unity project that enables everyone to quickly get started with the VRception Toolkit. But keep in mind that the Toolkit is a research artefact that cannot provide a highly detailed documentation and 24/7 support as you would expect from commercial software. Nevertheless, we provided comments in every script we developed and additionally adjusted the inspector view of the scripts developed to make them self-explanatory. Also, if you have trouble getting started or find bugs, please do not hesitate to contact us. We try to be as responsive as possible and will try to fix any bugs discovered.

### Setting Up the Unity Project
In the following, we provide some inital steps to get started with the Unity project of the VRception Toolkit. 
1. Download or clone this repository to your computer
2. Download and install the [Unity Hub](https://unity.com/download)
3. Start the Unity Hub and click on 'Installs' to install the Unity version 2020.1.8f1 (other versions may work as well but were not tested)
4. Go to 'Projects' in the Unity Hub and 'Add' this repository as a Unity project
5. Click on the Unity project in the Unity Hub to start it
6. In Unity, go to File > Build Settings and change the platform to Android (this will take some time)
7. Open the VRception scene in Project > Assets > VRception > Scenes > [VRception.unity](/Assets/VRception/Scenes/VRception.unity)
8. Congrats! You sucessfully loaded the Unity project of the VRception Toolkit
  
![The VRception Toolkit opened as a Unity project.](/Figures/UnityProject.jpg?raw=true "The VRception Toolkit opened as a Unity project.")

**Figure 8: The VRception Toolkit opened as a Unity project.**
  
### Settings of the VRception Toolkit
In the [VRception.unity](/Assets/VRception/Scenes/VRception.unity) scene, there are different gameobjects that offer certain settings to configure the VRception Toolkit.

* **General Settings** contains various settings that are quite central to the VRception Toolkit. It allows to configure the mode as well as the Reality-Virtuality slider. Moreover, one can select different headsets that represent the player's currently experienced manifestation of the Continuum. Furthermore, one can specify the predefined objects that are available for prototyping Cross-Reality Systems in-game. Examples of such predefined objects are the [simple display](Assets/VRception/Resources/Prefabs/SimpleDisplay.prefab) and the [simple projector](Assets/VRception/Resources/Prefabs/SimpleProjector.prefab). If you take a closer look at these prefabs, you will quickly learn how to create your own predefined objects and make them available in-game. In essence, every object in VRception that should be interactable requires the [Interactable](/Assets/VRception/Scripts/Interactable/Interactable.cs) script attached. To customize the functionality of an interactable, different modules are available, allowing users to translate, rotate, and scale them, for example. Simply add the modules to the prefab to extend the functionality.
* **Space Settings** allow to configure the available spaces in the VRception Toolkit. Fundamentally, reality and virtuality are both spaces and each can be represented by n Unity scenes. To keep the Toolkit generic, we refer to reality as left space and virtuality as right space, allowing customization to simulate two virtualities for example (virtuality-1 would be left space and virtuality-2 would be the right space). Besides the left and right space, there is also a shared space that is always visible (in reality as well as virtuality) and a default space that is never visible (mostly contains all the settings gameobjects). 
* **Experience Settings** enables users to configure the experiences available in-game. Experiences offer an alternative way to express custom manifestations of the Reality-Virtuality Contiuum (e.g., Augmented Reality). Users can implement their own experiences by deriviating from the abstract class [IExperience](/Assets/VRception/Scripts/Experiences/IExperience.cs) and adding the experience to the enum [Experience](/Assets/VRception/Scripts/Experiences/Experience.cs). Moreover, experiences allows users to define marker that are visible in-game and enable players to quickly jump to their position. These markers can be represented by an abstract camera symbol or by a character model to, for example, simulate a real-world bystander.
* **Network and Player Settings** allow users to select the name of the network room. The name should be unique as everyone using the same name is joining the same network room. Moreover, users can specify how many players should be in one room at maximum and which prefab should be used as the Player prefab. When integrating the paid package FinalIK (for inverse kinematics of VR player character based on headset and controller) or custom player character models, you need to alter the [Player](/Assets/Vrception/Resources/Player.prefab) prefab.
* **Logging and Replay Settings** empower users to specify a MongoDB for complete logging of all interactions carried out in the VRception Toolkit when in-game. The logged interactions can be used to analyze the user behavior (for example, in a user study) or to use for replay to view a logged session.


## Third-Party Content
To implement the VRception Toolkit, we used different software and assets provided by third parties. We had to use this third-party content because some of the benefits it offers are either to complex to quickly implement them ourselfs (e.g., networking) or are required to use (e.g., Oculus headset software). Unfortunately, not all third-party content is freely available and thus, cannot be offered with the source files. Nevertheless, the basic functionality of the VRception Toolkit is not limited by some missing software and assets. Moreover, we provide an APK of the complete VRception Toolkit including the paid content and made it easy to add the content back into the Unity project after buying it.
  
### Third-Party Content (freely available) - Included in This Repository
  
* Software: Oculus XR Plugin (https://docs.unity3d.com/Packages/com.unity.xr.oculus@1.4/manual/index.html)
* Software: Oculus Unity Integration (https://developer.oculus.com/downloads/package/unity-integration)
* Software: Photon Networking (https://www.photonengine.com/pun)
* Software: MongoDB (https://www.mongodb.com)
* Software: DNS Client (https://www.nuget.org/packages/DnsClient)
* Software: System Buffers (https://docs.microsoft.com/en-us/dotnet/api/system.buffers)
* Asset: Real-World Scan (https://sketchfab.com/3d-models/living-room-5621e1375b2d4ef1bdbba34aefd3fd36)
* Asset: City Street Skyboxes - Reality Skybox (https://assetstore.unity.com/packages/2d/textures-materials/sky/city-street-skyboxes-vol-1-157401)
* Asset: Simple Cumulus Skybox - Virtuality Skybox (https://assetstore.unity.com/packages/2d/textures-materials/sky/farland-skies-simple-cumulus-62565)
* Asset: Video "Reporter" (https://vimeo.com/343680002)
  
### Third-Party Content (paid) - NOT Included in This Repository
* Software: FinalIK (http://root-motion.com)
* Asset: Liam Character (https://assetstore.unity.com/packages/3d/characters/humanoids/humans/liam-lowpoly-character-100007)
* Asset: VR Headset Package (https://assetstore.unity.com/packages/3d/props/vr-headset-vol-1-161024)
* Asset: MR Headset Package (https://assetstore.unity.com/packages/3d/props/electronics/mr-headset-vol-1-urp-203805)

  
## References
[1] Paul Milgram and Fumio Kishino. 1994. A taxonomy of mixed reality visual displays. IEICE TRANSACTIONS on Information and Systems 77, 12 (1994), 1321–1329.
  
[2] Adalberto L. Simeone, Mohamed Khamis, Augusto Esteves, Florian Daiber, Matjaž Kljun, Klen Čopič Pucihar, Poika Isokoski, and Jan Gugenheimer. 2020. International Workshop on Cross-Reality (XR) Interaction. In Companion Proceedings of the 2020 Conference on Interactive Surfaces and Spaces (Virtual Event, Portugal) (ISS ’20). Association for Computing Machinery, New York, NY, USA, 111–114. https://dl.acm.org/doi/10.1145/3380867.3424551

[3] Christian Mai, Lukas Rambold, and Mohamed Khamis. 2017. TransparentHMD: Revealing the HMD User’s Face to Bystanders. In Proceedings of the 16th International Conference on Mobile and Ubiquitous Multimedia (Stuttgart, Germany) (MUM ’17). Association for Computing Machinery, New York, NY, USA, 515–520. https://dl.acm.org/doi/10.1145/3152832.3157813
  
[4] Mark McGill, Daniel Boland, Roderick Murray-Smith, and Stephen Brewster. 2015. A Dose of Reality: Overcoming Usability Challenges in VR Head-Mounted Displays. In Proceedings of the 33rd Annual ACM Conference on Human Factors inComputing Systems. Association for Computing Machinery, New York, NY, USA,2143–2152. https://dl.acm.org/doi/10.1145/2702123.2702382

[5] Jan Gugenheimer, Evgeny Stemasov, Julian Frommel, and Enrico Rukzio. 2017. ShareVR: Enabling Co-Located Experiences for Virtual Reality between HMD and Non-HMD Users. In Proceedings of the 2017 CHI Conference on Human Factors in Computing Systems. Association for Computing Machinery, New York, NY,USA, 4021–4033. https://dl.acm.org/doi/10.1145/3025453.3025683
  
[6] Chiu-Hsuan Wang, Seraphina Yong, Hsin-Yu Chen, Yuan-Syun Ye, and LiweiChan. 2020. HMD Light: Sharing In-VR Experience via Head-Mounted Projector for Asymmetric Interaction. In Proceedings of the 33rd Annual ACM Symposium on User Interface Software and Technology (Virtual Event, USA) (UIST ’20). Association for Computing Machinery, New York, NY, USA, 472–486. https://dl.acm.org/doi/10.1145/3379337.3415847
  
[7] Lung-Pan  Cheng,  Eyal  Ofek,  Christian  Holz,  and  Andrew  D.  Wilson.  2019. VRoamer: Generating On-The-Fly VR Experiences While Walking inside Large, Unknown Real-World Building Environments. In 2019 IEEE Conference on Virtual Reality and 3D User Interfaces. IEEE, Piscataway, New Jersey, United States, 359–366. https://ieeexplore.ieee.org/document/8798074

[8] Pascal Jansen, Fabian Fischbach, Jan Gugenheimer, Evgeny Stemasov, Julian Frommel, and Enrico Rukzio. 2020. ShARe: Enabling Co-Located Asymmetric Multi-User Interaction for Augmented Reality Head-Mounted Displays. In Proceedings of the 33rd Annual ACM Symposium on User Interface Software and Technology (Virtual Event, USA) (UIST ’20). Association for Computing Machinery, New York, NY, USA, 459–471. https://dl.acm.org/doi/10.1145/3379337.3415843
