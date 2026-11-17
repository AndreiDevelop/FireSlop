# Drone flight and walkable deck

Updated 2026-11-17. Scope covers step 15. The trampoline is deferred.

The connected drone prefab in Main has a 1.90 × 2.10 m walkable deck with the four supports spread around the center passage. Boarding requires standing on the deck. E switches between ground movement and flight, with the same resident shoulder attachment. The flight camera sits behind the drone, supplies the hose aim, and steers the body yaw when the flight volumes have room to turn. Touch altitude controls are authored in the existing HUD prefab.

The initial hover delay is 10 seconds in [Flight.asset](../../../Assets/SO/Drone/Flight.asset). It starts when the pilot exits flight control. Descent starts once the delay has elapsed and the firefighter has stepped off. An aligned drone enables automatic capsule clearance and sill stepping at an open window.

Unity Play Mode checks exercised ground boarding through CharacterController movement, the camera switch, ascent with pilot movement, and supported exit. Scripted movement through the cleared cabinet-room window passed in both directions after aligning the deck about 30 cm above the opening guard's lower edge to clear the roof parapet. The outward crossing and boarding passed with the resident attached to the shoulder. Moving the piloted pair into the actual rescue volume triggered victory, and the authored Restart button loaded an active mission. With a simulated touchscreen, both HUD altitude buttons published and released the correct input. The drone held height while occupied and during the departure delay, then descended once the empty-delay gate expired.

The Editor checks arranged docking and delivery positions directly. A continuous manual flight from the yard to the window and through delivery, reticle and hose feel, HUD screenshots across aspect ratios, and a physical phone playthrough remain open.
