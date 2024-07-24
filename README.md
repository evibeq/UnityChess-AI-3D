# UnityChessAI-3D ♟️
This fork is a version of [SebLague](https://github.com/SebLague)'s project, introduced with [this video](https://www.youtube.com/watch?v=U4ogK0MIzqk), which includes an additional 3D graphics component. All new scripts are based on those defined to manage 2D in the original version and have the same names with the addition of the letters "3D". All new files are located inside the "3D" folder, except for the new scene in the "scenes" folder.

The part managing the input with the mouse, including selecting and dragging the pieces, was implemented with the help of a raycast.

The creation and management of 3D objects are almost identical to those for 2D, except for the coordinates. Based on existing code, the pieces are instantiated and destroyed with every move.

UPDATE: In addition to the 3D version of the scripts, there is now a version called "plus" which avoids the continuous cycle of Destroy() and Instantiate() to manage the pieces. Now, during a game, the prefabs are only moved!

![Alt text](Assets/Screenshots/image.png?raw=true "Screenshot")
