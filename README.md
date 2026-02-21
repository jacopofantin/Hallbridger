# Hallbridger

This component enables communication between facility management systems of multipurpose halls (starting one is CURIO System - by Decima 1948, Padua, Italy -, the software installed in the "Roberto de Silva" theater in Rho, Milan, Italy), and the 3D model of the hall.

This readme file describes how the two input files (the one with real hall data and the one with the 3D hall) should be structured in order for them to be correctly read, and information about the configuration file (note: custom preferences should not be specified editing the configuration file. The user should navigate to the "Options" menu in the software interface and tweak the options in the "Configurations" window, instead. The hereby description has illustration and documentation purposes only).





CONFIGURATION FILE (where custom preferences are saved to)

- Name: conf
- Extension: .ini
- Location (relative to directory where the main Hallbridger folder is placed in the theater PC): Hallbridger\Configuration\
- Structure to follow:
  - Sections are defined in square brackets
  - Each section has a number of key-value pairs separated by equal sign =
  - Values are read as strings but they don't need double quotes to be defined (just write the values straight, see .ini file for example)

Section description:
1) [RealHallFileCheck]: controls the timer for automatic reading of the file containing data coming out of the real hall
   Keys:
   - Directory: specifies the path of the directory where the file to be read/updated relative to this timer is placed, see default value for each timer in the corresponding section of the present file)
   - FileName: specifies the name (including file extension) of the file to be read/updated relative to this timer, see default value for each timer in the corresponding section of the present file)
   - Interval: integer number specifying the amount of time (in milliseconds) passing from one file check to the next one performed by the timer
   - Active: boolean value indicating whether the timer is active or not (write true if active, false if inactive)
2) [Hall3DModelFileCheck]: controls the timer for automatic reading and updating of the file containing the 3D model of the theater hall
   Keys:
   - Directory: specifies the path of the directory where the file to be read/updated relative to this timer is placed, see default value for each timer in the corresponding section of the present file)
   - FileName: specifies the name (including file extension) of the file to be read/updated relative to this timer, see default value for each timer in the corresponding section of the present file)
   - Interval: integer number specifying the amount of time (in milliseconds) passing from one file check to the next one performed by the timer
   - Active: boolean value indicating whether the timer is active or not (write true if active, false if inactive)
   - Operation: available for the 3D model file only. Specifies the operation that must be performed with the 3D model file. Can take values "Load" or "Update" only
3) [AutomaticDiscrepancyHighlightingOption]: controls the automatic highlighing of discrepancies between data of real and 3D hall
   Keys:
   - Enabled: boolean value indicating whether the option is enabled or not (write true if enabled, false if disabled)






REAL HALL INPUT FILE

- Related to timer controlled by section [RealHallFileCheck] of the INI file
- Name: Real_hall_snapshot (default)
- Allowed extensions: .txt
- Location (relative path of directory where the main Hallbridger folder is placed in the theater PC): Hallbridger\IO_files\ (default)
- Allowed data models (allowed facility management systems): CURIO data model

File description (data model -> extension):
- CURIO
  - TXT
    - The file consists of three blocks of data, one block per moving element group and each with one line per moving element
    - Each of these lines must begin following the format: "XX.###   *****", where:
      - XX is an alphabetic identifier for the piece of equipment/panel array (e.g. "PS" for "Pannello Sinistra", "PD" for "Pannello Destra")
      - ### is a numeric identifier for the piece of equipment/panel array
      - ***** is an integer number in millimeters determining the position of the piece of equipment/aperture of the panel array (for block 1, it corresponds to the vertical distance of the piece of equipment starting from the top). It must contain 5 digits, so zeros must be added at the beginning if the number has less than 5 digits
    - File structure to follow:
      - First 3 lines have no theater information
      - Following 13 lines (block 1) contain information about stagecraft equipment positions
      - Following line has no information
      - Following 26 lines (block 2) contain information about pivoting panel arrays on the left wall (alphabetic ID: PS ("Pannello Sinistro"))
      - Following line has no information
      - Following 26 lines (block 3) contain information about pivoting panel arrays on the right wall (alphabetic ID: PD ("Pannello Destro"))




3D HALL INPUT FILE (3D model)

- Related to timer controlled by section [Hall3DModelFileCheck] of the INI file
- Name: 3D_hall_model (default)
- Allowed extensions: .ifc
- Location (relative path of directory where the main Hallbridger folder is placed in the theater PC): Hallbridger\IO_files\ (default)
