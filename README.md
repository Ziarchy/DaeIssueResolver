# DaeIssueResolver
Corrects issues where dae files have multiple UVs per mesh and maybe other stuff

How to use

Download repo from github
Open the deaIssueResolver.sln file using visual studio or your preferred IDE.
Navigate in file explorer to ~DaeIssueResolver\deaIssueResolver\bin\Debug\net6.0 (if this path doesnt exist yet, just run the program in Visual studio and it will create it)
Inside the net6.0 folder create 2 folders, one called "input" and the other called "output" (these are case sensitive) (do not include the quotes in the name)
Copy the .dae file you want to correct into the "input" folder (make sure this is the only file in the folder)
Inside visual studio, open the Program.cs file
On line 120, change the text in the quotes to whatever you would like your dae file to be named after it is finished processing
Run the program
