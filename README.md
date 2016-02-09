# PropertyPopup
Property handling in popup menu

Common feature of the CAD softwares, you have to use the toolbar buttons to control them, and to change the selected object's properties a ‘property grid’ like something for aid. Old-time users handling their softwares with key-combinations. In both situations the work flow blocked because they mousing needless kilometers or they looking the keyboard for commands which are huge amount a waste of time.

In the develop of Building Patterns that’s why was very important, to be a position sensitive popup menu to control the CAD objects and set the properties. Obviously it wasn’t a problem to put the functions into a menu, but had some time to effectively handling the properties. The solution looked like on hand, to show the Property Grid in menu as it looks like on training videos (at the system’s web page) but I looked for a more logical possibility too.

Now I have a solution for this, and I present it.

In the test environment the drawing surface is a PictureBox also we have 2 classes, the Ellipse and the Rectangle, to show how it works. At the initialization of the program some objects shown up, above the objects and drawing place with a right-click appears the place sensitive popup menu.
The PropertyManager class making the menu elements from the object’s properties.
I am taking the properties with a “foreach”, except the “Browsable = False” and those which doesn’t contain “[Description…]” attribute. I am dividing the basic attributes in a “switch”, selecting the own classes by name.

We can handle some properties yet.
On some properties the “[Description…]” are out commented, because showing the full palette takes too long (2 secs) but it is worth to try.
Connected to this I would like to know if somebody can suggest a solution to the menu which like in the PropertyGrid? I mean the right pointing arrow should  stay, but the loading of the whole color palette should be done when the menu element have been pushed.
Also I show the realization of the “property changed” event at the Penwidth attribute. It is like I call an “Invalidate” method in part of the attribute “set” which redraws the PictureBox. We need this, because only after the redraw can shows the change.

My solution is not complete yet as some of the attribute control wasn’t implemented, but still it’s usable. I hope will be somebody who can find useful and will use in programming practice.
I would like to know your opinions and I count on receiving some suggestions to make this solution better and more effective.
