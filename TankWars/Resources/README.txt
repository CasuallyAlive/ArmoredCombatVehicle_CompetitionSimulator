Authors: Jordy A. Larrea Rodriguez and Ryan Garcia 

Version History:
===========================

Ps8
===========================
03/30/2021: TankWars: V1.0 - Set up the tankwars solution and started implementing Game Controller to expected specifications. Used MVCChatController as a reference for design decisions
			TankWars: V1.1 - Added/implemented Beam and PowerUp classes in Model project. In addition, implemented Wall class. TODO: everything else.

03/31/2021: TankWars: V1.2 - Imlemented World class and modified Beam, PowerUp, Projectile, Tank, and Wall classes with getters for IDs and alive booleans.
			TankWars: V1.2.1 - Started handshake protocol in controller method. 

04/01/2021: TankWars: V1.3 - Implemented logic in controller that updates objects of the world with new objects from the server.  Added logic for initial handshake such that the player ID and world size are received without issue; similarily, walls are processed only once during the handshake process.
			TankWars: V1.3.1 - Implemented ControlCommand class within GameController, implemented input command methods to update movement, firing, and orientation.

04/04/2021	TankWars: V1.4 - TankWars: V 1.4 - Began implementing the view for the tankwars client.

04/05/2021	TankWars: V1.4.1 - Implemented a drawingpanel class that encapsulates the drawing logic for the view GUI for this tankwars client. Added method stubs for input handlers.
			TankWars: V1.4.2 - Refined drawingpanel class (centering on player, receiving data from server etc.), began View update process with server information.
			TankWars: V1.4.3 - Fixed bugs associated with the handshake procedures of this client and tested movement commands alongside basic drawing testing with more than one tank in a given server.
		
04/06/2021	TankWars: V1.4.4 - Fixed some bugs with drawing walls. Walls are still somewhat off with the servers version.
			TankWars: V1.4.5 - Fixed choppy movement, and began implementing logic associated with loading images.

04/07/2021	TankWars: V1.5.0 - Fixed the wall coordinate bugs (endpoints and not corner coordinates). Loaded and now draw walls, tanks, and turrets with sprite images.
			TankWars: V1.5.1 - Began implementing logic to load wall sprites on segments of walls. Added default text to textboxes. Tanks now despawn when killed. 

04/07/2021	TankWars: V1.5.2 - Added turret rotation, power-up drawing, and mouse-movement detection.
			TankWars: V1.5.3 - Projectiles are now drawn. Fixed a bug that caused all of a players beams to be fired sequentially. The view has been updated such that there is an error pop up window whenever an error is encountered. Still need to add beam and explosion animations. Walls still need to be drawn correctly. 

04/08/2021	TankWars: V1.6.0 - Wall drawing is now fixed and is done without incrementing over a length of wall.
			TankWars: V1.6.1 - Added in player names and health bars for each individual tank.
			TankWars: V1.6.2 - Added the beam animation and began polishing movement controls.
			TankWars: V1.6.3 - Reverted movement changes to Queueing two movement commands.

04/09/2021	TankWars: V1.7 - Added explosion animation and implemented handlers for a disconnection event.  
			TankWars: V2.0 - Added Control and About help menus and fixed numFrames increment logic for animations. Ready for release.

Ps9
===========================
04/13/2021	TankWars-Server: V1.0 - Laid the foundation for the server aspect of the TankWars game, i.e. implemented the initial stages of the handshake and handling for server settings.
			TankWars-Server: V1.1 - Implemented server settings file reading.

04/14/2021	TankWars-Server: V1.2 - Began implementing networking structure in the server controller. 
			TankWars-Server: V1.3 - Began implementing handshake process, finalizing a connection etc.

04/15/2021	TankWars-Server: V1.4 - Resolved race conditions when adding clients to server, i.e. this server can now accept multiple connections simultaneously. Previous connections are removed when disconnected. Tanks now spawn at random locations in the world, including in walls! Added events that allow the server-app to display when a client has connected/disconnected.
			TankWars-Server: V1.5 - ControlCommand was moved to a separate project and now the server can receive control commands from the client. Movement is currently the only recognized command. Clients are seemingly disconnected at random still and this must be fixed.

04/16/2021	TankWars: V1.6 - Fixed bugs in our wall class, i.e. walls had properties that we didn't want serialized, well serialized. Modified how the server processes commands. Commands are now only processed once per frame. 
			TankWars: V1.7 - Added tank orientation changing along with movement, as well as turret orientation changing with the mouse. This causes major lag when more than one client is connected so this needs to be fixed.

04/19/2021	TankWars: V1.7.1 - Began implementing collision logic.
			TankWars: V1.8 - Tank and Wall collision now works.

04/20/2021	TankWars: V1.8.1 - Projectiles are now fired at rates determined by the rules of the server and projectiles cease to exist when they interact with walls. 
			TankWars: V1.9.0 - Projectiles now collide and can kill tanks, powerups now spawn in once every cycle of the frame limit and are maxed out defined by ServerSettings. TODO: Make sure scores update on tank kills.

04/21/2021	TankWars: V1.9.5 - Implemented logic that keeps score of a given tank's kills and beam stockpile. Beams are now processed by the server. Furthermore, tanks are now removed upon disconnection. TODO - Implement collision logic for beams and the extra feature. 
			TankWars: V2.0.0 - Finalized beam collision, now kills tanks instantly and updates player scores. World wraparound for tanks is now implemented, you are unable to wraparound if a wall exists on the other side of the world. Projectiles now die when hitting the world border.

04/22/2021	TankWars: V2.1.0 - Fixed a bug that failed to respawn tanks or failed to suppress movement commands on death in the case that a tank was shot and kill  by two tanks at the same time (negative hp). Began implementing the extra game mode, titled "Drunk Tanks Mode".
			TankWars: V2.2.0 - Added "Drunk" mode. When a tank activates a powerup in drunk mode speed increases by a factor of 3, controls are reversed, and tanks fire many shots in all directions.
Polish:
===========================
Added queue for movement so client can process up to two keys at a time. Allows for smoother transition between key presses making movement less choppy.
Added unique explosion animation, courtesy of Sinestesia. https://sinestesia.itch.io/

Server (Extra Game Mode):
	- Designed an extra game-mode that modified the mechanics of the beam-powerUp system. We named our game-mode, "Drunk Mode", after the "effects" that are experienced by players after activing the ability (ability is activated by right-clicking, identically to firing a beam).
	  Effects experienced by a player upon activation of the ability include increased speed (3 times speed), improved manueverability (inverted controls), and increased firepower. 