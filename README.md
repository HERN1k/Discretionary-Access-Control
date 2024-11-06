<h2 align="center">🎓 Discretionary Access Control</h2>

This project was created for laboratory work on the topic 

`Implementation of a discretionary access control mechanism`

The basis is the model of discretionary management 

`Object-oriented model of isolated software environment`

<div align="center">
  <img alt="Home" src="/Src/home.png" />
</div>
<p></p>

What modules are in the program?

  1. A module for `registration` and `authorization` of subjects

  2. A unique `object identification` module

  3. Methods that allow `subjects` to perform `object` operations

  4. An `administrator` module has been created in which subjects, objects and rights are managed

  5. A module to check the access right that wants to get a subject to an object in `real time`

  6. A `event audit` module that logging: events, failed accesses and successful operations

<div align="center">
  <img alt="Log" src="/Src/log.png" />
  <img alt="Help" src="/Src/help.png" />
</div>

## 🛠 Getting Started and Installation

Clone this repository. You will need to install `.NET SDK` and `Git` globally on your machine
<p></p>

1. Clone repository: `git clone https://github.com/HERN1k/Discretionary-Access-Control.git`

2. Go to the project folder:
   ```bash
   cd Discretionary-Access-Control
   
3. Installation dependencies:
   ```bash
   dotnet restore
   dotnet build

4. End you can run: 
   ```bash
   dotnet run
