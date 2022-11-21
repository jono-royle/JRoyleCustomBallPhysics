# JRoyleCustomBallPhysicsGit

Unity 2021 project using Unity rigidbody collision detection and custom physics simulation of ball trajectories.

Controls:
Move mouse to aim, hold left mouse button and release to fire. The longer the button is held, the faster the ball will be launched.

Up/down arrows: Increase the backspin/topsin

Left/right arrows: Increase the sidespin

The following public variables can be adjusted on the BallController game object:
- Ball launch speed: the value is how fast the ball will be launched in m/s if the mouse button is held for 1 second
- Ball diameter in meters
- Ball mass in kg
(Note: lighter than air balls will have their mass increased to have the same density as air, to prevent issues imparted by the drag force being too high when low density balls are given an instantaneous velocity
and avoid having to deal with buoyancy)
- Drag coefficient: a property of the ball that determines how much air resistance it encounters
- Restitution coefficient: how much energy the ball conserves on a bounce
- Gravity in m/s^2 
- Air density in kg/m^3 (affects the air resistance drag force and the magnus effect lift force on the ball)
- Air viscosity in cSt (affects the torque the ball generates against the air while spinning)

This simulation models balls trajectory through the air and handles collisions with obstacles and collisions between balls. The physics simulation models gravity and air resistance.
If the ball is given spin by the player, the model also simulates magnus effect forces which affect the trajectory, torque from the air which slows the rate of spin and increased air resistance due to the spinning.
Collisions with obstacles also reduce the spin velocity, and impart linear velocity based on the spin e.g a ball with backspin will gain some backwards velocity when it bounces off the ground.

This provides a good approximation of ball trajectories in a variety of conditions. Some areas where the model could be expanded where I to keep working on it:
- As tennis balls can deform, their spin velocity not only decreases on a collision but can also change direction as friction is applied from the ground. Currently my model assumes spin direction does not change after firing,
e.g more like an inflexible ping pong ball
- Currently balls in the simulation which have finished bouncing and are resting upon the ground slide. In reality the ground would apply friction opposite to the direction of movement causing balls to roll 
- In the simualtion balls collide and bounce off each other, but slow moving or stationary balls can intersect. This could be improved by checking upon each collision and pushing the balls apart.

Sources for formulas
- http://www.physics.usyd.edu.au/~cross/TRAJECTORIES/42.%20Ball%20Trajectories.pdf
- https://en.wikipedia.org/wiki/Reflection_(mathematics)
- https://en.wikipedia.org/wiki/Coefficient_of_restitution
- https://exploratoria.github.io/exhibits/mechanics/elastic-collisions-in-3d/index.html
- http://hyperphysics.phy-astr.gsu.edu/hbase/isph.html
- https://file.scirp.org/Html/1-4900488_77588.htm
- http://hobbieroth.blogspot.com/2018/01/the-viscous-torque-on-rotating-sphere.html

Source for ball image
https://www.robinwood.com/Catalog/FreeStuff/Textures/TexturePages/BallMaps.html
