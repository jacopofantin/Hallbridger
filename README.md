# CURIOsity

This component enables communication between CURIO (a facility management software installed in the multipurpose hall "Roberto de Silva" in Rho, Milan, Italy), through a TXT file, and the BIM model of the theater, through an IFC file.

This readme file describes how the TXT and IFC file should be structured in order for them to be correctly read, and how to customize the application behavior through an appropriate INI file.





CONFIGURATION FILE (to customize app behavior)

Name: conf

Extension: .ini

Location (relative to directory where the main CURIOsity folder is placed in the theater PC): CURIOsity\Configuration\

Structure to follow:

Sections are defined in square brackets
Each section controls a timer for automatic data reading and updating
Each section has a number of key-value pairs separated by equal sign =
Values are read as strings but they don't need double quotes to be defined (just write the values straight, see .ini file for example)
The keys (i.e. the customizable parameters for each timer) are the following:
- Directory: specifies the path of the directory where the file to be read/updated relative to this timer is placed, see default value for each timer in the corresponding section of the present file)
- FileName: specifies the name (including file extension) of the file to be read/updated relative to this timer, see default value for each timer in the corresponding section of the present file)
- Interval: integer number specifying the amount of time (in milliseconds) passing from one file check to the next one performed by the timer
- Active: boolean value indicating whether the timer is active or not (write true if active, false if inactive)

First section (i.e. first timer) is about the text file containing data coming out of CURIO (section [TextFileCheck])
Second section (i.e. second timer) is about the BIM file containing the 3D model of the theater hall (section [BimFileCheck])





INPUT TEXT FILE (CURIO data)

- Related to timer controlled by section [TextFileCheck] of the INI file
- Name: Fotografia_sala_CURIO (default, can be changed through .ini file, see corresponding section of the present file)
- Extension: .txt
- Location (default, can be changed through .ini file, see corresponding section of the present file; path is relative to directory where the main CURIOsity folder is placed in the theater PC): CURIOsity\IO_files\

The file consists of three blocks of data, one block per moving element group and each with one line per moving element
Each of these lines must begin following the format: "XX.###   *****", where:
- XX is an alphabetic identifier for the piece of equipment/panel array (e.g. "PS" for "Pannello Sinistra", "PD" for "Pannello Destra")
- ### is a numeric identifier for the piece of equipment/panel array
- ***** is an integer number in millimeters determining the position of the piece of equipment/aperture of the panel array (for block 1, it corresponds to the vertical distance of the piece of equipment starting from the top). It must contain 5 digits, so zeros must be added at the beginning if the number has less than 5 digits

File structure to follow:

First 3 lines have no theater information
Following 13 lines (block 1) contain information about stagecraft equipment positions
Following line has no information
Following 26 lines (block 2) contain information about pivoting panel arrays on the left wall (alphabetic ID: PS ("Pannello Sinistro"))
Following line has no information
Following 26 lines (block 3) contain information about pivoting panel arrays on the right wall (alphabetic ID: PD ("Pannello Destro"))





INPUT BIM FILE (theater hall 3D model)

- Related to timer controlled by section [BimFileCheck] of the INI file
- Name: model (default, can be changed through .ini file, see corresponding section of the present file)
- Extension: .ifc
- Location (default, can be changed through .ini file, see corresponding section of the present file; path is relative to directory where the main CURIOsity folder is placed in the theater PC): CURIOsity\IO_files\
